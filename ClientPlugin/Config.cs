using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClientPlugin;

public enum ShadowQuality
{
    Low,
    Medium,
    High,
    Ultra,
}

public enum ShadowSource
{
    Combined,
    ContactOnly,
    AtlasOnly,
}

public enum ShadowDebugView
{
    Visibility,
    AtlasFaces,
    PlayerMask,
}

public class Config : INotifyPropertyChanged
{
    #region Options

    private bool enabled = true;
    private float intensity = 1f;
    private ShadowQuality quality = ShadowQuality.Medium;
    private float thickness = 0.5f;
    private float maxDistance;
    private bool debugOverlay;
    private bool useTemporal;
    private bool useOccupancy;
    private bool useLightViews;
    private int cubeLightCap = 4;
    private ShadowSource source;
    private ShadowDebugView debugView;
    private int debugAtlasRow;
    private bool singleLight;

    #endregion

    #region User interface

    public readonly string Title = "Screen Space Shadows";

    [Separator("Point-light contact shadows")]

    [Checkbox(description: "Master switch for screen-space point-light contact shadows")]
    public bool Enabled
    {
        get => enabled;
        set => SetField(ref enabled, value);
    }

    [Slider(0f, 1f, 0.01f, SliderAttribute.SliderType.Float, description: "How much point-light energy the contact shadow removes")]
    public float Intensity
    {
        get => intensity;
        set => SetField(ref intensity, value);
    }

    [Dropdown(description: "March steps, lights per pixel, and default ray distance")]
    public ShadowQuality Quality
    {
        get => quality;
        set => SetField(ref quality, value);
    }

    [Slider(0.2f, 1.2f, 0.05f, SliderAttribute.SliderType.Float, description: "View-depth hit tolerance in meters. Larger accepts more blockers and more false hits; this is not light-source softness.")]
    public float Thickness
    {
        get => thickness;
        set => SetField(ref thickness, value);
    }

    [Slider(0f, 16f, 0.25f, SliderAttribute.SliderType.Float, label: "Max distance", description: "Ray length in meters. 0 uses the quality preset (Low 0.5 / Medium 1 / High 1.5 / Ultra 2). Short-range contact detail. Pixel budget can shorten this distance; point-light energy stays within the light radius")]
    public float MaxDistance
    {
        get => maxDistance;
        set => SetField(ref maxDistance, value);
    }

    [Checkbox(description: "Replace LBuffer: magenta=no depth, cyan=no Frame tiles, blue=empty tile, green=lit, red=shadow with energy, yellow=occluded but nothing to subtract")]
    public bool DebugOverlay
    {
        get => debugOverlay;
        set => SetField(ref debugOverlay, value);
    }

    [Dropdown(description: "Combined: atlas player shadows plus scene contact shadows. ContactOnly: scene contact without the player. AtlasOnly: mesh atlas.")]
    public ShadowSource Source
    {
        get => source;
        set => SetField(ref source, value);
    }

    [Dropdown(description: "Debug overlay: visibility or six raw atlas faces (+X, -X, +Y, -Y, +Z, -Z).")]
    public ShadowDebugView DebugView
    {
        get => debugView;
        set => SetField(ref debugView, value);
    }

    [Slider(0f, 63f, 1f, SliderAttribute.SliderType.Integer, label: "Atlas row", description: "Atlas light row for face preview and single-light isolation. Selection follows the nearest-light atlas order.")]
    public int DebugAtlasRow
    {
        get => debugAtlasRow;
        set => SetField(ref debugAtlasRow, value);
    }

    [Checkbox(description: "Shadow only the light in the selected atlas row. Other lights remain lit. Use a one-light scene for stable camera-rotation tests.")]
    public bool SingleLight
    {
        get => singleLight;
        set => SetField(ref singleLight, value);
    }

    [Separator("Around corners")]

    // Legacy XML compatibility only; old depth is no longer a shadow caster.
    public bool UseTemporal
    {
        get => useTemporal;
        set => SetField(ref useTemporal, value);
    }

    [Checkbox(description: "March the camera-relative occupancy clipmap (2 m voxels; around-corner boxes). Off by default — too coarse for character contact")]
    public bool UseOccupancy
    {
        get => useOccupancy;
        set => SetField(ref useOccupancy, value);
    }

    [Checkbox(description: "Light-view AABB extras for selected point lights (local player AABB is never stamped). Mesh map of the local suit is always requested")]
    public bool UseLightViews
    {
        get => useLightViews;
        set => SetField(ref useLightViews, value);
    }

    [Slider(0f, 64f, 1f, SliderAttribute.SliderType.Integer, label: "Light-view cap", description: "How many closest point lights get a cube atlas (mesh + optional AABB). Default 4. 0 still keeps the mesh cap at 4. Max 64.")]
    public int CubeLightCap
    {
        get => cubeLightCap;
        set => SetField(ref cubeLightCap, value);
    }

    [Separator("Status")]

    [Button(label: "Show Status", description: "Compose, dest, last IsolatedSub merge/skip, catalog binds")]
    public static void ShowStatus() => Shadows.ShadowsStatus.Show();

    #endregion

    #region Property change notification boilerplate

    public static readonly Config Default = new Config();
    public static readonly Config Current = ConfigStorage.Load();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
