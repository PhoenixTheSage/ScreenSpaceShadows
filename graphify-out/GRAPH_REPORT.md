# Graph Report - ScreenSpaceShadows  (2026-09-16)

## Corpus Check
- 46 files · ~14,643 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 484 nodes · 760 edges · 30 communities (28 shown, 1 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 22 edges (avg confidence: 0.83)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `31adac18`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PointContactShadows.hlsl
- Game Assemblies To Publicize
- Layout
- AnomalyBridge
- Plugin
- SettingsGenerator
- HLSL Shader Programming
- Preloader Helpers
- Element
- Control Button Data
- Config
- Config Dialog Example Screenshot
- Slider
- Hashing
- Tools
- Attribute
- Dropdown
- ClientPlugin.Settings.Elements
- Assembly Definition
- CheckboxAttribute
- Color
- Element 2
- Separator
- Textbox
- Client Plugin
- Control
- clean.sh
- StatusScreen
- ShadowsStatus

## God Nodes (most connected - your core abstractions)
1. `SettingsGenerator` - 17 edges
2. `TranspilerHelpers` - 17 edges
3. `Config` - 16 edges
4. `PreloaderHelpers` - 16 edges
5. `ClientPlugin.Settings.Elements` - 15 edges
6. `Control` - 15 edges
7. `AnomalyBridge` - 14 edges
8. `IElement` - 14 edges
9. `KeybindAttribute` - 14 edges
10. `StatusScreen` - 11 edges

## Surprising Connections (you probably didn't know these)
- `IsolatedSub Compose` --semantically_similar_to--> `IsolatedMix Energy`  [INFERRED] [semantically similar]
  README.md → AGENTS.md
- `PointContactShadows.hlsl` --semantically_similar_to--> `HLSL Shader Programming`  [INFERRED] [semantically similar]
  README.md → .agents/skills/a5c-ai-babysitter-hlsl/SKILL.md
- `March LOD` --semantically_similar_to--> `AnomalySafetyScale`  [INFERRED] [semantically similar]
  AGENTS.md → README.md
- `March LOD` --semantically_similar_to--> `Contact March Quality Presets`  [INFERRED] [semantically similar]
  AGENTS.md → README.md
- `Space Engineers Plugin Development` --semantically_similar_to--> `Space Engineers`  [INFERRED] [semantically similar]
  AGENTS.md → README.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Agent Instruction Bootstrap** — github_copilot_instructions, vscode_agents, agents [EXTRACTED 1.00]
- **More Settings Control Row** — docs_configdialogexample_more_settings, docs_configdialogexample_color, docs_configdialogexample_color_with_alpha, docs_configdialogexample_keybind [EXTRACTED 1.00]
- **Some Settings Control Row** — docs_configdialogexample_some_settings, docs_configdialogexample_toggle, docs_configdialogexample_integer, docs_configdialogexample_number, docs_configdialogexample_text, docs_configdialogexample_dropdown [EXTRACTED 1.00]
- **Point Shadow Layers** — readme_contact_shadows, readme_temporal_depth, readme_occupancy_clipmap, readme_light_view_maps [EXTRACTED 1.00]
- **Config Widget Type Catalog** — docs_configdialogexample_toggle, docs_configdialogexample_integer, docs_configdialogexample_number, docs_configdialogexample_text, docs_configdialogexample_dropdown, docs_configdialogexample_color, docs_configdialogexample_color_with_alpha, docs_configdialogexample_keybind, docs_configdialogexample_button [INFERRED 0.85]
- **Duplicated HLSL Skill Copies** — agents_skills_a5c_ai_babysitter_hlsl_readme, agents_skills_a5c_ai_babysitter_hlsl_skill, cursor_skills_a5c_ai_babysitter_hlsl_readme, cursor_skills_a5c_ai_babysitter_hlsl_skill [INFERRED 0.95]

## Communities (30 total, 1 thin omitted)

### Community 0 - "PointContactShadows.hlsl"
Cohesion: 0.07
Nodes (38): Anomaly Owns Shared Rendering Gaps, CometWorks Skills, Docs/Extensibility.md Slice AI, IsolatedMix Energy, March LOD, Rich HUD Config Save Rule, se-dev Skill, Space Engineers Plugin Development (+30 more)

### Community 1 - "Game Assemblies To Publicize"
Cohesion: 0.17
Nodes (13): CodeInstructionNotFound, TranspilerHelpers, CodeInstruction, CodeInstructionPredicate, IEnumerable, List, MethodInfo, Exception (+5 more)

### Community 2 - "Layout"
Cohesion: 0.10
Nodes (18): Layout, SettingsPanelSize, Func, List, MyGuiControlBase, Vector2, None, SettingsPanelSize (+10 more)

### Community 3 - "AnomalyBridge"
Cohesion: 0.08
Nodes (17): Assembly, AnomalyBridge, PackRegistered, PackRoot, RegistryFound, IEnumerable, MethodInfo, Type (+9 more)

### Community 4 - "Plugin"
Cohesion: 0.08
Nodes (15): Plugin, Instance, MethodImpl, AnomalyTerminalHook, Action, MethodInfo, Type, ConfigStorage (+7 more)

### Community 5 - "SettingsGenerator"
Cohesion: 0.16
Nodes (11): AttributeInfo, SettingsGenerator, ActiveLayout, Dialog, Action, Func, List, MethodInfo (+3 more)

### Community 6 - "HLSL Shader Programming"
Cohesion: 0.08
Nodes (26): HLSL Skill README, Compute GPU Processing, DirectX Shaders, GLSL, HLSL, Shader Optimization, HLSL Skill, Compute Shaders (+18 more)

### Community 7 - "Preloader Helpers"
Cohesion: 0.21
Nodes (9): PreloaderHelpers, CodeInstructionPredicate, Instruction, List, Collection, FieldReference, MethodDefinition, MethodReference (+1 more)

### Community 8 - "Element"
Cohesion: 0.20
Nodes (22): Element, Path, _detect_pulsar_dir(), _detect_space_engineers(), _generate_guid(), _get_install_locations(), _get_linux_steam_path(), _get_steam_path() (+14 more)

### Community 9 - "Control Button Data"
Cohesion: 0.16
Nodes (13): ControlButtonData, KeybindAttribute, SupportedTypes, Action, Func, List, Type, Binding (+5 more)

### Community 10 - "Config"
Cohesion: 0.09
Nodes (20): Button, Config, CubeLightCap, DebugOverlay, Enabled, Intensity, MaxDistance, Quality (+12 more)

### Community 11 - "Config Dialog Example Screenshot"
Cohesion: 0.13
Nodes (19): Config Dialog Example Screenshot, Action Button, Close Button, Color Picker, Color With Alpha Picker, Config Demo Window, Plugin Config Control Catalog, Dropdown Selector (+11 more)

### Community 12 - "Slider"
Cohesion: 0.18
Nodes (10): SliderAttribute, SupportedTypes, SliderType, Float, Integer, Action, Func, List (+2 more)

### Community 13 - "Hashing"
Cohesion: 0.16
Nodes (9): Hashing, CodeInstruction, IEnumerable, Instruction, MethodImpl, MethodInfo, ConstructorInfo, System.Runtime.CompilerServices (+1 more)

### Community 14 - "Tools"
Cohesion: 0.27
Nodes (4): Tools, Color, Match, Regex

### Community 15 - "Attribute"
Cohesion: 0.29
Nodes (7): Attribute, ButtonAttribute, SupportedTypes, Action, Func, List, Type

### Community 16 - "Dropdown"
Cohesion: 0.28
Nodes (6): DropdownAttribute, SupportedTypes, Action, Func, List, Type

### Community 17 - "ClientPlugin.Settings.Elements"
Cohesion: 0.24
Nodes (3): ClientPlugin.Settings.Tools, ClientPlugin.Settings.Elements, ClientPlugin.Settings.Layouts

### Community 18 - "Assembly Definition"
Cohesion: 0.25
Nodes (4): AssemblyDefinition, IEnumerable, Preloader, TargetDLLs

### Community 19 - "CheckboxAttribute"
Cohesion: 0.29
Nodes (6): CheckboxAttribute, SupportedTypes, Action, Func, List, Type

### Community 20 - "Color"
Cohesion: 0.29
Nodes (7): ColorAttribute, SupportedTypes, Action, Color, Func, List, Type

### Community 21 - "Element 2"
Cohesion: 0.29
Nodes (6): IElement, SupportedTypes, Action, Func, List, Type

### Community 22 - "Separator"
Cohesion: 0.29
Nodes (6): SeparatorAttribute, SupportedTypes, Action, Func, List, Type

### Community 23 - "Textbox"
Cohesion: 0.29
Nodes (6): TextboxAttribute, SupportedTypes, Action, Func, List, Type

### Community 24 - "Client Plugin"
Cohesion: 0.29
Nodes (5): ClientPlugin, net10.0, net48, Lib.Harmony (2.4.2), Mono.Cecil (0.11.6)

### Community 25 - "Control"
Cohesion: 0.50
Nodes (4): Control, MyGuiControlBase, Vector2, MyGuiDrawAlignEnum

### Community 28 - "StatusScreen"
Cohesion: 0.16
Nodes (9): StatusScreen, IEnumerable, StringBuilder, SettingsScreen, Func, List, MyGuiControlBase, Vector2 (+1 more)

### Community 29 - "ShadowsStatus"
Cohesion: 0.27
Nodes (3): ShadowsStatus, CurrentText, StringBuilder

## Knowledge Gaps
- **72 isolated node(s):** `RegistryFound`, `PackRegistered`, `PackRoot`, `net10.0`, `net48` (+67 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 175 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `System.Runtime.CompilerServices` connect `Hashing` to `Game Assemblies To Publicize`, `Config`?**
  _High betweenness centrality (0.180) - this node is a cross-community bridge._
- **Why does `ClientPlugin.Settings.Elements` connect `ClientPlugin.Settings.Elements` to `Layout`, `SettingsGenerator`, `Config`, `Slider`, `Dropdown`, `CheckboxAttribute`, `Element 2`, `Separator`, `Textbox`?**
  _High betweenness centrality (0.164) - this node is a cross-community bridge._
- **Why does `SettingsGenerator` connect `SettingsGenerator` to `Control`, `Layout`, `Plugin`, `StatusScreen`?**
  _High betweenness centrality (0.124) - this node is a cross-community bridge._
- **What connects `RegistryFound`, `PackRegistered`, `PackRoot` to the rest of the system?**
  _72 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PointContactShadows.hlsl` be split into smaller, more focused modules?**
  _Cohesion score 0.07152496626180836 - nodes in this community are weakly interconnected._
- **Should `Layout` be split into smaller, more focused modules?**
  _Cohesion score 0.09666666666666666 - nodes in this community are weakly interconnected._
- **Should `AnomalyBridge` be split into smaller, more focused modules?**
  _Cohesion score 0.08258258258258258 - nodes in this community are weakly interconnected._