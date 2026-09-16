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
            return;
        }

        var packed = PackConstants(config, debug);
        AnomalyBridge.SetUniforms(AnomalyBridge.PassId, packed);
        AnomalyBridge.SetUniforms(AnomalyBridge.DebugPassId, packed);

        var cubeCap = config.UseLightViews ? config.CubeLightCap : 0;
        if (cubeCap < 0)
            cubeCap = 0;
        if (cubeCap > 64)
            cubeCap = 64;
        AnomalyBridge.RequestPointShadows(cubeCap, 64);
        AnomalyBridge.RequestOccupancy(config.UseOccupancy);
    }

    static float[] PackConstants(Config config, bool debug)
    {
        // MaxLights is the contact-march cap. The shader evaluates up to 128 tile lights.
        QualityPreset(config.Quality, out var steps, out var maxLights, out var presetDistance);
        var maxDistance = config.MaxDistance > 0.01f ? config.MaxDistance : presetDistance;
        return new[]
        {
            config.Intensity,
            config.Thickness,
            maxDistance,
            steps,
            maxLights,
            debug ? 1f : 0f,
            config.UseTemporal ? 1f : 0f,
            config.UseOccupancy ? 1f : 0f,
            config.UseLightViews && config.CubeLightCap > 0 ? 1f : 0f,
        };
    }

    static void QualityPreset(ShadowQuality quality, out float steps, out float maxLights, out float distance)
    {
        switch (quality)
        {
            case ShadowQuality.Low:
                steps = 8f;
                maxLights = 8f;
                distance = 3f;
                break;
            case ShadowQuality.High:
                steps = 24f;
                maxLights = 16f;
                distance = 8f;
                break;
            case ShadowQuality.Ultra:
                steps = 24f;
                maxLights = 24f;
                distance = 12f;
                break;
            default:
                steps = 16f;
                maxLights = 16f;
                distance = 6f;
                break;
        }
    }
}
