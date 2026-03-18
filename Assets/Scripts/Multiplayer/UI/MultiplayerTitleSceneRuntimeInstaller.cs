using Core.GameFlow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Multiplayer
{
    public static class MultiplayerTitleSceneRuntimeInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallDriverOnMultiplayerTitleScene()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!MultiplayerScenePaths.IsMultiplayerTitleScene(activeScene.path))
            {
                return;
            }

            TitleSceneController titleSceneController = Object.FindFirstObjectByType<TitleSceneController>();
            if (titleSceneController == null)
            {
                Debug.LogWarning("MultiplayerTitleSceneRuntimeInstaller: TitleSceneController was not found in multiplayer title scene.");
                return;
            }

            if (titleSceneController.GetComponent<MultiplayerTitleSceneDriver>() != null)
            {
                return;
            }

            titleSceneController.gameObject.AddComponent<MultiplayerTitleSceneDriver>();
        }
    }
}
