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
        sb.Append(" · maxDist ").Append(cfg.MaxDistance.ToString("0.##", CultureInfo.InvariantCulture));
        sb.Append(" · temporal ").Append(Yes(cfg.UseTemporal));
        sb.Append(" · occupancy ").Append(Yes(cfg.UseOccupancy));
        sb.Append(" · cubes ").AppendLine(Yes(cfg.UseLightViews));
    }

    static void AppendAnomaly(StringBuilder sb)
    {
        sb.Append("Anomaly  registry ").Append(Yes(AnomalyBridge.RegistryFound));
        sb.Append(" · pack ").AppendLine(Yes(AnomalyBridge.PackRegistered));
        if (AnomalyBridge.TryGetProgramStatus(AnomalyBridge.PassId, out var contact))
            sb.Append("Contact  ").AppendLine(contact);
        else
            sb.AppendLine("Contact  unknown (Anomaly TryGetProgramStatus missing or id not registered)");
        if (AnomalyBridge.TryGetProgramStatus(AnomalyBridge.DebugPassId, out var debug))
            sb.Append("Debug    ").AppendLine(debug);
        else
            sb.AppendLine("Debug    unknown");
    }

    static void AppendRuntime(StringBuilder sb)
    {
        sb.Append("Catalog  pointLights ").Append(Yes(AnomalyBridge.CatalogAvailable("pointLights")));
        sb.Append(" · tileIndices ").AppendLine(Yes(AnomalyBridge.CatalogAvailable("tileIndices")));
        sb.Append("     historyDepth ").Append(Yes(AnomalyBridge.CatalogAvailable("historyDepth")));
        sb.Append(" · occupancy ").Append(Yes(AnomalyBridge.CatalogAvailable("occupancy")));
        sb.Append(" · pointShadowAtlas ").AppendLine(Yes(AnomalyBridge.CatalogAvailable("pointShadowAtlas")));
        sb.AppendLine("Expect   Contact live AfterLighting/IsolatedSub merge=blend dest*(1-src)");
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
