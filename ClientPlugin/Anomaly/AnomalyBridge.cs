using System;
using System.Collections.Generic;
using System.Reflection;
using ClientPlugin.Shadows;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.Anomaly;

/// <summary>
/// Reflection-only bind to Anomaly. No compile-time reference.
/// </summary>
internal static class AnomalyBridge
{
    public const string PackId = "screenspace.shadows";
    public const string PassId = "screenspace.shadows.contact";
    public const string DebugPassId = "screenspace.shadows.debug";
    public const string UniformPassId = "screenspace.shadows.uniforms";

    const string PackRegistryType = "ClientPlugin.Shaders.ShaderPackRegistry";
    const string FullscreenType = "ClientPlugin.Shaders.FullscreenPassRegistry";
    const string OwnedPassType = "ClientPlugin.Shaders.OwnedPassRegistry";
    const string CatalogType = "ClientPlugin.Buffers.BufferCatalog";
    const string LocalCharacterType = "ClientPlugin.Shaders.LocalCharacter";

    static readonly object Gate = new();
    static bool registered;
    static MethodInfo setUniforms;
    static MethodInfo setEnabled;
    static MethodInfo tryGetProgramStatus;
    static MethodInfo catalogActive;
    static MethodInfo requestPointShadows;
    static PropertyInfo pointShadowStatus;
    static MethodInfo requestWorldBoxes;
    static MethodInfo requestOccupancy;
    static MethodInfo tryGetCamRelBox;
    static string packRoot;

    public static bool RegistryFound
    {
        get { lock (Gate) return setUniforms != null || registered; }
    }

    public static bool PackRegistered
    {
        get { lock (Gate) return registered; }
    }

    public static string PackRoot
    {
        get { lock (Gate) return packRoot; }
    }

    public static bool TryRegisterPack(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            return false;

        lock (Gate)
        {
            if (registered)
                return true;

            Type packType = null;
            Type fullscreen = null;
            Type owned = null;
            Type catalog = null;
            Type localCharacter = null;
            foreach (var assembly in SafeAssemblies())
            {
                packType ??= assembly.GetType(PackRegistryType, false, false);
                fullscreen ??= assembly.GetType(FullscreenType, false, false);
                owned ??= assembly.GetType(OwnedPassType, false, false);
                catalog ??= assembly.GetType(CatalogType, false, false);
                localCharacter ??= assembly.GetType(LocalCharacterType, false, false);
            }

            if (packType == null)
            {
                MyLog.Default.Warning($"{Plugin.Name}: Anomaly ShaderPackRegistry not found");
                return false;
            }

            packType.GetMethod("Register", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, new object[] { PackId, root });

            setUniforms = fullscreen?.GetMethod("SetUniforms", BindingFlags.Public | BindingFlags.Static);
            setEnabled = fullscreen?.GetMethod("SetEnabled", BindingFlags.Public | BindingFlags.Static);
            tryGetProgramStatus = fullscreen?.GetMethod(
                "TryGetProgramStatus",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(string).MakeByRefType() },
                null);
            catalogActive = catalog?.GetMethod("Active", BindingFlags.Public | BindingFlags.Static);
            pointShadowStatus = catalog?.GetProperty("PointShadowStatus", BindingFlags.Public | BindingFlags.Static);
            requestPointShadows = catalog?.GetMethod("RequestPointShadows", BindingFlags.Public | BindingFlags.Static);
            requestWorldBoxes = catalog?.GetMethod("RequestWorldBoxes", BindingFlags.Public | BindingFlags.Static);
            requestOccupancy = catalog?.GetMethod("RequestOccupancy", BindingFlags.Public | BindingFlags.Static);
            tryGetCamRelBox = localCharacter?.GetMethod("TryGetCamRelBox", BindingFlags.Public | BindingFlags.Static);

            var register = FindOwnedRegister(owned);
            register?.Invoke(null, new object[]
            {
                UniformPassId,
                "AfterLighting",
                0,
                0,
                (Action<object>)(_ => ShadowRenderer.PushUniforms()),
                0,
            });

            registered = true;
            packRoot = root;
            MyLog.Default.WriteLine($"{Plugin.Name}: registered Anomaly pack '{PackId}'");
            return true;
        }
    }

    public static bool CatalogAvailable(string name)
    {
        MethodInfo method;
        lock (Gate)
            method = catalogActive;
        if (method == null || string.IsNullOrWhiteSpace(name))
            return false;
        try
        {
            var buf = method.Invoke(null, new object[] { name });
            if (buf == null)
                return false;
            var available = buf.GetType().GetProperty("IsAvailable", BindingFlags.Public | BindingFlags.Instance);
            return available?.GetValue(buf) is true;
        }
        catch
        {
            return false;
        }
    }

    public static bool ProgramStatusAvailable
    {
        get { lock (Gate) return tryGetProgramStatus != null; }
    }

    public static bool TryGetProgramStatus(string programId, out string status)
    {
        status = null;
        MethodInfo method;
        lock (Gate)
            method = tryGetProgramStatus;
        if (method == null || string.IsNullOrWhiteSpace(programId))
            return false;
        try
        {
            var args = new object[] { programId, null };
            if (!(method.Invoke(null, args) is true))
                return false;
            status = args[1] as string;
            return !string.IsNullOrEmpty(status);
        }
        catch (Exception e)
        {
            MyLog.Default.Warning($"{Plugin.Name}: TryGetProgramStatus failed: {e.Message}");
            return false;
        }
    }

    public static void SetUniforms(string passId, float[] values)
    {
        MethodInfo method;
        lock (Gate)
            method = setUniforms;
        try
        {
            method?.Invoke(null, new object[] { passId, values });
        }
        catch (Exception e)
        {
            MyLog.Default.Error($"{Plugin.Name}: SetUniforms failed: {e.Message}");
            RenderTraceBind.Dump("Shadows.SetUniforms", e);
        }
    }

    public static string PointShadowStatus
    {
        get
        {
            try { return pointShadowStatus?.GetValue(null) as string ?? "unavailable (update Anomaly)"; }
            catch { return "unavailable"; }
        }
    }

    public static void RequestPointShadows(int maxLights, int faceResolution)
    {
        MethodInfo method;
        lock (Gate)
            method = requestPointShadows;
        try
        {
            method?.Invoke(null, new object[] { maxLights, faceResolution });
        }
        catch (Exception e)
        {
            MyLog.Default.Error($"{Plugin.Name}: RequestPointShadows failed: {e.Message}");
        }
    }

    public static void RequestWorldBoxes(bool enabled)
    {
        MethodInfo method;
        lock (Gate)
            method = requestWorldBoxes;
        try
        {
            method?.Invoke(null, new object[] { enabled });
        }
        catch (Exception e)
        {
            MyLog.Default.Error($"{Plugin.Name}: RequestWorldBoxes failed: {e.Message}");
        }
    }

    public static void RequestOccupancy(bool enabled)
    {
        MethodInfo method;
        lock (Gate)
            method = requestOccupancy;
        try
        {
            method?.Invoke(null, new object[] { enabled });
        }
        catch (Exception e)
        {
            MyLog.Default.Error($"{Plugin.Name}: RequestOccupancy failed: {e.Message}");
        }
    }

    public static bool TryGetCamRelBox(out Vector3 center, out Vector3 half)
    {
        center = Vector3.Zero;
        half = Vector3.Zero;
        MethodInfo method;
        lock (Gate)
            method = tryGetCamRelBox;
        if (method == null)
            return false;
        try
        {
            var args = new object[] { Vector3.Zero, Vector3.Zero };
            if (!(method.Invoke(null, args) is true))
                return false;
            center = (Vector3)args[0];
            half = (Vector3)args[1];
            return true;
        }
        catch (Exception e)
        {
            MyLog.Default.Warning($"{Plugin.Name}: TryGetCamRelBox failed: {e.Message}");
            return false;
        }
    }

    public static void SetPassEnabled(string passId, bool enabled)
    {
        MethodInfo method;
        lock (Gate)
            method = setEnabled;
        try
        {
            method?.Invoke(null, new object[] { passId, enabled });
        }
        catch (Exception e)
        {
            MyLog.Default.Error($"{Plugin.Name}: SetEnabled failed: {e.Message}");
        }
    }

    static MethodInfo FindOwnedRegister(Type owned)
    {
        if (owned == null)
            return null;
        foreach (var method in owned.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.Name != "Register" || method.IsGenericMethodDefinition)
                continue;
            var p = method.GetParameters();
            if (p.Length == 6 &&
                p[0].ParameterType == typeof(string) &&
                p[1].ParameterType == typeof(string) &&
                p[5].ParameterType == typeof(int))
                return method;
        }

        return null;
    }

    static IEnumerable<Assembly> SafeAssemblies()
    {
        Assembly[] assemblies;
        try
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }
        catch
        {
            yield break;
        }

        foreach (var assembly in assemblies)
        {
            if (assembly != null && assembly != typeof(AnomalyBridge).Assembly)
                yield return assembly;
        }
    }
}
