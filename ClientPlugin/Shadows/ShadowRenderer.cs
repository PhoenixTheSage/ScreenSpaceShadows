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

        var packed = PackConstants(config, debug);
        AnomalyBridge.SetUniforms(AnomalyBridge.PassId, packed);
        AnomalyBridge.SetUniforms(AnomalyBridge.DebugPassId, packed);

        var cubeCap = config.CubeLightCap;
        if (cubeCap < 4)
            cubeCap = 4;
        if (cubeCap > 64)
            cubeCap = 64;
        var face = config.Quality == ShadowQuality.Ultra ? 1024 :
            config.Quality == ShadowQuality.High ? 512 : 256;
        AnomalyBridge.RequestPointShadows(cubeCap, face);
        AnomalyBridge.RequestWorldBoxes(config.UseLightViews && config.CubeLightCap > 0);
        AnomalyBridge.RequestOccupancy(config.UseOccupancy);
    }

    static float[] PackConstants(Config config, bool debug)
    {
        // MaxLights is the contact-march cap. The shader evaluates up to 64 tile lights.
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
            0f, // Legacy depth-history occluders caused repeated animated silhouettes.
            config.UseOccupancy ? 1f : 0f,
            1f,
            (float)config.Source,
            (float)config.DebugView,
            (float)System.Math.Min(System.Math.Max(config.DebugAtlasRow, 0), 63),
            config.SingleLight ? 1f : 0f,
        };
    }

    static void QualityPreset(ShadowQuality quality, out float steps, out float maxLights, out float distance)
    {
        switch (quality)
        {
            case ShadowQuality.Low:
                steps = 48f;
                maxLights = 4f;
                distance = 0.5f;
                break;
            case ShadowQuality.High:
                steps = 128f;
                maxLights = 8f;
                distance = 1.5f;
                break;
            case ShadowQuality.Ultra:
                steps = 192f;
                maxLights = 10f;
                distance = 2f;
                break;
            default:
                steps = 96f;
                maxLights = 6f;
                distance = 1f;
                break;
        }
    }
}
