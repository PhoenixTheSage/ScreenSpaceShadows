# Screen Space Shadows

Anomaly pack that adds **point / omni light shadows** for Space Engineers. Vanilla SE only shadows the sun (CSM) and up to four spots; tiled point lights are unshadowed. This pack contact-marches those lights at `AfterLighting` and IsolatedSub-darkens `LBuffer` where the march hits.

Requires [Anomaly](https://github.com/PhoenixTheSage/Anomaly). This is a normal Pulsar plugin with `DependencyIds` pointing at Anomaly — not a Hidden plugin.

## What it does

After Keen’s tiled point-light pass, Anomaly publishes the live GPU buffers as catalog `pointLights` and `tileIndices`. This pack’s `Fullscreen/AfterLighting/PointContactShadows.hlsl` walks the same 16×16 tiles as `Lighting/LightPoint.hlsl`, evaluates matching unshadowed radiance, then contact-marches `linearDepth` toward each light. Optional layers (off by default):

1. **Contact** (always) — ray-march this-frame `linearDepth` toward the light. Hits only occluders closer to the camera than the receiver, so the floor does not self-shadow.
2. **Temporal** — reproject last-frame `historyDepth` (`AnomalyPrevViewProj`). Helps flicker; still camera-fed.
3. **Occupancy** — 64³ camera-relative clipmap (2 m voxels). Around-corner boxes only; too coarse for character contact.
4. **Light-view maps** — AABB boxes rasterized from the closest N point lights into `pointShadowAtlas`. **This is the only layer that shadows occluders the camera has never seen.** Cap default **4**, max **64**.

Compose is **IsolatedSub**: the shader writes `(1 − minVis) × Intensity` (0–1 dest fraction via `AnomalyIsolatedSub`). Anomaly blends that occupancy onto `LBuffer` with `dest*(1-src)` (same dest RTV as debug Replace). `temporal` is `InColor`+`Reactive` so Anomaly stamps `reactiveMask` from `.a`. Debug overlay is a separate **Replace** (vis false-color); it does not sample t0. Rich HUD **Anomaly Shaders → Screen Space Shadows → Status** (or Pulsar plugin options → Status) shows compose, dest, last merge/skip, and catalog binds.

## Settings

Dual config, one `Config.Current` (`Profile/Storage/Shadows.cfg`):

| UI | Where |
|----|--------|
| Rich HUD | **Anomaly Shaders → Screen Space Shadows → Settings** |
| Pulsar | plugin config dialog |

Controls: Enabled, Intensity, Quality, Thickness, Max distance, Debug overlay, Temporal depth, Occupancy clipmap, Light-view maps, **Light-view cap** (0–64). Occupancy, temporal, and light-views default **off**.

Quality presets (contact march uniforms, not shader permutations):

| Quality | Steps | March lights | Ray distance |
|---------|-------|--------------|--------------|
| Low | 8 | 8 | 3 m |
| Medium | 16 | 16 | 6 m |
| High | 24 | 16 | 8 m |
| Ultra | 24 | 24 | 12 m |

Quality **March lights** is how many of the brightest tile lights get a contact ray. The shader still evaluates up to 128 tile lights for the vis mask. Tile lists are unsorted; `maxTileLights` is the global stride, not a per-tile cap.

March step count is also scaled by Anomaly `AnomalySafetyScale` (camera translation / look). Do not globally floor that scale.

## Build / deploy

`dotnet build` copies `plugin.dll`, `plugin.xml`, and `Pack/` into Pulsar `Local/Shadows`. Enable Anomaly first, then Screen Space Shadows.

## Out of scope

Spot and sun shadows (already in vanilla). Overlay of `Lighting/LightPoint.hlsl`. Pack-side Harmony or a publicizer. Full mesh light-view raster (Anomaly draws actor AABBs, not Keen meshes). A full Hi-Z mip chain (Anomaly `hiZ` is a single half-res min of `linearDepth`).
