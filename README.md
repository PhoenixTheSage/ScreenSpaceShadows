# Screen Space Shadows

An Anomaly pack for point/omni-light shadows in Space Engineers. Normal Pulsar
plugin, dependent on Anomaly. Sun and spotlight shadows remain engine-owned.

## Current shadow paths

- **Character atlas:** Anomaly draws one complete opaque, animated character LOD
  from each selected point light. This includes body/head geometry hidden from
  the first-person camera. Shared cube sampling uses weighted depth comparisons.
- **Contact detail:** `AnomalyContactShadows.hlsli` traverses contiguous projected
  pixel intervals, then tests Euclidean metres to the 3D light-ray segment
  (`thickness` plus a texel footprint), not a view-Z slab. Behind-camera
  endpoints are skipped. It rejects the receiver tangent plane instead of
  imposing an arbitrary camera-depth separation window. The pixel budget
  truncates a ray rather than creating gaps between samples. Default is a
  crisp first-hit. Soft Contact (off by default) softens only the umbra edge
  (1D filter on neighbor receivers, perpendicular to the light, ~3 cm virtual
  source). Quality sets taps (Low/Medium 4, High/Ultra 8). This is not the
  atlas 4×4 tent and is not a disk around the blocker.
- **Composition:** remove each light's own shadowed photometric energy through
  IsolatedSub. Atlas visibility runs only for lights with photometric energy,
  against a preloaded header row (max 8) matched nearest within 5 cm / 2% range.
  Soft 4×4 tent is High/Ultra only. Contact top-N rank is attenuation×NdotL
  (not view-dependent BRDF). Cube faces and occupancy use extras
  `AnomalyViewToWorldAt0`. Contact `towardLight` is LightPoint view-space (same
  axis); hits stay world-locked when the camera orbits.
  The tile evaluation cap is 32.

Previous-frame depth is no longer treated as another occluder. The serialized
UseTemporal setting is accepted for old configs but ignored and no longer shown.
Anomaly's normal InColor+Reactive temporal policy remains in place.

Optional occupancy and AABB extras are coarse approximations, not detailed world
shadows. Keep them off when evaluating shadow quality. Detailed offscreen world
mesh casters remain an Anomaly framework gap.

## Quality and limits

Quality now scales the expensive work (atlas + IsolatedSub), not only the march:

| Quality | Face | Atlas lights | Atlas PCF | Contact steps | Contact lights | Reach | Contact soft taps |
|---------|------|--------------|-----------|---------------|----------------|-------|-------------------|
| Low | 64 | 1 | off | 16 | 4 | 0.5 m | 4 |
| Medium | 128 | 2 | off | 24 | 6 | 1 m | 4 |
| High | 128 | 4 | on | 32 | 8 | 1.5 m | 8 |
| Ultra | 256 | 4 | on | 48 | 10 | 2 m | 8 |

Max distance overrides contact reach only. A projected ray can stop earlier when
its pixel budget is exhausted. Contact is a local supplement and cannot recover
geometry missing from screen depth. Thickness is Euclidean metres from the 3D
light ray (0.2–1.2 m plus a texel footprint), not a view-depth slab or a
light-source softness control. Soft Contact is a separate upgrade: a 1D
edge filter perpendicular to the light (~3 cm virtual source, pixel-clamped).
Off keeps the validated crisp first-hit. When on, the tap column above is used.

Anomaly limits the RGBA32F atlas to roughly 96 MiB and D3D11 texture dimensions.
Stamp the character mesh on all six cube faces. IsolatedSub preloads atlas
headers once per pixel and skips soft filtering on Low/Medium.
The tile evaluation cap is 32.

## Settings and diagnostics

Settings are available in Rich HUD under Anomaly Shaders / Screen Space Shadows
and in the Pulsar plugin dialog. Both share Profile/Storage/Shadows.cfg.

- Soft Contact: optional 1D edge soften on the contact umbra. Off by default.
- Shadow source: Combined, ContactOnly or AtlasOnly. Isolated modes bypass the
  optional occupancy path. AtlasOnly is useful for inspecting the body silhouette.
- Debug overlay / Debug view: Visibility or AtlasFaces. Six panels are +X, -X,
  +Y, -Y, +Z, -Z. Blue means clear depth; magenta means the selected row is absent.
- Atlas row / Single light: preview or shadow only the selected atlas light.
  Other lights remain lit. Rows follow nearest-light selection, not persistent IDs.
- Status: program/catalog health, selected/captured lights, accepted mesh proxies,
  submitted draws and missing-character/shader states.

## Build and validation

Build both Anomaly and this pack together. `dotnet build` deploys the appropriate
runtime and shader files to local Pulsar; source-loader installations use the
workspace trees directly. The latest observed game uses Pulsar Interim and the
Anomaly include root in this workspace.

See [Light Test validation](Docs/ShadowValidation.md). Automated checks cover
C# compilation, FXC, atlas projection and contact traversal on D3D11 WARP. They do
not establish visual quality or frame cost in the game.
