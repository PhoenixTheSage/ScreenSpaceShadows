# Point-shadow baseline and next phases

## Implemented baseline

- Shared Anomaly cube-face mapping now agrees with the renderer's viewport Y.
- Four weighted depth comparisons, face-clamped taps, no half-texel offset into
  a neighboring face, header or light row. This is not an area-light penumbra.
- One highest-detail drawable character LOD; opaque head/body materials hidden
  from the main camera remain shadow casters. Main-camera flags are untouched.
- Depth receiver reconstruction uses the live projection inverse.
- ContactOnly / AtlasOnly / Combined, raw six-face preview, selected atlas light
  isolation, and caster/submission status in both supported settings UIs.

## Automated checks

Both Anomaly and Shadows build for net48 and net10.0. FXC ps_5_0 compiles the full
pack shader against installed Keen includes and current Anomaly includes.
Anomaly's scripts/TestPointShadowProjection.ps1 executes the real HLSL helper on
D3D11 WARP against VRage matrices for 600 directions. It checks face corners,
depth comparisons, header/row isolation and invalid rows. This does not validate
character skinning, render scheduling or visual quality in Space Engineers.

## In-game acceptance scene

1. Update/restart with both builds. Use one stationary point light, a floor,
   an asymmetric opaque blocker, and the local character. Disable other point
   lights if possible. Disable occupancy and AABB extras; legacy temporal depth is no longer active.
2. Choose AtlasOnly, Single light, Atlas row 0. Debug overlay -> AtlasFaces
   must show an animated body including opaque head geometry. Blue means clear;
   magenta means the selected row is absent. Status must show proxies > 0 and
   draws > 0. Draw count includes six faces per selected light.
3. Switch Debug view to Visibility, then turn Debug overlay off. Walk, crouch,
   turn, rotate the camera, and switch first/third person. Check that the body
   shadow follows the light, retains the head and does not duplicate its outline.
   A world blocker is not expected in the mesh-only atlas yet.
4. Choose ContactOnly. The world blocker should now contribute where visible
   in screen depth. Record repetitions separately from the mesh-only result.
   Camera-dependent disappearances remain a known contact limitation.
   Soft Contact off is the crisp first-hit. On should soften only the umbra
   edge (not stripe or wash interior). Direction stays locked
   (Quality Low/Medium 4 taps, High/Ultra 8).
5. Choose Combined. Verify adding contact does not introduce a displaced body
   copy. Repeat with upscaler/jitter enabled and disabled, keeping the scene fixed.
6. Add a second light only after this passes. Check atlas row changes and note
   that Single light isolates shadowing, not all illumination.

Evidence to capture: source mode, selected row, Status text, whether jitter is on,
and whether the artifact appears in raw atlas depth or only the final image.

## Next phases (ordered)

1. Live acceptance of the corrected atlas and full-body pose. If skinning fails,
   inspect actual proxy object/bone constants before changing contact bias.
2. Make atlas evaluation independent of the contact top-N budget; stable light
   identity and importance selection belong in Anomaly. Add atlas resolution
   and update budgets with performance measurements.
3. Add actual world-mesh casters in Anomaly, including material alpha cutouts,
   instance transforms, moving grids and culling from each light. Never promote
   the AABB approximation into the quality path.
4. Reduce contact to short-range detail: replace camera-depth separation gates
   and large start offsets with bounded receiver bias and projected ray traversal.
   Validate thin grates, floor self-hits and screen-edge exits independently.
5. Add history only with motion/disocclusion rejection. Measure GPU cost at each
   phase before enabling a broader light count or longer contact rays.

Do not call the full world-shadow overhaul complete from builds or WARP tests.


## Repeated-outline revision (2026-09-18)

The supplied Light Test images show a coarse primary body shadow and repeated
outlines along both the character and doorway. The profile had Ultra quality,
10 m preset contact rays, 0.5 m thickness, temporal depth enabled, AABB extras
enabled and a 10-light atlas cap. These were read from Shadows.cfg, not inferred
from screenshot pixels.

The revised code replaces disconnected world-distance samples with contiguous
projected pixel intervals and perspective-correct depth overlap, removes old-depth
occluders, and uses geometric receiver-plane rejection with a small origin bias.
Contact reaches are now 0.5/1/1.5/2 m. Atlas resolutions are 256/256/512/1024;
the roughly 96 MiB cap permits 16/16/4/1 lights respectively. Atlas shadows are
independent of contact top-N selection among the first 64 tile lights.

Automated contact test: Anomaly/scripts/TestContactShadows.ps1 executes the actual
helper on D3D11 WARP. 256 rays check a contiguous analytic blocker silhouette,
flat-receiver self-shadow rejection and finite ray endpoints. The existing atlas
GPU regression also remains applicable.

For this test the previous profile settings were backed up to
Docs/Shadows.before-light-test.cfg and temporal/occupancy/AABB extras were disabled.
The Light Test save itself was not edited. In-game before/after acceptance is still
required. On resume, both net48 and net10.0 builds deployed successfully and both GPU regression scripts passed. Interim was launched with -Sources -appdata "T:\SteamLibrary\steamapps\common\SpaceEngineers\Profile". Window capture and activation now work, but automated clicks and Enter did not activate Continue Game; manual loading of Light Test is pending.



### Camera stability and first-person scope (2026-09-18)

Extra local-character mesh capture is now first-person only. Third person uses
camera-depth contact shadows, with no off-screen third-person caster guarantee.
Atlas faces are world-axis aligned: headers remain view-space light positions;
sampling directions must be rotated from view to world coordinates.
AnomalyPointShadowVisibility uses a deterministic face-clamped 4x4 tent filter
(bilinear visibility convolved with [1,2,1]/4). Shadows reconstructs receivers
at SV_Position pixel centers, matching blocker sampling. GPU regressions pass;
live movement and perspective-switch acceptance remains pending.

Retest Light Test after restarting with -Sources and the custom Profile path:
rotate without moving, walk past the light, and switch first/third person.
Compare Combined vs AtlasOnly in first person and ContactOnly in third person.
Confirm one silhouette. Synthetic tests do not establish motion stability.


### Unified character shadow and light stability (2026-09-18)

This supersedes the first-person-only restriction above: the same skinned atlas
caster runs in both perspectives. Screen-space contact is not a complete
third-person character-shadow substitute.

Atlas header texels 0..63 retain (view-space light position, range). Texel 64
now contains (camera-relative world caster center, valid=1); texel 65 contains
world-axis half extents. Metadata is zero unless mesh draws completed, and is
written after capture. AnomalyContactVisibilityExcludingBounds accepts these
bounds plus the current lighting frame view-to-world rotation. Only a matching
atlas light in Combined mode excludes this caster from depth contact; missing
atlas and ContactOnly retain the original depth path. This is a bounded spatial
approximation: nearby geometry inside the character bounds can also be excluded;
a per-pixel caster identity mask remains the precise long-term solution.

Atlas selection uses the render character center, in-range preference, stable
actor-ID ties and incumbent retention (0.81 squared-distance factor). Status
includes selected actor IDs. Sampling uses the live lighting inverse-view matrix.
GPU tests cover rotated exclusion and preservation of blockers outside bounds.
Live flicker acceptance is still pending; these tests do not reproduce frame
generation, temporal reconstruction or dynamic light culling.


### HDR stage ordering correction (2026-09-18)

The old AfterLighting prefix on MyTransparentRendering.Render ran during
parallel command-list recording. MyRenderScheduler schedules lighting resolve
and transparency independently; CPU light capture could therefore lag the
shadow pass. Worse, linear depth was produced in Scheduler.Done postfix,
after the HDR shadow consumers had used the previous frame's depth.

AfterLighting now runs in MyGBufferResolver.ConsumeWork postfix on the immediate
render context, after lighting submission. OwnedBuffersPass.Execute produces
current depth at this boundary before pack execution. AfterTransparent runs
in MyTransparentRendering.ConsumeWork postfix, after transparency submission.
The late Scheduler.Done depth invocation was removed; velocity diagnostics and
camera-velocity processing remain there. This changes shared HDR-stage timing
for all Anomaly packs and requires live integration validation.

Player contact policy: the mesh atlas serves both perspectives. Header texel
64.w identifies valid player bounds independent of atlas light allocation;
65.w separately indicates successful mesh capture. Contact excludes player
bounds even for lights without an atlas row and in ContactOnly diagnostic mode
(scene-only contact). This prevents partial player silhouettes returning when
coverage changes. It remains a bounding-volume approximation, not a precise
per-pixel actor mask.

TestShadowFrameOrder.ps1 checks the hook topology against the local decompiled
engine schedule and rejects a return to late depth production. GPU contact and
atlas tests remain green. These checks do not establish visual acceptance.


### Player contact mask (2026-09-18)

Combined now excludes the player using a rendered per-pixel depth mask instead
of atlas-header bounds. PointShadowPass renders the same skinned character into
reserved catalog `playerDepth`, an R32_FLOAT texture at render resolution, using
the current camera projection. Values are Euclidean camera distance; clear is
100000. It costs one extra character render and 4 bytes per render pixel.

AnomalyContactVisibilityExcludingPlayer compares scene-point distance with this
mask at each sampled pixel (3 cm minimum tolerance, scaled by depth). It skips
matching player samples while preserving foreground walls. This is raster-depth
matching, not an engine object-ID buffer; discrepancies at silhouettes remain a
live validation concern. Header bounds remain available but no longer drive the
Shadows pack's exclusion. Pack t7 binds playerDepth instead of unused historyDepth.

PlayerMask debug view uses the same predicate: red means excluded player, dark
green means scene contact remains eligible. PointShadowStatus includes mask draw
count. Combined is atlas player plus scene contact; ContactOnly is scene contact
without player; AtlasOnly remains atlas. Synthetic WARP tests cover player removal
and a foreground wall at the same pixels. In-game acceptance remains pending.


### Camera-distance and cold third-person transform correction (2026-09-18)

Keen MyCullProxy.UpdateWorldMatrix refreshes camera-relative object matrices only
for selected render proxies. The atlas selected a stable highest-detail LOD, so
its cached CommonObjectData.LocalMatrix could be uninitialized on third-person
load or stale after zooming out. This also displaced the player exclusion mask.

PointShadowPass now uploads a private copy of common object constants with the
live actor WorldMatrix minus the current camera in double precision, preserving
Keen's bone-remapping layout. It does not modify the main-view proxy cache. Atlas
LOD remains stable; playerDepth uses CurrentLod to match visible scene geometry.
LOD cross-fade boundaries and unrelated scene-contact distance artifacts remain
live validation concerns. TestCharacterTransform.ps1 compiles the production
transform helper and checks rotated geometry under 0..100 m camera offsets at
million-metre world coordinates.

Acceptance: load directly into third person, never enter first person, zoom in/out and orbit. Check PlayerMask remains aligned, then Combined shadow attachment.


### Scene-contact camera-distance regression (2026-09-18)

TestContactDistance.ps1 renders a fixed planar blocker/receiver scene on D3D11
WARP while translating the camera. The original shader passed offsets 0/5 m
but weakened at 15 m (visibility 0.412645 instead of <=0.2). Its eight-pixel
end fade covered an increasing fraction of the physical ray with distance.
The fade now uses perspective-correct physical ray fraction (last 10%).

A second, short-range scene still missed hits at a 3 m camera offset after that
fix. Unit-pixel intervals anchored at the ray origin crossed cell boundaries
but sampled just one cell. Traversal now splits at exact X/Y pixel crossings,
so depth overlap is tested within the sampled cell. Subpixel rays receive one
interval rather than being discarded below half a pixel. Budget bounds use a
conservative crossing count; very long projected rays remain budget-limited.

Regression suite now passes 256 receivers at each of ten camera/scale cases:
0/5/15/30/40 m offsets plus a 0.2-scale short-range scene at 0/1/3/6/8 m. It
includes axis-aligned and diagonal rays, lit receivers outside the silhouette,
flat-floor self-hit rejection and finite endpoints. Existing player exclusion
and foreground-wall tests also pass. Off-screen/hidden blockers and unresolved
subpixel geometry remain screen-space limitations. These reproduced failures
are fixed; matching the reported in-game scene still requires visual acceptance.
