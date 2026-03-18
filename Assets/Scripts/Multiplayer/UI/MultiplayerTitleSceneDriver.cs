using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Core.GameFlow;

namespace Core.Multiplayer
{
    [DisallowMultipleComponent]
    public sealed class MultiplayerTitleSceneDriver : MonoBehaviour
    {
        private const string SoloPlayButtonName = "Solo PlayButton";
        private const string CreateRoomButtonName = "Create RoomButton";
        private const string JoinRoomButtonName = "JoinButton";
        private const string StartButtonName = "StartButton";

        private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Dictionary<Button, bool> _buttonInteractableCache = new Dictionary<Button, bool>(16);

        private TitleSceneController _titleSceneController;
        private Button[] _sceneButtons;
        private Button _soloPlayButton;
        private Button _createRoomButton;
        private Button _joinRoomButton;
        private Button _startButton;
        private TMP_Text _createRoomButtonLabel;
        private TMP_Text _joinRoomButtonLabel;
        private MethodInfo _canAcceptMenuActionMethod;
        private MethodInfo _handleCreateRoomMethod;
        private MethodInfo _handleJoinRoomMethod;
        private FieldInfo _requestedTransitionField;
        private FieldInfo _popupMessageTextField;
        private FieldInfo _wrongKeyPopupField;
        private bool _isBusy;

        private void Start()
        {
            if (!Application.isPlaying || !MultiplayerScenePaths.IsMultiplayerTitleScene(gameObject.scene.path))
            {
                enabled = false;
                return;
            }

            if (!TryBindTitleSceneController() || !TryBindReflectionMembers() || !TryBindSceneButtons())
            {
                enabled = false;
                return;
            }

            RebindButton(_soloPlayButton, HandleSoloPlaySelected);
            RebindButton(_createRoomButton, HandleCreateRoomSelectedAsync);
            RebindButton(_joinRoomButton, HandleJoinRoomSelectedAsync);
            RebindButton(_startButton, HandleStartSelected);
        }

        private bool TryBindTitleSceneController()
        {
            _titleSceneController = GetComponent<TitleSceneController>();
            if (_titleSceneController == null)
            {
                Debug.LogError("MultiplayerTitleSceneDriver: TitleSceneController component is missing.");
                return false;
            }

            return true;
        }

        private bool TryBindReflectionMembers()
        {
            Type controllerType = _titleSceneController.GetType();
            _canAcceptMenuActionMethod = controllerType.GetMethod("CanAcceptMenuAction", InstanceFlags);
            _handleCreateRoomMethod = controllerType.GetMethod("HandleCreateRoomSelected", InstanceFlags);
            _handleJoinRoomMethod = controllerType.GetMethod("HandleJoinRoomSelected", InstanceFlags);
            _requestedTransitionField = controllerType.GetField("_requestedTransition", InstanceFlags);
            _popupMessageTextField = controllerType.GetField("_popupMessageText", InstanceFlags);
            _wrongKeyPopupField = controllerType.GetField("_wrongKeyPopup", InstanceFlags);

            bool hasRequiredMembers = _canAcceptMenuActionMethod != null
                                      && _handleCreateRoomMethod != null
                                      && _handleJoinRoomMethod != null
                                      && _requestedTransitionField != null
                                      && _popupMessageTextField != null
                                      && _wrongKeyPopupField != null;

            if (!hasRequiredMembers)
            {
                Debug.LogError("MultiplayerTitleSceneDriver: Failed to bind TitleSceneController reflection members.");
            }

            return hasRequiredMembers;
        }

        private bool TryBindSceneButtons()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            List<Button> sceneButtonList = new List<Button>(allButtons.Length);

            for (int i = 0; i < allButtons.Length; i++)
            {
                Button button = allButtons[i];
                if (button != null && button.gameObject.scene == gameObject.scene)
                {
                    sceneButtonList.Add(button);
                }
            }

            _sceneButtons = sceneButtonList.ToArray();
            _soloPlayButton = FindSceneButton(SoloPlayButtonName);
            _createRoomButton = FindSceneButton(CreateRoomButtonName);
            _joinRoomButton = FindSceneButton(JoinRoomButtonName);
            _startButton = FindSceneButton(StartButtonName);
            _createRoomButtonLabel = ResolveButtonLabel(_createRoomButton);
            _joinRoomButtonLabel = ResolveButtonLabel(_joinRoomButton);

            bool hasAllButtons = _soloPlayButton != null
                                 && _createRoomButton != null
                                 && _joinRoomButton != null
                                 && _startButton != null
                                 && _createRoomButtonLabel != null
                                 && _joinRoomButtonLabel != null;

            if (!hasAllButtons)
            {
                Debug.LogError("MultiplayerTitleSceneDriver: Failed to bind one or more title scene buttons.");
            }

            return hasAllButtons;
        }

        private Button FindSceneButton(string buttonName)
        {
            for (int i = 0; i < _sceneButtons.Length; i++)
            {
                Button button = _sceneButtons[i];
                if (button != null && string.Equals(button.name, buttonName, StringComparison.Ordinal))
                {
                    return button;
                }
            }

            return null;
        }

        private static TMP_Text ResolveButtonLabel(Button button)
        {
            return button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
        }

        private static void RebindButton(Button button, UnityEngine.Events.UnityAction onClick)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }

        private async void HandleCreateRoomSelectedAsync()
        {
            if (_isBusy || !CanAcceptMultiplayerMenuAction())
            {
                return;
            }

            _isBusy = true;
            CacheAndLockSceneButtons();
            SetLabel(_createRoomButtonLabel, "Connecting...");

            try
            {
                await MultiplayerServicesBootstrap.Instance.EnsureInitializedAsync();

                if (this == null || _titleSceneController == null)
                {
                    return;
                }

                RestoreSceneButtons();
                _handleCreateRoomMethod.Invoke(_titleSceneController, null);
            }
            catch (Exception ex)
            {
                RestoreSceneButtons();
                ShowBootstrapErrorPopup(ex, "Host create failed. Please try again.");
            }
            finally
            {
                SetLabel(_createRoomButtonLabel, "Create Room");
                _isBusy = false;
            }
        }

        private async void HandleJoinRoomSelectedAsync()
        {
            if (_isBusy || !CanAcceptMultiplayerMenuAction())
            {
                return;
            }

            _isBusy = true;
            CacheAndLockSceneButtons();
            SetLabel(_joinRoomButtonLabel, "Connecting...");

            try
            {
                await MultiplayerServicesBootstrap.Instance.EnsureInitializedAsync();

                if (this == null || _titleSceneController == null)
                {
                    return;
                }

                RestoreSceneButtons();
                _handleJoinRoomMethod.Invoke(_titleSceneController, null);
            }
            catch (Exception ex)
            {
                RestoreSceneButtons();
                ShowBootstrapErrorPopup(ex, "Client join failed. Please try again.");
            }
            finally
            {
                SetLabel(_joinRoomButtonLabel, "Join");
                _isBusy = false;
            }
        }

        private void HandleSoloPlaySelected()
        {
            if (_isBusy || !CanAcceptMultiplayerMenuAction())
            {
                return;
            }

            MarkSceneTransitionRequested();
            SceneManager.LoadScene(MultiplayerScenePaths.GamePlayScenePath);
        }

        private void HandleStartSelected()
        {
            if (_isBusy || !CanAcceptMultiplayerMenuAction())
            {
                return;
            }

            if (_startButton == null || !_startButton.gameObject.activeInHierarchy || !_startButton.interactable)
            {
                return;
            }

            MarkSceneTransitionRequested();
            SceneManager.LoadScene(MultiplayerScenePaths.GamePlayScenePath);
        }

        private bool CanAcceptMultiplayerMenuAction()
        {
            object result = _canAcceptMenuActionMethod.Invoke(_titleSceneController, null);
            return result is bool canAccept && canAccept;
        }

        private void MarkSceneTransitionRequested()
        {
            _requestedTransitionField.SetValue(_titleSceneController, true);
        }

        private void CacheAndLockSceneButtons()
        {
            _buttonInteractableCache.Clear();

            for (int i = 0; i < _sceneButtons.Length; i++)
            {
                Button button = _sceneButtons[i];
                if (button == null)
                {
                    continue;
                }

                _buttonInteractableCache[button] = button.interactable;
                button.interactable = false;
            }
        }

        private void RestoreSceneButtons()
        {
            foreach (KeyValuePair<Button, bool> entry in _buttonInteractableCache)
            {
                if (entry.Key != null)
                {
                    entry.Key.interactable = entry.Value;
                }
            }

            _buttonInteractableCache.Clear();
        }

        private void ShowBootstrapErrorPopup(Exception exception, string fallbackMessage)
        {
            string errorMessage = MultiplayerServicesBootstrap.Instance.LastErrorMessage;
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = exception != null && exception.InnerException != null
                    ? exception.InnerException.Message
                    : exception != null ? exception.Message : string.Empty;
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = fallbackMessage;
            }

            TMP_Text popupMessageText = _popupMessageTextField.GetValue(_titleSceneController) as TMP_Text;
            if (popupMessageText != null)
            {
                popupMessageText.text = errorMessage;
            }

            GameObject wrongKeyPopup = _wrongKeyPopupField.GetValue(_titleSceneController) as GameObject;
            if (wrongKeyPopup != null)
            {
                wrongKeyPopup.SetActive(true);
            }
        }

        private static void SetLabel(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }
    }
}
