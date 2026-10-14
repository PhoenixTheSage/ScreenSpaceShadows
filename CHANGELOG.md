# ScreenSpaceShadows changelog

## 1.3.1 — 2026-10-14

- Register a dedicated renderer-neutral Status page independently of settings and device initialization.
- Remove duplicate diagnostic labels and report controls from settings; include status-registration health in shadow reports.

## 1.3.0 — previous release

Release 1.3.0 guest-bus contact shadows

Version: 1.2.0 -> 1.3.0 (minor). The shadow program itself is unchanged.

Changelog:
- Route the pack through Anomaly RendererRoute so registration stays on the shared bus and device work waits for DX11.
- The same AfterLighting contact-shadow program is the guest-bus path when Vulkan is the renderer.

Co-authored-by: Cursor <cursoragent@cursor.com>
