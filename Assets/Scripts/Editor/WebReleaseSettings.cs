using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Tools > PondBound > Apply Web Release Settings sets the Web Player Settings for speed in the
// browser, saves them, and logs what is stored. Tools > PondBound > Log Web Settings only logs.
// Code Optimization is kept in Library/, so it is per machine: run this on whoever builds the Web game.
public static class WebReleaseSettings
{
    [MenuItem("Tools/PondBound/Apply Web Release Settings")]
    static void Apply()
    {
        NamedBuildTarget web = NamedBuildTarget.WebGL;

        // Frame rate first: fastest Wasm, fast IL2CPP code, cheap exceptions.
        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.RuntimeSpeedLTO;
        PlayerSettings.SetIl2CppCodeGeneration(web, Il2CppCodeGeneration.OptimizeSpeed);
        PlayerSettings.WebGL.wasm2023 = true;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly; // FishNet catches exceptions

        // Fewer draw calls: the lab's static props get combined.
        PlayerSettings.SetStaticBatchingForPlatform(BuildTarget.WebGL, true);

        // Memory: start big enough that the heap doesn't have to grow (and copy) mid-game.
        PlayerSettings.WebGL.initialMemorySize = 512;
        PlayerSettings.WebGL.maximumMemorySize = 2048;
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

        // Download size and load time.
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetManagedStrippingLevel(web, ManagedStrippingLevel.Medium); // drop to Low if networking breaks in the Web build
        PlayerSettings.stripUnusedMeshComponents = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        // Decompression Fallback is left as it is: keep it on unless your host sends Content-Encoding: br.

        AssetDatabase.SaveAssets();
        Debug.Log("[WebRelease] Applied and saved.");
        LogSettings();
    }

    [MenuItem("Tools/PondBound/Log Web Settings")]
    static void LogSettings()
    {
        NamedBuildTarget web = NamedBuildTarget.WebGL;
        Debug.Log("[WebRelease] Web target values:\n" +
                  $"codeOptimization={UnityEditor.WebGL.UserBuildSettings.codeOptimization} (per machine)\n" +
                  $"il2cppCodeGeneration={PlayerSettings.GetIl2CppCodeGeneration(web)}\n" +
                  $"wasm2023={PlayerSettings.WebGL.wasm2023} exceptions={PlayerSettings.WebGL.exceptionSupport}\n" +
                  $"staticBatching={PlayerSettings.GetStaticBatchingForPlatform(BuildTarget.WebGL)}\n" +
                  $"memory initial={PlayerSettings.WebGL.initialMemorySize} max={PlayerSettings.WebGL.maximumMemorySize} growth={PlayerSettings.WebGL.memoryGrowthMode}\n" +
                  $"stripEngineCode={PlayerSettings.stripEngineCode} managedStripping={PlayerSettings.GetManagedStrippingLevel(web)} stripMeshComponents={PlayerSettings.stripUnusedMeshComponents}\n" +
                  $"compression={PlayerSettings.WebGL.compressionFormat} decompressionFallback={PlayerSettings.WebGL.decompressionFallback} dataCaching={PlayerSettings.WebGL.dataCaching} debugSymbols={PlayerSettings.WebGL.debugSymbolMode}\n" +
                  $"graphicsAPIs={string.Join(", ", PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL))}");
    }
}