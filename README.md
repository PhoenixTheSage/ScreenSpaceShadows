# Screen Space Shadows

An Anomaly pack for point/omni-light shadows in Space Engineers. Normal Pulsar
plugin, dependent on Anomaly. Sun and spotlight shadows remain engine-owned.

## Current shadow paths

- **Character atlas:** Anomaly draws one complete opaque, animated character LOD
  from each selected point light. This includes body/head geometry hidden from
  the first-person camera. Shared cube sampling uses weighted depth comparisons.
- **Contact detail:** `AnomalyContactShadows.hlsli` traverses contiguous projected
  pixel intervals with perspective-correct depth bounds. It rejects the receiver
  tangent plane instead of imposing an arbitrary camera-depth separation window.
  The pixel budget truncates a ray rather than creating gaps between samples.
- **Composition:** remove each light's own shadowed photometric energy through
  IsolatedSub. Atlas visibility is evaluated for all considered tile lights,
  independently of the smaller contact top-N budget. The tile evaluation cap
  remains 64; unsorted overflow is still a limitation.

Previous-frame depth is no longer treated as another occluder. The serialized
UseTemporal setting is accepted for old configs but ignored and no longer shown.
Anomaly's normal InColor+Reactive temporal policy remains in place.

Optional occupancy and AABB extras are coarse approximations, not detailed world
shadows. Keep them off when evaluating shadow quality. Detailed offscreen world
mesh casters remain an Anomaly framework gap.

## Quality and limits

Low: 48 projected pixel intervals, 4 contact lights, 0.5 m contact reach, 256 face.
Medium: 96 intervals, 6 lights, 1 m, 256 face.
High: 128 intervals, 8 lights, 1.5 m, 512 face.
Ultra: 192 intervals, 10 lights, 2 m, 1024 face.

Max distance overrides contact reach only. A projected ray can stop earlier when
its pixel budget is exhausted; its final eight intervals fade. Contact is a local
supplement and cannot recover geometry missing from screen depth. Thickness is
a view-depth hit tolerance (0.2–1.2 m), not a light-source softness control.

Anomaly limits the RGBA32F atlas to roughly 96 MiB and D3D11 texture dimensions.
The maximum effective light count is 16 at 256, 4 at 512, and 1 at 1024 pixels.
The requested cap may be larger; Status reports the selected count. This trades
light coverage for detail at high quality. The default request cap remains four.
Atlas faces remain view-oriented and use clamped filtering at seams.

## Settings and diagnostics

Settings are available in Rich HUD under Anomaly Shaders / Screen Space Shadows
and in the Pulsar plugin dialog. Both share Profile/Storage/Shadows.cfg.

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
