using System.Globalization;
using System.Text;
using ClientPlugin.Anomaly;
using ClientPlugin.Gui;
using Sandbox.Graphics.GUI;

namespace ClientPlugin.Shadows;

public static class ShadowsStatus
{
    public static string CurrentText
    {
        get
        {
            var sb = new StringBuilder();
            sb.AppendLine(PluginStatusPage.RendererLine);
            AppendConfig(sb);
            sb.AppendLine();
            AppendAnomaly(sb);
            sb.AppendLine();
            AppendRuntime(sb);
            sb.AppendLine();
            AppendPaths(sb);
            return sb.ToString();
        }
    }

    public static void Show()
    {
        MyGuiSandbox.AddScreen(new StatusScreen(CurrentText));
    }

    static void AppendConfig(StringBuilder sb)
    {
        var cfg = Config.Current;
        sb.Append("Shadows ").Append(cfg.Enabled ? "on" : "off");
        sb.Append(" · ").Append(cfg.Quality);
        sb.Append(" · intensity ").Append(cfg.Intensity.ToString("0.##", CultureInfo.InvariantCulture));
        sb.Append(" · debug ").AppendLine(cfg.DebugOverlay ? "on (Replace exclusive)" : "off");
        sb.Append("     thickness ").Append(cfg.Thickness.ToString("0.##", CultureInfo.InvariantCulture));
        sb.Append(" · soft contact ").Append(Yes(cfg.SoftContact));
        sb.Append(" · maxDist ").Append(cfg.MaxDistance.ToString("0.##", CultureInfo.InvariantCulture));
        sb.Append(" · legacy temporal disabled");
        sb.Append(" · occupancy ").Append(Yes(cfg.UseOccupancy));
        sb.Append(" · cubes ").AppendLine(Yes(cfg.UseLightViews));
        sb.Append("     source ").Append(cfg.Source).Append(" · view ").Append(cfg.DebugView);
        sb.Append(" · atlas row ").Append(cfg.DebugAtlasRow).Append(" · single light ").AppendLine(Yes(cfg.SingleLight));
        sb.AppendLine("     quality sets face/cap/atlas-soft (Low 64×1 … Ultra 256×4); Soft Contact is 1D edge (4/8 taps)");
    }

    static void AppendAnomaly(StringBuilder sb)
    {
        sb.Append("Anomaly  registry ").Append(Yes(AnomalyBridge.RegistryFound));
        sb.Append(" · pack ").AppendLine(Yes(AnomalyBridge.PackRegistered));
        AppendProgram(sb, "Contact", AnomalyBridge.PassId);
        AppendProgram(sb, "Debug  ", AnomalyBridge.DebugPassId);
    }

    static void AppendProgram(StringBuilder sb, string label, string id)
    {
        if (!AnomalyBridge.ProgramStatusAvailable)
        {
            sb.Append(label).AppendLine("  unknown (Anomaly TryGetProgramStatus missing)");
            return;
        }

        if (AnomalyBridge.TryGetProgramStatus(id, out var status))
            sb.Append(label).Append("  ").AppendLine(status);
        else
            sb.Append(label).Append("  not registered id=").AppendLine(id);
    }

    static void AppendRuntime(StringBuilder sb)
    {
        sb.Append("Catalog  pointLights ").Append(Yes(AnomalyBridge.CatalogAvailable("pointLights")));
        sb.Append(" · tileIndices ").AppendLine(Yes(AnomalyBridge.CatalogAvailable("tileIndices")));
        sb.Append("     historyDepth ").Append(Yes(AnomalyBridge.CatalogAvailable("historyDepth")));
        sb.Append(" · occupancy ").Append(Yes(AnomalyBridge.CatalogAvailable("occupancy")));
        sb.Append(" · pointShadowAtlas ").AppendLine(Yes(AnomalyBridge.CatalogAvailable("pointShadowAtlas")));
        sb.Append("     ").AppendLine(AnomalyBridge.PointShadowStatus);
        sb.AppendLine("Expect   Contact live AfterLighting/IsolatedSub merge=blend dest*(1-src)");
        sb.AppendLine("         occ = shadowed point energy / dest (not minVis).");
        sb.AppendLine("         Contact not registered = json IsolatedSub lost (two passes, one hlsl).");
        sb.AppendLine("         Debug on sets skip=live-replace and hides IsolatedSub that frame.");
    }

    static void AppendPaths(StringBuilder sb)
    {
        sb.AppendLine("Paths");
        sb.Append("     pack ").AppendLine(Empty(AnomalyBridge.PackRoot));
    }

    static string Yes(bool value) => value ? "yes" : "no";

    static string Empty(string value) => string.IsNullOrEmpty(value) ? "(none)" : value;
}
