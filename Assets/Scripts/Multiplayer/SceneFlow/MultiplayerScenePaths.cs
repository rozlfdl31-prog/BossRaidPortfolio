using System;

namespace Core.Multiplayer
{
    public static class MultiplayerScenePaths
    {
        public const string SceneFolderPath = "Assets/Scenes/mutiplayer";
        public const string TitleScenePath = SceneFolderPath + "/TitleScene.unity";
        public const string LoadingScenePath = SceneFolderPath + "/LoadingScene.unity";
        public const string GamePlayScenePath = SceneFolderPath + "/GamePlayScene.unity";

        public static bool IsMultiplayerScene(string scenePath)
        {
            return !string.IsNullOrEmpty(scenePath)
                   && scenePath.StartsWith(SceneFolderPath, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsMultiplayerTitleScene(string scenePath)
        {
            return string.Equals(scenePath, TitleScenePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
