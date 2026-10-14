using System;
using System.Collections.Generic;
using System.Reflection;
using ClientPlugin.Anomaly;
using ClientPlugin.RichHud;
using ClientPlugin.Settings;
using ClientPlugin.Settings.Layouts;
using Sandbox.Graphics.GUI;
using VRage.Plugins;
using VRage.Utils;

#if !LOCAL_BUILD
[assembly: AssemblyVersion("1.3.0.0")]
[assembly: AssemblyFileVersion("1.3.0.0")]
#endif

namespace ClientPlugin;

// ReSharper disable once UnusedType.Global
public class Plugin : IPlugin
{
    public const string Name = "Shadows";
    public static Plugin Instance { get; private set; }
    private SettingsGenerator settingsGenerator;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Init(object gameInstance)
    {
        Instance = this;
        Instance.settingsGenerator = new SettingsGenerator();
        AnomalyTerminalHook.TryInstall();
    }

    public void Dispose()
    {
        ConfigStorage.FlushPending(true);
        Instance = null;
    }

    public void Update()
    {
        AnomalyRenderer.Observe();
        if (AnomalyRenderer.LinuxRequiresGuest)
            return;
        if (AnomalyRenderer.AllowGuestWork)
            Shadows.ShadowRenderer.PushUniforms();
        AnomalyTerminalHook.TryInstall();
        ConfigStorage.FlushPending();
    }

    // ReSharper disable once UnusedMember.Global
    public void LoadAssets(IReadOnlyDictionary<string, string> assets)
    {
        if (assets != null && assets.TryGetValue("AnomalyPack", out var root))
            AnomalyBridge.TryRegisterPack(root);
        else
            MyLog.Default.Warning($"{Name}: AnomalyPack asset missing");
        AnomalyTerminalHook.TryInstall();
    }

    // ReSharper disable once UnusedMember.Global
    public void LoadAssets(string folder)
    {
        // Named AnomalyPack is registered from the dictionary overload.
    }

    // ReSharper disable once UnusedMember.Global
    public void OpenConfigDialog()
    {
        Instance.settingsGenerator.SetLayout<Simple>();
        MyGuiSandbox.AddScreen(Instance.settingsGenerator.Dialog);
    }
}
