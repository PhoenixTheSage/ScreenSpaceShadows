using ClientPlugin.Anomaly;

namespace ClientPlugin.Shadows;

/// <summary>
/// Game/render-thread uniforms for the AfterLighting contact-shadow program.
/// Does not compile or draw; Anomaly owns the pass.
/// </summary>
internal static class ShadowRenderer
{
    public static void PushUniforms()
    {
        var config = Config.Current;
        var enabled = config.Enabled;
        var debug = enabled && config.DebugOverlay;
        AnomalyBridge.SetPassEnabled(AnomalyBridge.PassId, enabled && !debug);
        AnomalyBridge.SetPassEnabled(AnomalyBridge.DebugPassId, debug);
        if (!enabled)
        {
            var empty = new float[16];
            AnomalyBridge.SetUniforms(AnomalyBridge.PassId, empty);
            AnomalyBridge.SetUniforms(AnomalyBridge.DebugPassId, empty);
            AnomalyBridge.RequestPointShadows(0, 128);
            AnomalyBridge.RequestWorldBoxes(false);
            AnomalyBridge.RequestOccupancy(false);
            return;
        }

        QualityBudget(config.Quality, out var steps, out var maxLights, out var presetDistance,
            out var face, out var atlasCap, out var atlasSoft, out var contactSoftTaps);
        if (!config.SoftContact)
            contactSoftTaps = 0f;
        var packed = PackConstants(config, debug, steps, maxLights, presetDistance, atlasSoft,
            contactSoftTaps);
        AnomalyBridge.SetUniforms(AnomalyBridge.PassId, packed);
        AnomalyBridge.SetUniforms(AnomalyBridge.DebugPassId, packed);

        // Quality ceilings the expensive atlas. CubeLightCap is an extra ceiling.
        var cubeCap = config.CubeLightCap <= 0 ? atlasCap : System.Math.Min(config.CubeLightCap, atlasCap);
        if (cubeCap < 1)
            cubeCap = 1;
        if (cubeCap > 64)
            cubeCap = 64;
        AnomalyBridge.RequestPointShadows(cubeCap, face);
        AnomalyBridge.RequestWorldBoxes(config.UseLightViews && config.CubeLightCap > 0);
        AnomalyBridge.RequestOccupancy(config.UseOccupancy);
    }

    static float[] PackConstants(Config config, bool debug, float steps, float maxLights,
        float presetDistance, float atlasSoft, float contactSoftTaps)
    {
        var maxDistance = config.MaxDistance > 0.01f ? config.MaxDistance : presetDistance;
        return new[]
        {
            config.Intensity,
            config.Thickness,
            maxDistance,
            steps,
            maxLights,
            debug ? 1f : 0f,
            0f, // Legacy depth-history occluders caused repeated animated silhouettes.
            config.UseOccupancy ? 1f : 0f,
            1f,
            (float)config.Source,
            (float)config.DebugView,
            (float)System.Math.Min(System.Math.Max(config.DebugAtlasRow, 0), 63),
            config.SingleLight ? 1f : 0f,
            atlasSoft,
            contactSoftTaps,
            0f,
        };
    }

    /// <summary>
    /// Steps / contact lights / reach scale the IsolatedSub march.
    /// Face / atlasCap / atlasSoft scale the character cube atlas.
    /// contactSoftTaps is the 1D edge-filter sample count when Soft Contact
    /// is on (0 when off). Not a disk around the blocker.
    /// </summary>
    static void QualityBudget(ShadowQuality quality, out float steps, out float maxLights,
        out float distance, out int face, out int atlasCap, out float atlasSoft,
        out float contactSoftTaps)
    {
        switch (quality)
        {
            case ShadowQuality.Low:
                steps = 16f;
                maxLights = 4f;
                distance = 0.5f;
                face = 64;
                atlasCap = 1;
                atlasSoft = 0f;
                contactSoftTaps = 4f;
                break;
            case ShadowQuality.High:
                steps = 32f;
                maxLights = 8f;
                distance = 1.5f;
                face = 128;
                atlasCap = 4;
                atlasSoft = 1f;
                contactSoftTaps = 8f;
                break;
            case ShadowQuality.Ultra:
                steps = 48f;
                maxLights = 10f;
                distance = 2f;
                face = 256;
                atlasCap = 4;
                atlasSoft = 1f;
                contactSoftTaps = 8f;
                break;
            default:
                steps = 24f;
                maxLights = 6f;
                distance = 1f;
                face = 128;
                atlasCap = 2;
                atlasSoft = 0f;
                contactSoftTaps = 4f;
                break;
        }
    }
}
