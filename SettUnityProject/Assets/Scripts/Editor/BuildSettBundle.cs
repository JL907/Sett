using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildSettBundle
{
    // Builds every asset bundle in the project (Assets/Sett is tagged "sett")
    // and copies "sett" and "sett.manifest" into SettMod/, where the .csproj embeds it.
    [MenuItem("Sett/Build Asset Bundle")]
    public static void Build()
    {
        string outDir = Path.Combine(Application.dataPath, "../AssetBundles");
        Directory.CreateDirectory(outDir);

        BuildPipeline.BuildAssetBundles(outDir, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);

        string modDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../SettMod"));
        if (!Directory.Exists(modDir))
        {
            Debug.LogWarning("[Sett] Bundle built to " + outDir + " but SettMod/ was not found at " + modDir);
            return;
        }

        foreach (string file in new[] { "sett", "sett.manifest" })
        {
            string src = Path.Combine(outDir, file);
            if (File.Exists(src))
                File.Copy(src, Path.Combine(modDir, file), true);
            else
                Debug.LogWarning("[Sett] Missing build output: " + src);
        }
        Debug.Log("[Sett] Copied sett and sett.manifest to " + modDir + ". Rebuild SettMod in Visual Studio to embed the bundle.");
    }
}
