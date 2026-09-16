# Graph Report - ScreenSpaceShadows  (2026-09-16)

## Corpus Check
- 44 files · ~13,622 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 452 nodes · 700 edges · 28 communities (26 shown, 1 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 20 edges (avg confidence: 0.83)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `31adac18`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- A G E N
- Game Assemblies To Publicize
- Config
- Assembly
- Plugin
- Info
- H L S L
- Preloader Helpers
- Element
- Control Button Data
- Config 2
- Config Dialog Example Screenshot
- Slider
- Hashing
- Tools
- Button
- Dropdown
- Simple
- Assembly Definition
- Checkbox
- Color
- Element 2
- Separator
- Textbox
- Client Plugin
- Control
- clean.sh

## God Nodes (most connected - your core abstractions)
1. `SettingsGenerator` - 17 edges
2. `TranspilerHelpers` - 17 edges
3. `PreloaderHelpers` - 16 edges
4. `Config` - 15 edges
5. `ClientPlugin.Settings.Elements` - 15 edges
6. `Control` - 15 edges
7. `IElement` - 14 edges
8. `KeybindAttribute` - 14 edges
9. `Layout` - 11 edges
10. `SettingsScreen` - 11 edges

## Surprising Connections (you probably didn't know these)
- `IsolatedSub Compose` --semantically_similar_to--> `IsolatedMix Energy`  [INFERRED] [semantically similar]
  README.md → AGENTS.md
- `March LOD` --semantically_similar_to--> `AnomalySafetyScale`  [INFERRED] [semantically similar]
  AGENTS.md → README.md
- `March LOD` --semantically_similar_to--> `Contact March Quality Presets`  [INFERRED] [semantically similar]
  AGENTS.md → README.md
- `Space Engineers Plugin Development` --semantically_similar_to--> `Space Engineers`  [INFERRED] [semantically similar]
  AGENTS.md → README.md
- `PointContactShadows.hlsl` --semantically_similar_to--> `HLSL Shader Programming`  [INFERRED] [semantically similar]
  README.md → .agents/skills/a5c-ai-babysitter-hlsl/SKILL.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Agent Instruction Bootstrap** — github_copilot_instructions, vscode_agents, agents [EXTRACTED 1.00]
- **More Settings Control Row** — docs_configdialogexample_more_settings, docs_configdialogexample_color, docs_configdialogexample_color_with_alpha, docs_configdialogexample_keybind [EXTRACTED 1.00]
- **Some Settings Control Row** — docs_configdialogexample_some_settings, docs_configdialogexample_toggle, docs_configdialogexample_integer, docs_configdialogexample_number, docs_configdialogexample_text, docs_configdialogexample_dropdown [EXTRACTED 1.00]
- **Point Shadow Layers** — readme_contact_shadows, readme_temporal_depth, readme_occupancy_clipmap, readme_light_view_maps [EXTRACTED 1.00]
- **Config Widget Type Catalog** — docs_configdialogexample_toggle, docs_configdialogexample_integer, docs_configdialogexample_number, docs_configdialogexample_text, docs_configdialogexample_dropdown, docs_configdialogexample_color, docs_configdialogexample_color_with_alpha, docs_configdialogexample_keybind, docs_configdialogexample_button [INFERRED 0.85]
- **Duplicated HLSL Skill Copies** — agents_skills_a5c_ai_babysitter_hlsl_readme, agents_skills_a5c_ai_babysitter_hlsl_skill, cursor_skills_a5c_ai_babysitter_hlsl_readme, cursor_skills_a5c_ai_babysitter_hlsl_skill [INFERRED 0.95]

## Communities (28 total, 1 thin omitted)

### Community 0 - "A G E N"
Cohesion: 0.06
Nodes (43): Anomaly Owns Shared Rendering Gaps, CometWorks Skills, Docs/Extensibility.md Slice AI, IsolatedMix Energy, March LOD, Rich HUD Config Save Rule, se-dev Skill, Compute Shaders (+35 more)

### Community 1 - "Game Assemblies To Publicize"
Cohesion: 0.17
Nodes (13): CodeInstructionNotFound, TranspilerHelpers, CodeInstruction, CodeInstructionPredicate, IEnumerable, List, MethodInfo, Exception (+5 more)

### Community 2 - "Config"
Cohesion: 0.10
Nodes (18): Layout, SettingsPanelSize, Func, List, MyGuiControlBase, Vector2, None, SettingsPanelSize (+10 more)

### Community 3 - "Assembly"
Cohesion: 0.10
Nodes (13): Assembly, AnomalyBridge, IEnumerable, MethodInfo, Type, RenderTraceBind, Exception, MethodInfo (+5 more)

### Community 4 - "Plugin"
Cohesion: 0.10
Nodes (13): Plugin, Instance, MethodImpl, AnomalyTerminalHook, Action, MethodInfo, Type, ConfigStorage (+5 more)

### Community 5 - "Info"
Cohesion: 0.10
Nodes (17): AttributeInfo, SettingsGenerator, ActiveLayout, Dialog, Action, Func, List, MethodInfo (+9 more)

### Community 6 - "H L S L"
Cohesion: 0.10
Nodes (21): HLSL Skill README, Compute GPU Processing, DirectX Shaders, GLSL, HLSL, Shader Optimization, HLSL Skill, Microsoft HLSL Documentation (+13 more)

### Community 7 - "Preloader Helpers"
Cohesion: 0.21
Nodes (9): PreloaderHelpers, CodeInstructionPredicate, Instruction, List, Collection, FieldReference, MethodDefinition, MethodReference (+1 more)

### Community 8 - "Element"
Cohesion: 0.20
Nodes (22): Element, Path, _detect_pulsar_dir(), _detect_space_engineers(), _generate_guid(), _get_install_locations(), _get_linux_steam_path(), _get_steam_path() (+14 more)

### Community 9 - "Control Button Data"
Cohesion: 0.16
Nodes (13): ControlButtonData, KeybindAttribute, SupportedTypes, Action, Func, List, Type, Binding (+5 more)

### Community 10 - "Config 2"
Cohesion: 0.09
Nodes (20): Config, CubeLightCap, DebugOverlay, Enabled, Intensity, MaxDistance, Quality, Thickness (+12 more)

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

### Community 15 - "Button"
Cohesion: 0.29
Nodes (6): ButtonAttribute, SupportedTypes, Action, Func, List, Type

### Community 16 - "Dropdown"
Cohesion: 0.28
Nodes (6): DropdownAttribute, SupportedTypes, Action, Func, List, Type

### Community 17 - "Simple"
Cohesion: 0.29
Nodes (3): ClientPlugin.Settings.Tools, ClientPlugin.Settings.Elements, ClientPlugin.Settings.Layouts

### Community 18 - "Assembly Definition"
Cohesion: 0.25
Nodes (4): AssemblyDefinition, IEnumerable, Preloader, TargetDLLs

### Community 19 - "Checkbox"
Cohesion: 0.25
Nodes (7): Attribute, CheckboxAttribute, SupportedTypes, Action, Func, List, Type

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
Cohesion: 0.40
Nodes (4): Control, MyGuiControlBase, Vector2, MyGuiDrawAlignEnum

## Knowledge Gaps
- **68 isolated node(s):** `net10.0`, `net48`, `Lib.Harmony (2.4.2)`, `Mono.Cecil (0.11.6)`, `Low` (+63 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 170 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `System.Runtime.CompilerServices` connect `Hashing` to `Game Assemblies To Publicize`, `Config 2`?**
  _High betweenness centrality (0.185) - this node is a cross-community bridge._
- **Why does `ClientPlugin.Settings.Elements` connect `Simple` to `Config 2`, `Slider`, `Button`, `Dropdown`, `Checkbox`, `Element 2`, `Separator`, `Textbox`, `Control`?**
  _High betweenness centrality (0.179) - this node is a cross-community bridge._
- **Why does `SettingsGenerator` connect `Info` to `Simple`, `Config`, `Plugin`, `Control`?**
  _High betweenness centrality (0.124) - this node is a cross-community bridge._
- **What connects `net10.0`, `net48`, `Lib.Harmony (2.4.2)` to the rest of the system?**
  _68 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `A G E N` be split into smaller, more focused modules?**
  _Cohesion score 0.0613107822410148 - nodes in this community are weakly interconnected._
- **Should `Config` be split into smaller, more focused modules?**
  _Cohesion score 0.10144927536231885 - nodes in this community are weakly interconnected._
- **Should `Assembly` be split into smaller, more focused modules?**
  _Cohesion score 0.09879032258064516 - nodes in this community are weakly interconnected._