using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class FastMultiplayerVerifyBuildRunner
{
    private const string TitleScenePath = "Assets/Scenes/mutiplayer/TitleScene.unity";
    private const string VerifyScenePath = "Assets/Scenes/mutiplayer/GamePlayScene_Verify.unity";

    private const string BeautifyResourcesPath = "Assets/Map/Beautify/URP/Runtime/Resources";
    private const string BeautifyResourcesDisabledPath = "Assets/Map/Beautify/URP/Runtime/Resources~";

    private const string OutputDirectory = "Builds/FastMultiplayerVerify";
    private const string ExecutableName = "BossRaidPortfolio_MPVerify.exe";

    [MenuItem("Tools/Build/Fast Multiplayer Verify Build")]
    public static void BuildWindows64()
    {
        BuildWindows64Internal(revealOutputDirectory: true);
    }

    public static void BuildWindows64BatchMode()
    {
        BuildWindows64Internal(revealOutputDirectory: false);
    }

    [MenuItem("Tools/Build/Restore Fast Build Assets")]
    public static void RestoreFastBuildAssets()
    {
        RestoreBeautifyResourcesIfNeeded(logIfNoop: true);
    }

    [InitializeOnLoadMethod]
    private static void RestoreBeautifyResourcesOnEditorLoad()
    {
        RestoreBeautifyResourcesIfNeeded(logIfNoop: false);
    }

    private static void BuildWindows64Internal(bool revealOutputDirectory)
    {
        ValidateBuildInputs();

        bool restoreAfterBuild = false;

        try
        {
            EnsureBeautifyResourcesExcludedForBuild(ref restoreAfterBuild);

            string outputDirectory = Path.GetFullPath(OutputDirectory);
            Directory.CreateDirectory(outputDirectory);

            string executablePath = Path.Combine(outputDirectory, ExecutableName);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { TitleScenePath, VerifyScenePath },
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Fast multiplayer verify build failed: {summary.result}");
            }

            Debug.Log($"Fast multiplayer verify build completed: {executablePath}");

            if (revealOutputDirectory)
            {
                EditorUtility.RevealInFinder(outputDirectory);
            }
        }
        finally
        {
            if (restoreAfterBuild)
            {
                RestoreBeautifyResourcesIfNeeded(logIfNoop: false);
            }
        }
    }

    private static void ValidateBuildInputs()
    {
        if (!File.Exists(TitleScenePath))
        {
            throw new FileNotFoundException($"Missing build scene: {TitleScenePath}");
        }

        if (!File.Exists(VerifyScenePath))
        {
            throw new FileNotFoundException($"Missing build scene: {VerifyScenePath}");
        }
    }

    private static void EnsureBeautifyResourcesExcludedForBuild(ref bool restoreAfterBuild)
    {
        bool hasActiveResources = Directory.Exists(BeautifyResourcesPath);
        bool hasDisabledResources = Directory.Exists(BeautifyResourcesDisabledPath);

        if (hasActiveResources && hasDisabledResources)
        {
            throw new InvalidOperationException(
                $"Both Beautify resource folders exist. Resolve manually: {BeautifyResourcesPath}, {BeautifyResourcesDisabledPath}");
        }

        if (hasDisabledResources && !hasActiveResources)
        {
            restoreAfterBuild = true;
            Debug.Log("Beautify runtime resources are already excluded for fast build.");
            return;
        }

        if (!hasActiveResources)
        {
            Debug.Log("Beautify runtime resources were not found. Fast build will continue without exclusion step.");
            return;
        }

        MoveAssetSidecar(BeautifyResourcesPath, BeautifyResourcesDisabledPath);
        restoreAfterBuild = true;
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Debug.Log("Beautify runtime resources are temporarily excluded for fast multiplayer verify build.");
    }

    private static void RestoreBeautifyResourcesIfNeeded(bool logIfNoop)
    {
        bool hasActiveResources = Directory.Exists(BeautifyResourcesPath);
        bool hasDisabledResources = Directory.Exists(BeautifyResourcesDisabledPath);

        if (hasActiveResources && hasDisabledResources)
        {
            throw new InvalidOperationException(
                $"Both Beautify resource folders exist. Resolve manually: {BeautifyResourcesPath}, {BeautifyResourcesDisabledPath}");
        }

        if (!hasDisabledResources)
        {
            if (logIfNoop)
            {
                Debug.Log("No excluded Beautify runtime resources were found.");
            }

            return;
        }

        MoveAssetSidecar(BeautifyResourcesDisabledPath, BeautifyResourcesPath);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Debug.Log("Beautify runtime resources have been restored.");
    }

    private static void MoveAssetSidecar(string sourceAssetPath, string destinationAssetPath)
    {
        string sourceMetaPath = sourceAssetPath + ".meta";
        string destinationMetaPath = destinationAssetPath + ".meta";

        if (File.Exists(destinationMetaPath) || Directory.Exists(destinationAssetPath) || File.Exists(destinationAssetPath))
        {
            throw new IOException($"Destination already exists: {destinationAssetPath}");
        }

        FileUtil.MoveFileOrDirectory(sourceAssetPath, destinationAssetPath);

        if (File.Exists(sourceMetaPath))
        {
            FileUtil.MoveFileOrDirectory(sourceMetaPath, destinationMetaPath);
        }
    }
}
