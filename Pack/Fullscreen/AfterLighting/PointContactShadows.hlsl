#define ANOMALY_PACK_SRV0_TYPE Texture2D<float>
#include <AnomalyFullscreen.hlsli>
#include <AnomalyPointShadows.hlsli>
#include <AnomalyContactShadows.hlsli>
#include <Lighting/Brdf.hlsli>
#include <VertexTransformations.hlsli>

#define Intensity     AnomalyPassUniform0.x
#define Thickness     AnomalyPassUniform0.y
#define MaxDistance   AnomalyPassUniform0.z
#define StepCount     AnomalyPassUniform0.w
#define MaxLights     AnomalyPassUniform1.x
#define DebugOverlay  AnomalyPassUniform1.y
#define UseOccupancy  AnomalyPassUniform1.w
#define UseAtlas      AnomalyPassUniform2.x
#define ShadowSource  AnomalyPassUniform2.y
#define DebugView     AnomalyPassUniform2.z
#define DebugAtlasRow AnomalyPassUniform2.w
#define SingleLight   AnomalyPassUniform3.x

#define AnomalyPlayerDepth       AnomalyPackSrv0
#define AnomalyOccupancy         AnomalyPackSrv1
#define AnomalyPointShadowAtlas  AnomalyPackSrv2

#define MAX_STEPS 192
// PrepareLights fills tiles in race order. frame_.Light.maxTileLights is the
// global list stride (256–4096), not a per-tile cap. Eval BRDF for many;
// march the brightest MaxLights by photometric energy.
#define MAX_EVAL 64
#define MAX_MARCH 12
#define MAX_OCC_STEPS 16
#define MAX_CUBE_LIGHTS 64
#define TILE_SIZE 16
#define OCC_DIM 64
#define OCC_VOXEL 2.0
#define OCC_ORIGIN float3(-64.0, -64.0, -64.0)
// Photometric range still lights. Marches may continue ~1.4× so vis is
// not a hard sphere. IsolatedSub energy is photometric only (no tail).
#define SHADOW_RANGE_SCALE 1.4
float2 GetAttenuation(float distance, float range, float fallOff)
{
    float d = distance / range;
    float dq = d * d;
    float dq1 = 1.0 - dq * dq;
    float attenuationHard = dq1 * dq1 * dq1;
    float attenuationDiffuse = 1 / (1 + fallOff * dq);
    return float2(attenuationHard, attenuationDiffuse);
}

// Clone of LightPoint.hlsl: no NdotL cull. MaterialRadiance saturates ln.
float3 PointUnshadowedRange(AnomalyPointLight light, float3 viewPos, float3 N, float3 V,
    float3 albedo, float3 f0, float gloss, float ao, float range)
{
    float3 toLight = light.positionView - viewPos;
    float distance = length(toLight);
    if (distance >= range || distance < 1e-4)
        return 0;
    float3 L = toLight / distance;
    float2 attenuation = GetAttenuation(distance, range, light.fallOff);
    float3 lightFactor = attenuation.x * light.color * ao;
    if (dot(lightFactor, lightFactor) <= 1e-7)
        return 0;
    float3 H = normalize(V + L);
    return lightFactor * MaterialRadiance(albedo, 1, f0, gloss * light.glossFactor,
        N, L, V, H, light.diffuseFactor * attenuation.y);
}

float3 PointUnshadowed(AnomalyPointLight light, float3 viewPos, float3 N, float3 V,
    float3 albedo, float3 f0, float gloss, float ao)
{
    return PointUnshadowedRange(light, viewPos, N, V, albedo, f0, gloss, ao, light.range);
}

float ShadowRange(AnomalyPointLight light, float maxDist)
{
    return max(light.range * SHADOW_RANGE_SCALE, maxDist);
}

bool PointLightDir(AnomalyPointLight light, float3 viewPos, float shadowRange, out float3 L, out float distance)
{
    float3 toLight = light.positionView - viewPos;
    distance = length(toLight);
    if (distance >= shadowRange || distance < 1e-4)
    {
        L = 0;
        return false;
    }
    L = toLight / distance;
    return true;
}

float2 ViewToUv(float3 viewPos)
{
    // GAP: Slice AI — unjittered view + jitter-matched depth sample.
    // AnomalyViewToLightingUv of a jittered reconstruct crawls with Halton.
    return AnomalyViewToDepthUv(viewPos);
}

// InvViewAt0 rows (same expansion as Aurora / clouds). mul(M, v) transposes.
float3 ViewToWorldAt0(float3 viewPos)
{
    return mul(viewPos, (float3x3)frame_.Environment.inv_view_matrix);
}

float OccupancyAt(float3 worldAt0)
{
    float3 cellF = (worldAt0 - OCC_ORIGIN) / OCC_VOXEL;
    if (any(cellF < 0) || any(cellF >= OCC_DIM))
        return 0;
    uint3 cell = uint3(cellF);
    uint2 tile = uint2(cell.z % 8, cell.z / 8);
    uint2 px = tile * OCC_DIM + cell.xy;
    return AnomalyOccupancy.Load(int3(px, 0)).r;
}

float MarchOccupancy(float3 viewPos, float3 lightDir, float marchDist)
{
    // 2 m voxels fill floors. Hit only after leaving the start solid, then
    // re-entering another cell (a box around a corner), not the same slab.
    float3 pStart = ViewToWorldAt0(viewPos);
    bool leftSolid = OccupancyAt(pStart) < 0.5;
    float3 p0 = ViewToWorldAt0(viewPos + lightDir * OCC_VOXEL);
    float3 p1 = ViewToWorldAt0(viewPos + lightDir * marchDist);
    float vis = 1.0;
    [loop]
    for (uint s = 1; s <= MAX_OCC_STEPS; s++)
    {
        float t = (float)s / (float)MAX_OCC_STEPS;
        float3 p = lerp(p0, p1, t);
        bool occ = OccupancyAt(p) > 0.5;
        if (!occ)
            leftSolid = true;
        else if (leftSolid)
        {
            vis = 0;
            break;
        }
    }

    return vis;
}

uint2 AtlasSize()
{
    uint w, h;
    AnomalyPointShadowAtlas.GetDimensions(w, h);
    return uint2(w, h);
}

float SampleCubeRow(uint row, float3 dir, float dist)
{
    return AnomalyPointShadowVisibility(AnomalyPointShadowAtlas, row, dir, dist, 0.04);
}

float CubeVisibility(float3 viewPos, float3 lightViewPos, float range, out bool capturedCharacter)
{
    capturedCharacter = false;
    uint2 atlasSize = AtlasSize();
    if (atlasSize.y < 2)
        return 1;
    uint faceRes = max(atlasSize.x / 6, 1);
    uint count = min((atlasSize.y - 1) / faceRes, MAX_CUBE_LIGHTS);
    float3 toPoint = viewPos - lightViewPos;
    float dist = length(toPoint);
    if (dist < 1e-4 || dist > range)
        return 1;
    // Atlas faces are anchored to world axes, independent of camera rotation.
    float3 dir = ViewToWorldAt0(toPoint / dist);
    [loop]
    for (uint i = 0; i < MAX_CUBE_LIGHTS; i++)
    {
        if (i >= count)
            break;
        float4 slot = AnomalyPointShadowAtlas.Load(int3(i, 0, 0));
        if (slot.w < 0.05)
            continue;
        if (distance(slot.xyz, lightViewPos) < 0.001 && abs(slot.w - range) < 0.001)
        {
            capturedCharacter = atlasSize.x >= 66 && AnomalyPointShadowAtlas.Load(int3(65, 0, 0)).w == 1;
            return SampleCubeRow(i, dir, dist);
        }
    }

    return 1;
}

void InsertTop(inout float topLuma[MAX_MARCH], inout uint topEval[MAX_MARCH],
    uint marchCap, float score, uint evalI)
{
    if (score <= 1e-6)
        return;
    uint at = marchCap;
    [loop]
    for (uint k = 0; k < MAX_MARCH; k++)
    {
        if (k >= marchCap)
            break;
        if (score > topLuma[k])
        {
            at = k;
            break;
        }
    }
    if (at >= marchCap)
        return;
    [loop]
    for (int s = (int)marchCap - 2; s >= (int)at; s--)
    {
        topLuma[s + 1] = topLuma[s];
        topEval[s + 1] = topEval[s];
    }
    topLuma[at] = score;
    topEval[at] = evalI;
}

float4 DebugColor(float linearDepth, uint numLights, float3 removed, float3 unshadowed, float minVis)
{
    if (linearDepth < 1e-4)
        return float4(0.72, 0.08, 0.85, 1);
    if (frame_.Light.maxTileLights == 0 || frame_.Light.tiles_x == 0)
        return float4(0.08, 0.55, 0.95, 1);
    if (numLights == 0)
        return float4(0.12, 0.22, 0.82, 1);

    float energy = dot(unshadowed, 1.0 / 3.0);
    float removedLuma = dot(removed, 1.0 / 3.0);
    // Yellow: occluded but almost nothing to IsolatedSub (self-hit / tiny energy).
    if (minVis < 0.5 && energy < 0.02 && removedLuma < 0.01)
        return float4(0.95, 0.82, 0.10, 1);
    float3 lit = float3(0.08, 0.78, 0.22) * saturate(energy * 2.0 + 0.15);
    float3 shadow = float3(1.0, 0.12, 0.04) * saturate(removedLuma * 4.0 + 0.15);
    return float4(lerp(lit, shadow, saturate(removedLuma * 3.0)), 1);
}

float4 __pixel_shader(float4 pos : SV_Position, float2 uv : TEXCOORD0) : SV_Target
{
    // GAP: LightPoint uses screen_to_uv(SV_Position), not interpolator UV.
    if (DebugOverlay > 0.5 && DebugView > 1.5)
    {
        uint2 p = uint2(pos.xy);
        float z = AnomalyLinearDepth[p];
        bool player = z > 0 && AnomalyContactPlayerPixel(AnomalyPlayerDepth, p,
            AnomalyLightingViewPos(pos.xy, z));
        return player ? float4(1, 0.05, 0.05, 1) : float4(0.04, 0.12, 0.04, 1);
    }
    if (DebugOverlay > 0.5 && DebugView > 0.5)
    {
        uint2 size = AtlasSize();
        uint faceSize = size.x / 6;
        uint row = (uint)max(DebugAtlasRow, 0);
        if (faceSize == 0 || size.y <= 1 || row >= (size.y - 1) / max(faceSize, 1))
            return float4(0.8, 0.1, 0.8, 1);
        float4 header = AnomalyPointShadowAtlas.Load(int3(row, 0, 0));
        if (header.w < 0.05)
            return float4(0.8, 0.1, 0.8, 1);
        // Six panels: +X, -X, +Y, -Y, +Z, -Z, left to right.
        uint2 p = min(uint2(saturate(uv) * float2(size.x, faceSize)), uint2(size.x - 1, faceSize - 1));
        p.y += 1 + row * faceSize;
        float depth = AnomalyPointShadowAtlas.Load(int3(p, 0)).r;
        return depth > 1e4 ? float4(0.03, 0.08, 0.16, 1)
            : float4((1 - saturate(depth / header.w)).xxx, 1);
    }
    uint2 pixel = (uint2)pos.xy;
    float linearDepth = AnomalyLinearDepth[pixel];
    uint2 tileCoord = pixel;
    uint2 screenRes = max((uint2)frame_.Screen.resolution, uint2(1, 1));
    tileCoord = tileCoord % screenRes;
    tileCoord /= TILE_SIZE;
    uint tileIndex = mad(frame_.Light.tiles_x, tileCoord.y, tileCoord.x);
    uint numLights = min(AnomalyTileIndices[tileIndex], frame_.Light.maxTileLights);
    uint evalCount = min(numLights, MAX_EVAL);
    uint marchCap = (uint)clamp(MaxLights, 1, MAX_MARCH);

    if (linearDepth < 1e-4)
    {
        if (DebugOverlay > 0.5)
            return DebugColor(linearDepth, numLights, 0, 0, 1);
        return AnomalyIsolatedSub(0);
    }

    float4 g0 = AnomalyGBuffer0[pixel];
    float4 g1 = AnomalyGBuffer1[pixel];
    float4 g2 = AnomalyGBuffer2[pixel];
    float3 nView = normalize(unpack_normals2(g1.xy));
    float3 N = AnomalyLightingN(nView);
    float3 viewPos = AnomalyLightingViewPos(pos.xy, linearDepth);
    float3 viewPosMarch = AnomalyLightingViewPosUnjittered(pos.xy, linearDepth);
    float3 geometricNormal = cross(ddx(viewPosMarch), ddy(viewPosMarch));
    float normalLength = dot(geometricNormal, geometricNormal);
    geometricNormal = normalLength > 1e-10 ? geometricNormal * rsqrt(normalLength) : nView;
    geometricNormal *= dot(geometricNormal, nView) < 0 ? -1 : 1;
    float3 V = -normalize(compute_screen_ray(AnomalyLightingUv(pos.xy)));
    float3 albedo = SurfaceAlbedo(g0.rgb, g2.r);
    float3 f0 = SurfaceF0(g0.rgb, g2.r);
    float gloss = g2.g;
    float ao = saturate(1 - (1 - max(g1.b, 0.01)) * frame_.Light.aoPointLight);

    uint steps = (uint)clamp(StepCount, 12, MAX_STEPS);
    float thickness = clamp(Thickness, 0.2, 1.2);
    float maxDist = max(MaxDistance, 0.01);

    float topLuma[MAX_MARCH];
    uint topEval[MAX_MARCH];
    [loop]
    for (uint k = 0; k < MAX_MARCH; k++)
    {
        topLuma[k] = -1.0;
        topEval[k] = 0;
    }

    float3 removed = 0;
    float minVis = 1;
    float atlasOnly = 0;
    float3 unshadowedTotal = 0;
    [loop]
    for (uint findI = 0; findI < MAX_EVAL; findI++)
    {
        if (findI >= evalCount)
            break;
        uint findIndex = AnomalyTileIndices[frame_.Light.tiles_num + mad(frame_.Light.maxTileLights, tileIndex, findI)];
        AnomalyPointLight findLight = AnomalyPointLights[findIndex];
        if (SingleLight > 0.5)
        {
            uint2 atlasSize = AtlasSize();
            uint row = (uint)max(DebugAtlasRow, 0);
            if (atlasSize.x < 6 || atlasSize.y <= 1 || row >= (atlasSize.y - 1) / max(atlasSize.x / 6, 1))
                continue;
            float4 selected = AnomalyPointShadowAtlas.Load(int3(row, 0, 0));
            if (selected.w < 0.05 || distance(selected.xyz, findLight.positionView) > 0.001 || abs(selected.w - findLight.range) > 0.001)
                continue;
        }
        float3 findE = PointUnshadowed(findLight, viewPos, N, V, albedo, f0, gloss, ao);
        unshadowedTotal += findE;
        if (UseAtlas > 0.5 && (ShadowSource < 0.5 || ShadowSource > 1.5))
        {
            bool capturedCharacter;
            float atlasVis = CubeVisibility(viewPosMarch, findLight.positionView, findLight.range, capturedCharacter);
            removed += findE * (1 - atlasVis);
            minVis = min(minVis, atlasVis);
        }
        float findRange = ShadowRange(findLight, maxDist);
        float3 dummyL;
        float dummyDist;
        if (!PointLightDir(findLight, viewPosMarch, findRange, dummyL, dummyDist))
            continue;
        InsertTop(topLuma, topEval, marchCap, dot(findE, 1.0 / 3.0), findI);
    }

    [loop]
    for (uint m = 0; m < MAX_MARCH; m++)
    {
        if (ShadowSource > 1.5 || m >= marchCap)
            break;
        if (topLuma[m] < 0)
            break;

        uint i = topEval[m];
        uint index = AnomalyTileIndices[frame_.Light.tiles_num + mad(frame_.Light.maxTileLights, tileIndex, i)];
        AnomalyPointLight light = AnomalyPointLights[index];
        float shadowRange = ShadowRange(light, maxDist);
        float3 L;
        float distance;
        if (!PointLightDir(light, viewPosMarch, shadowRange, L, distance))
            continue;

        float3 energy = PointUnshadowed(light, viewPos, N, V, albedo, f0, gloss, ao);
        float marchDist = min(distance, maxDist);
        float atlasVis = 1;
        bool capturedCharacter = false;
        if (UseAtlas > 0.5 && ShadowSource < 0.5)
            atlasVis = CubeVisibility(viewPosMarch, light.positionView, light.range, capturedCharacter);
        float vis = AnomalyContactVisibilityExcludingPlayer(AnomalyLinearDepth,
            AnomalyPlayerDepth, viewPosMarch, geometricNormal, L, marchDist, thickness, steps);
        float contact = vis;
        vis = min(vis, atlasVis);
        if (vis < 0.45 && contact > 0.75)
            atlasOnly = 1;
        // Previous-frame depth is not a second occluder: it ghosts moving poses.
        if (ShadowSource < 0.5 && UseOccupancy > 0.5)
            vis = min(vis, MarchOccupancy(viewPosMarch, L, marchDist));
        minVis = min(minVis, vis);
        // Atlas contribution was already accumulated for every evaluated light.
        removed += energy * max(atlasVis - vis, 0);
    }

    if (DebugOverlay > 0.5)
    {
        // Magenta: atlas occludes, contact missed (FP body / AABB).
        if (atlasOnly > 0.5)
            return float4(0.95, 0.08, 0.85, 1);
        return DebugColor(linearDepth, numLights, removed, unshadowedTotal, minVis);
    }

    // IsolatedSub src is photometric point-light energy / dest. A longer-range
    // BRDF tail at the falloff dest-punched (duplicate/stepped silhouettes
    // that do not appear in the bright center).
    float3 destRgb = AnomalySceneColor[pixel].rgb;
    return AnomalyIsolatedSubEnergy(removed * saturate(Intensity), destRgb, unshadowedTotal);
}
