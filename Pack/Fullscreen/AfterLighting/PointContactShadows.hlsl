#include <AnomalyFullscreen.hlsli>
#include <Lighting/Brdf.hlsli>
#include <VertexTransformations.hlsli>

#define Intensity     AnomalyPassUniform0.x
#define Thickness     AnomalyPassUniform0.y
#define MaxDistance   AnomalyPassUniform0.z
#define StepCount     AnomalyPassUniform0.w
#define MaxLights     AnomalyPassUniform1.x
#define DebugOverlay  AnomalyPassUniform1.y
#define UseTemporal   AnomalyPassUniform1.z
#define UseOccupancy  AnomalyPassUniform1.w
#define UseCubes      AnomalyPassUniform2.x

#define AnomalyHistoryDepth      AnomalyPackSrv0
#define AnomalyOccupancy         AnomalyPackSrv1
#define AnomalyPointShadowAtlas  AnomalyPackSrv2

#define MAX_STEPS 24
// PrepareLights fills tiles in race order. frame_.Light.maxTileLights is the
// global list stride (256–4096), not a per-tile cap. Eval BRDF for many;
// march only the brightest (quality MaxLights).
#define MAX_EVAL 128
#define MAX_MARCH 24
#define MAX_OCC_STEPS 16
#define MAX_CUBE_LIGHTS 64
#define TILE_SIZE 16
#define OCC_DIM 64
#define OCC_VOXEL 2.0
#define OCC_ORIGIN float3(-64.0, -64.0, -64.0)

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
float3 PointUnshadowed(AnomalyPointLight light, float3 viewPos, float3 N, float3 V,
    float3 albedo, float3 f0, float gloss, float ao)
{
    float3 toLight = light.positionView - viewPos;
    float distance = length(toLight);
    if (distance >= light.range || distance < 1e-4)
        return 0;
    float3 L = toLight / distance;
    float2 attenuation = GetAttenuation(distance, light.range, light.fallOff);
    float3 lightFactor = attenuation.x * light.color * ao;
    if (dot(lightFactor, lightFactor) <= 1e-7)
        return 0;
    float3 H = normalize(V + L);
    return lightFactor * MaterialRadiance(albedo, 1, f0, gloss * light.glossFactor,
        N, L, V, H, light.diffuseFactor * attenuation.y);
}

bool PointLightDir(AnomalyPointLight light, float3 viewPos, out float3 L, out float distance)
{
    float3 toLight = light.positionView - viewPos;
    distance = length(toLight);
    if (distance >= light.range || distance < 1e-4)
    {
        L = 0;
        return false;
    }
    L = toLight / distance;
    return true;
}

float2 ViewToUv(float3 viewPos)
{
    float z = max(-viewPos.z, 1e-4);
    float3 ray = viewPos / z;
    float ray_x = rcp(max(frame_.Environment.projection_matrix._11, 1e-6));
    float ray_y = rcp(max(frame_.Environment.projection_matrix._22, 1e-6));
    float2 projOffset = float2(
        frame_.Environment.projection_matrix._31 * ray_x,
        frame_.Environment.projection_matrix._32 * ray_y);
    return float2(
        ((ray.x - projOffset.x) / ray_x + 1) * 0.5,
        (1 - (ray.y - projOffset.y) / ray_y) * 0.5);
}

// InvViewAt0 rows (same expansion as Aurora / clouds). mul(M, v) transposes.
float3 ViewToWorldAt0(float3 viewPos)
{
    return AnomalyCameraToWorld[0].xyz * viewPos.x
         + AnomalyCameraToWorld[1].xyz * viewPos.y
         + AnomalyCameraToWorld[2].xyz * viewPos.z;
}

float2 WorldToPrevUv(float3 worldAt0)
{
    float4 clip = mul(float4(worldAt0, 1), AnomalyPrevViewProj);
    float w = max(abs(clip.w), 1e-4);
    float2 ndc = clip.xy / w;
    return float2(ndc.x * 0.5 + 0.5, 0.5 - ndc.y * 0.5);
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

float MarchContact(float3 viewPos, float3 viewNormal, float3 lightDir, float marchDist,
    float thickness, uint steps)
{
    float vis = 1.0;
    float receiverZ = -viewPos.z;
    float invSteps = rcp((float)steps);
    float stepLen = marchDist * invSteps;
    // Offset off the receiver so the first samples are not the same surface.
    float3 origin = viewPos + viewNormal * 0.04 + lightDir * 0.08;
    float bias = 0.03;
    float occluderCloser = 0.08;
    [loop]
    for (uint s = 1; s <= MAX_STEPS; s++)
    {
        if (s > steps)
            break;
        float t = (float)s * stepLen;
        float3 samplePos = origin + lightDir * t;
        float2 sampleUv = AnomalyScreenUvToTexel(ViewToUv(samplePos));
        if (any(sampleUv < 0) || any(sampleUv > 1))
            break;

        float sceneZ = AnomalyLinearDepth.SampleLevel(AnomalyPointSampler, sampleUv, 0);
        float rayZ = -samplePos.z;
        float delta = rayZ - sceneZ;
        // Hit only if the ray is behind a closer occluder (character), not the
        // receiver itself (floor). Thickness is occluder depth along the ray.
        if (delta > bias && delta < thickness && sceneZ < receiverZ - occluderCloser)
        {
            vis = 0;
            break;
        }
    }

    return vis;
}

float MarchHistory(float3 viewPos, float3 viewNormal, float3 lightDir, float marchDist,
    float thickness, uint steps)
{
    float vis = 1.0;
    float3 worldReceiver = ViewToWorldAt0(viewPos);
    float4 recvClip = mul(float4(worldReceiver, 1), AnomalyPrevViewProj);
    float recvPrevZ = max(abs(recvClip.w), 1e-4);
    float invSteps = rcp((float)steps);
    float stepLen = marchDist * invSteps;
    float3 origin = viewPos + viewNormal * 0.04 + lightDir * 0.08;
    float bias = 0.03;
    float occluderCloser = 0.08;
    [loop]
    for (uint s = 1; s <= MAX_STEPS; s++)
    {
        if (s > steps)
            break;
        float t = (float)s * stepLen;
        float3 samplePos = origin + lightDir * t;
        float3 world = ViewToWorldAt0(samplePos);
        float2 prevUv = WorldToPrevUv(world);
        if (any(prevUv < 0) || any(prevUv > 1))
            break;

        float histZ = AnomalyHistoryDepth.SampleLevel(AnomalyPointSampler, prevUv, 0).r;
        float4 prevClip = mul(float4(world, 1), AnomalyPrevViewProj);
        float prevRayZ = max(abs(prevClip.w), 1e-4);
        float delta = prevRayZ - histZ;
        if (delta > bias && delta < thickness && histZ < recvPrevZ - occluderCloser)
        {
            vis = 0;
            break;
        }
    }

    return vis;
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

void CubeFaceUv(float3 dir, out uint face, out float2 uv)
{
    float3 a = abs(dir);
    if (a.x >= a.y && a.x >= a.z)
    {
        face = dir.x >= 0 ? 0 : 1;
        uv = dir.x >= 0 ? float2(-dir.z, -dir.y) : float2(dir.z, -dir.y);
        uv = uv / max(a.x, 1e-6) * 0.5 + 0.5;
    }
    else if (a.y >= a.z)
    {
        face = dir.y >= 0 ? 2 : 3;
        uv = dir.y >= 0 ? float2(dir.x, dir.z) : float2(dir.x, -dir.z);
        uv = uv / max(a.y, 1e-6) * 0.5 + 0.5;
    }
    else
    {
        face = dir.z >= 0 ? 4 : 5;
        uv = dir.z >= 0 ? float2(dir.x, -dir.y) : float2(-dir.x, -dir.y);
        uv = uv / max(a.z, 1e-6) * 0.5 + 0.5;
    }
}

float SampleCubeRow(uint row, float3 dir, float dist)
{
    uint2 atlasSize = AtlasSize();
    if (atlasSize.x < 6 || atlasSize.y < 2)
        return 1;
    uint faceRes = atlasSize.x / 6;
    uint face;
    float2 uv;
    CubeFaceUv(dir, face, uv);
    float2 px = float2((face + uv.x) * faceRes, 1 + row * faceRes + uv.y * faceRes);
    float map = AnomalyPointShadowAtlas.SampleLevel(AnomalyPointSampler, (px + 0.5) / atlasSize, 0).r;
    if (map >= 1e4)
        return 1;
    return map + 0.08 < dist ? 0.0 : 1.0;
}

float CubeVisibility(float3 viewPos, float3 lightViewPos, float range)
{
    uint2 atlasSize = AtlasSize();
    if (atlasSize.y < 2)
        return 1;
    uint faceRes = max(atlasSize.x / 6, 1);
    uint count = min((atlasSize.y - 1) / faceRes, MAX_CUBE_LIGHTS);
    float3 toPoint = viewPos - lightViewPos;
    float dist = length(toPoint);
    if (dist < 1e-4 || dist > range)
        return 1;
    float3 dir = toPoint / dist;
    [loop]
    for (uint i = 0; i < MAX_CUBE_LIGHTS; i++)
    {
        if (i >= count)
            break;
        float4 slot = AnomalyPointShadowAtlas.Load(int3(i, 0, 0));
        if (slot.w < 0.05)
            continue;
        if (distance(slot.xyz, lightViewPos) < 0.2)
            return SampleCubeRow(i, dir, dist);
    }

    return 1;
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
    // Yellow: occluded but almost nothing to IsolatedSub (self-hit / tiny energy).
    if (minVis < 0.5 && energy < 0.02)
        return float4(0.95, 0.82, 0.10, 1);
    float3 lit = float3(0.08, 0.78, 0.22) * saturate(energy * 2.0 + 0.15);
    float3 shadow = float3(1.0, 0.12, 0.04) * saturate(dot(removed, 1.0 / 3.0) * 4.0 + (1 - minVis));
    return float4(lerp(lit, shadow, saturate(1 - minVis)), 1);
}

float4 __pixel_shader(float4 pos : SV_Position, float2 uv : TEXCOORD0) : SV_Target
{
    // GAP: LightPoint uses screen_to_uv(SV_Position), not interpolator UV.
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
    float3 viewPos = AnomalyLightingViewPos(pixel, linearDepth);
    float3 V = -normalize(compute_screen_ray(AnomalyLightingUv(pixel)));
    float3 albedo = SurfaceAlbedo(g0.rgb, g2.r);
    float3 f0 = SurfaceF0(g0.rgb, g2.r);
    float gloss = g2.g;
    float ao = saturate(1 - (1 - max(g1.b, 0.01)) * frame_.Light.aoPointLight);

    uint steps = (uint)clamp(StepCount * AnomalySafetyScale, 1, MAX_STEPS);
    float thickness = max(Thickness, 0.5);
    float maxDist = max(MaxDistance, 0.01);

    float maxLuma = 0;
    [loop]
    for (uint findI = 0; findI < MAX_EVAL; findI++)
    {
        if (findI >= evalCount)
            break;
        uint findIndex = AnomalyTileIndices[frame_.Light.tiles_num + mad(frame_.Light.maxTileLights, tileIndex, findI)];
        float3 findU = PointUnshadowed(AnomalyPointLights[findIndex], viewPos, N, V, albedo, f0, gloss, ao);
        maxLuma = max(maxLuma, dot(findU, 1.0 / 3.0));
    }
    float lumaFloor = max(maxLuma * 0.2, 0.02);

    float3 removed = 0;
    float3 unshadowedTotal = 0;
    float minVis = 1;
    uint marches = 0;
    [loop]
    for (uint i = 0; i < MAX_EVAL; i++)
    {
        if (i >= evalCount)
            break;

        uint index = AnomalyTileIndices[frame_.Light.tiles_num + mad(frame_.Light.maxTileLights, tileIndex, i)];
        AnomalyPointLight light = AnomalyPointLights[index];
        float3 unshadowed = PointUnshadowed(light, viewPos, N, V, albedo, f0, gloss, ao);
        unshadowedTotal += unshadowed;

        float luma = dot(unshadowed, 1.0 / 3.0);
        if (luma < lumaFloor)
            continue;
        if (marches >= marchCap)
            continue;

        float3 L;
        float distance;
        if (!PointLightDir(light, viewPos, L, distance))
            continue;

        marches++;
        float marchDist = min(distance, maxDist);
        float vis = MarchContact(viewPos, nView, L, marchDist, thickness, steps);
        if (UseTemporal > 0.5 && AnomalyLightingHistoryValid)
            vis = min(vis, MarchHistory(viewPos, nView, L, marchDist, thickness, steps));
        if (UseOccupancy > 0.5)
            vis = min(vis, MarchOccupancy(viewPos, L, marchDist));
        if (UseCubes > 0.5)
            vis = min(vis, CubeVisibility(viewPos, light.positionView, light.range));
        minVis = min(minVis, vis);
        removed += unshadowed * (1 - vis);
    }

    if (DebugOverlay > 0.5)
        return DebugColor(linearDepth, numLights, removed, unshadowedTotal, minVis);

    // IsolatedSub src is a 0–1 dest fraction. Anomaly merges dest*(1-src)
    // from the AfterLighting dest copy (blit before pack PS). Debug Replace
    // is vis false-color and does not sample t0.
    float occ = saturate(1.0 - minVis) * saturate(Intensity);
    return AnomalyIsolatedSub(occ);
}
