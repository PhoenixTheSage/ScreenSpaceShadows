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

    [Slider(0.15f, 2f, 0.05f, SliderAttribute.SliderType.Float, description: "Occluder thickness along the ray in meters. Character-scale needs ~0.5; the march uses at least 0.5")]
    public float Thickness
    {
        get => thickness;
        set => SetField(ref thickness, value);
    }

    [Slider(0f, 16f, 0.25f, SliderAttribute.SliderType.Float, label: "Max distance", description: "Ray length in meters. 0 uses the quality preset (Low 3 / Medium 6 / High 8 / Ultra 12)")]
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

    [Separator("Around corners")]

    [Checkbox(description: "Reproject last-frame linear depth while marching (reduces flicker; still camera-fed)")]
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

    [Checkbox(description: "Light-view AABB depth for selected point lights — the only layer that shadows never-seen occluders")]
    public bool UseLightViews
    {
        get => useLightViews;
        set => SetField(ref useLightViews, value);
    }

    [Slider(0f, 64f, 1f, SliderAttribute.SliderType.Integer, label: "Light-view cap", description: "How many closest point lights get a cube atlas. Default 4 for wide hardware. 0 disables cubes. Max 64.")]
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
