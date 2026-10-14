using System;
using System.Reflection;
using ClientPlugin.Settings;
using ClientPlugin.Shadows;
using VRage.Utils;

namespace ClientPlugin.RichHud;

internal static class AnomalyTerminalHook
{
    public const string RegistryTypeName = "ClientPlugin.RichHud.TerminalConfigRegistry";
    public const string FolderTitle = "Screen Space Shadows";
    public const string SettingsPage = "Settings";
    public const string StatusPage = "Status";

    static readonly object Gate = new();
    static bool installed;

    public static bool TryInstall()
    {
        lock (Gate)
        {
            if (installed)
                return true;

            var page = RequestPage();
            if (page == null)
                return false;

            Populate(page);
            installed = true;
            MyLog.Default.WriteLine($"{Plugin.Name}: Rich HUD page under Anomaly Shaders / {FolderTitle} / {SettingsPage}");
            return true;
        }
    }

    static object RequestPage()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly == null || assembly == typeof(AnomalyTerminalHook).Assembly)
                continue;

            Type registry;
            try
            {
                registry = assembly.GetType(RegistryTypeName, false, false);
            }
            catch
            {
                continue;
            }

            if (registry == null)
                continue;

            var requestFolder = registry.GetMethod("RequestFolderPage", BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(string), typeof(string) }, null);
            try
            {
                return requestFolder?.Invoke(null, new object[] { FolderTitle, SettingsPage });
            }
            catch (Exception e)
            {
                MyLog.Default.Warning($"{Plugin.Name}: RequestFolderPage failed: {e.Message}");
                return null;
            }
        }

        return null;
    }

    static void Populate(object page)
    {
        var t = page.GetType();
        Invoke(t, page, "Category", "Point lights");
        Invoke(t, page, "Checkbox", "Enabled",
            (Func<bool>)(() => Config.Current.Enabled),
            (Action<bool>)(v => Set(() => Config.Current.Enabled = v)),
            "Master switch for screen-space point-light contact shadows");
        Invoke(t, page, "Slider", "Intensity", 0f, 1f,
            (Func<float>)(() => Config.Current.Intensity),
            (Action<float>)(v => Set(() => Config.Current.Intensity = v)),
            "How much point-light energy the contact shadow removes", 0.01f);
        Invoke(t, page, "Dropdown", "Quality", typeof(ShadowQuality),
            (Func<object>)(() => Config.Current.Quality),
            (Action<object>)(v => Set(() => Config.Current.Quality = (ShadowQuality)v)),
            "March steps, lights per pixel, atlas size, and contact soft taps when Soft Contact is on");
        Invoke(t, page, "Checkbox", "Soft Contact",
            (Func<bool>)(() => Config.Current.SoftContact),
            (Action<bool>)(v => Set(() => Config.Current.SoftContact = v)),
            "Soften the contact umbra edge only (1D, perpendicular to the light). Off keeps crisp first-hit umbras. Quality: Low/Medium 4 taps, High/Ultra 8. Independent of atlas soft.");
        Invoke(t, page, "Slider", "Thickness", 0.2f, 1.2f,
            (Func<float>)(() => Config.Current.Thickness),
            (Action<float>)(v => Set(() => Config.Current.Thickness = v)),
            "Euclidean metres from the 3D light ray; larger accepts more blockers and more false hits, not a penumbra control", 0.05f);
        Invoke(t, page, "Slider", "Max distance", 0f, 16f,
            (Func<float>)(() => Config.Current.MaxDistance),
            (Action<float>)(v => Set(() => Config.Current.MaxDistance = v)),
            "Ray length in meters. 0 uses the quality preset (Low 0.5 / Medium 1 / High 1.5 / Ultra 2)", 0.25f);
        Invoke(t, page, "Checkbox", "Debug overlay",
            (Func<bool>)(() => Config.Current.DebugOverlay),
            (Action<bool>)(v => Set(() => Config.Current.DebugOverlay = v)),
            "Show removed point-light energy as a red overlay instead of darkening LBuffer");
        Invoke(t, page, "Dropdown", "Shadow source", typeof(ShadowSource),
            (Func<object>)(() => Config.Current.Source),
            (Action<object>)(v => Set(() => Config.Current.Source = (ShadowSource)v)),
            "Combined: atlas player plus scene contact. ContactOnly: scene contact without player. AtlasOnly: mesh atlas.");
        Invoke(t, page, "Dropdown", "Debug view", typeof(ShadowDebugView),
            (Func<object>)(() => Config.Current.DebugView),
            (Action<object>)(v => Set(() => Config.Current.DebugView = (ShadowDebugView)v)),
            "Visibility, six atlas faces, or PlayerMask (red pixels excluded from contact). Requires Debug overlay.");
        Invoke(t, page, "Slider", "Atlas row", 0f, 63f,
            (Func<float>)(() => Config.Current.DebugAtlasRow),
            (Action<float>)(v => Set(() => Config.Current.DebugAtlasRow = (int)v)),
            "Light row for atlas preview and single-light isolation; nearest-light order.", 1f);
        Invoke(t, page, "Checkbox", "Single light",
            (Func<bool>)(() => Config.Current.SingleLight),
            (Action<bool>)(v => Set(() => Config.Current.SingleLight = v)),
            "Shadow only the selected atlas light. Other lights remain lit.");
        Invoke(t, page, "Category", "Around corners");
        Invoke(t, page, "Checkbox", "Occupancy clipmap",
            (Func<bool>)(() => Config.Current.UseOccupancy),
            (Action<bool>)(v => Set(() => Config.Current.UseOccupancy = v)),
            "March the camera-relative occupancy clipmap");
        Invoke(t, page, "Checkbox", "Light-view maps",
            (Func<bool>)(() => Config.Current.UseLightViews),
            (Action<bool>)(v => Set(() => Config.Current.UseLightViews = v)),
            "Optional AABB approximations. Character mesh maps are requested separately whenever enabled");
        Invoke(t, page, "Slider", "Light-view cap", 0f, 64f,
            (Func<float>)(() => Config.Current.CubeLightCap),
            (Action<float>)(v => Set(() => Config.Current.CubeLightCap = (int)v)),
            "Closest point lights with a cube atlas. Default 4. Values below 4 retain four mesh-map slots. Max 64.", 1f);
    }

    static void Set(Action apply)
    {
        apply();
        ConfigStorage.Save(Config.Current);
    }

    static void Invoke(Type type, object instance, string name, params object[] args)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        foreach (var method in type.GetMethods(flags))
        {
            if (method.Name != name || method.IsGenericMethodDefinition)
                continue;
            var parameters = method.GetParameters();
            if (args.Length > parameters.Length)
                continue;
            if (args.Length < parameters.Length)
            {
                var optional = true;
                for (var j = args.Length; j < parameters.Length; j++)
                {
                    if (!parameters[j].IsOptional && !parameters[j].HasDefaultValue)
                    {
                        optional = false;
                        break;
                    }
                }

                if (!optional)
                    continue;
            }
            var match = true;
            for (var i = 0; i < args.Length; i++)
            {
                var value = args[i];
                var expected = parameters[i].ParameterType;
                if (value == null)
                {
                    if (expected.IsValueType && Nullable.GetUnderlyingType(expected) == null)
                    {
                        match = false;
                        break;
                    }

                    continue;
                }

                if (!expected.IsInstanceOfType(value))
                {
                    match = false;
                    break;
                }
            }

            if (!match)
                continue;
            method.Invoke(instance, PadDefaults(method, args));
            return;
        }

        MyLog.Default.Warning($"{Plugin.Name}: Anomaly terminal missing {name}");
    }

    static object[] PadDefaults(MethodInfo method, object[] args)
    {
        var parameters = method.GetParameters();
        if (args.Length == parameters.Length)
            return args;
        var padded = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
            padded[i] = i < args.Length ? args[i] : parameters[i].DefaultValue;
        return padded;
    }
}
