using PlayFab.ClientModels;
using PlayFab;
using System;
using UnityEngine;
using PlayFab.AdminModels;
using PlayFab.ProfilesModels;
using System.Collections.Generic;
using ExecuteCloudScriptResult = PlayFab.ClientModels.ExecuteCloudScriptResult;
using PlayFab.Json;

public class CreateAccountWindowController : IViewController
{
    public Action<SceneType> OnOpenStartScene;
    public Action<WindowsTypes> OnOpenWindowByType;
    public Action<PlayerLoginData> OnRegistrationDone;

    private CreateAccountWindow _createAccountWindow;
    private string _password;
    private string _confirmPassword;
    private string _email;
    private string _playFabID;
    private string _newPlayerName;
    private PlayFabAdminInstanceAPI _adminAPI;
    private PlayerPrefsController _prefsController;

    private Action _onAccountParametersChange;
    private bool _isEmailValid;
    private bool _isPasswordValid;
    private bool _isConfirmPasswordValid;

    private bool _isPasswordShow;
    private bool _isConfirmPasswordShow;

    public CreateAccountWindowController(Canvas mainCanvas)
    {
        _prefsController = new PlayerPrefsController();
        _createAccountWindow = mainCanvas.GetComponentInChildren<CreateAccountWindow>();
        _createAccountWindow.ConfirmButton.interactable = false;

        SubscribeUIElements();

        var adminApiSettings = new PlayFabApiSettings()
        {
            TitleId = PlayFabConstants.TITLE_ID,
            DeveloperSecretKey = PlayFabConstants.DEVELOPER_SECRET_KEY,
        };

        _adminAPI = new PlayFabAdminInstanceAPI(adminApiSettings);
    }

    private void SubscribeUIElements()
    {
        _onAccountParametersChange += ChangeConfirmButtonInteractableStatus;
        _createAccountWindow.OnEmailInputEnd += CheckEmailInput;

        _createAccountWindow.ExitButton.onClick.AddListener(OpenStartScene);
        _createAccountWindow.PasswordField.onValueChanged.AddListener(UpdatePassword);
        _createAccountWindow.ConfirmPasswordField.onValueChanged.AddListener(UpdateConfirmPassword);
        _createAccountWindow.EmailField.onValueChanged.AddListener(UpdateEmail);
        _createAccountWindow.ConfirmButton.onClick.AddListener(CreateAccount);
        _createAccountWindow.BackButton.onClick.AddListener(BackButtonClick);
        _createAccountWindow.ShowPasswordButton.onClick.AddListener(ShowOrHidePassword);
        _createAccountWindow.ShowConfirmPasswordButton.onClick.AddListener(ShowOrHideConfirmPassword);
    }

    private void ShowOrHidePassword()
    {
        if (_isPasswordShow)
        {
            _createAccountWindow.PasswordField.contentType = TMPro.TMP_InputField.ContentType.Password;
            _createAccountWindow.PasswordButtonShowImage.enabled = false;
            _createAccountWindow.PasswordButtonHideImage.enabled = true;
            _isPasswordShow = false;
        }
        else
        {
            _createAccountWindow.PasswordField.contentType = TMPro.TMP_InputField.ContentType.Standard;
            _createAccountWindow.PasswordButtonShowImage.enabled = true;
            _createAccountWindow.PasswordButtonHideImage.enabled = false;
            _isPasswordShow = true;
        }

        _createAccountWindow.PasswordField.textComponent.SetAllDirty();
    }

    private void ShowOrHideConfirmPassword()
    {
        if (_isConfirmPasswordShow)
        {
            _createAccountWindow.ConfirmPasswordField.contentType = TMPro.TMP_InputField.ContentType.Password;
            _createAccountWindow.ConfirmPasswordButtonShowImage.enabled = false;
            _createAccountWindow.ConfirmPasswordButtonHideImage.enabled = true;
            _isConfirmPasswordShow = false;
        }
        else
        {
            _createAccountWindow.ConfirmPasswordField.contentType = TMPro.TMP_InputField.ContentType.Standard;
            _createAccountWindow.ConfirmPasswordButtonShowImage.enabled = true;
            _createAccountWindow.ConfirmPasswordButtonHideImage.enabled = false;
            _isConfirmPasswordShow = true;
        }

        _createAccountWindow.ConfirmPasswordField.textComponent.SetAllDirty();
    }

    private void ChangeConfirmButtonInteractableStatus()
    {
        if (_isEmailValid && _isPasswordValid && _isConfirmPasswordValid)
        {
            _createAccountWindow.ConfirmButton.interactable = true;
        }
        else
        {
            _createAccountWindow.ConfirmButton.interactable = false;
        }
    }

    private void UpdatePassword(string password)
    {
        if (!_createAccountWindow.IsWindowShow) return;
        _password = password;

        if (_password.Length < 6)
        {
            _isPasswordValid = false;
            _onAccountParametersChange?.Invoke();
            _createAccountWindow.PasswordErrorText.enabled = true;
            _createAccountWindow.ConfirmPasswordErrorText.enabled = false;
            _createAccountWindow.PasswordAcceptImage.enabled = false;
        }
        else
        {
            _isPasswordValid = true;
            _onAccountParametersChange?.Invoke();
            _createAccountWindow.PasswordAcceptImage.enabled = true;
            _createAccountWindow.PasswordErrorText.enabled = false;

            if (_password != _confirmPassword)
            {
                _isConfirmPasswordValid = false;
                _onAccountParametersChange?.Invoke();
                _createAccountWindow.ConfirmPasswordErrorText.enabled = true;
                _createAccountWindow.ConfirmPasswordAcceptImage.enabled = false;
            }
            else
            {
                _isConfirmPasswordValid = true;
                _onAccountParametersChange?.Invoke();
                _createAccountWindow.ConfirmPasswordErrorText.enabled = false;
                _createAccountWindow.ConfirmPasswordAcceptImage.enabled = true;
            }
        }
    }

    private void UpdateConfirmPassword(string confirmPassword)
    {
        if (!_createAccountWindow.IsWindowShow) return;
        _confirmPassword = confirmPassword;

        if (_password.Length < 6) return;

        if (_password == _confirmPassword)
        {
            _isConfirmPasswordValid = true;
            _onAccountParametersChange?.Invoke();
            _createAccountWindow.ConfirmPasswordErrorText.enabled = false;
            _createAccountWindow.ConfirmPasswordAcceptImage.enabled = true;
        }
        else
        {
            _isConfirmPasswordValid = false;
            _onAccountParametersChange?.Invoke();
            _createAccountWindow.ConfirmPasswordErrorText.enabled = true;
            _createAccountWindow.ConfirmPasswordAcceptImage.enabled = false;
        }
    }

    private void UpdateEmail(string email)
    {
        if (!_createAccountWindow.IsWindowShow) return;
        _email = email;
        _createAccountWindow.StartWaitCheckState();
        _isEmailValid = false;
        _onAccountParametersChange?.Invoke();
        _createAccountWindow.EmailAcceptImage.enabled = false;
    }

    private void CheckEmailInput()
    {
        _adminAPI.GetUserAccountInfo(new LookupUserAccountInfoRequest
        {
            Email = _email,
        }, OnUserFound, OnUserNotFoundOrError);

        void OnUserFound(LookupUserAccountInfoResult obj)
        {
            _isEmailValid = false;
            _onAccountParametersChange?.Invoke();
            _createAccountWindow.EmailIsBusyText.enabled = true;
            _createAccountWindow.EmailAcceptImage.enabled = false;
        }

        void OnUserNotFoundOrError(PlayFabError obj)
        {
            if (obj.Error == PlayFabErrorCode.AccountNotFound)
            {
                _isEmailValid = true;
                _onAccountParametersChange?.Invoke();
                _createAccountWindow.EmailErrorText.enabled = false;
                _createAccountWindow.EmailAcceptImage.enabled = true;
            }
            else if (obj.Error == PlayFabErrorCode.InvalidParams)
            {
                _isEmailValid = false;
                _onAccountParametersChange?.Invoke();
                _createAccountWindow.EmailErrorText.enabled = true;
                _createAccountWindow.EmailAcceptImage.enabled = false;
            }
        };
    }

    private void CreateAccount()
    {
        _createAccountWindow.ConfirmButton.interactable = false;

        _prefsController.SetRememberLogin(false);
        _prefsController.ResetLoginPrefs();

        PlayFabClientAPI.RegisterPlayFabUser(
            new RegisterPlayFabUserRequest
            {
                Email = _email,
                Password = _password,
                RequireBothUsernameAndEmail = false
            },
            result =>
            {
                Debug.Log($"Success: {_email}");
                _playFabID = result.PlayFabId;

                ExecutUpdateParametersFunction();
            },
            error =>
            {
                Debug.Log("У нас тут затычка для ошибки сработала. АЛАРМ!!! Репорт:");
                OnOpenWindowByType?.Invoke(WindowsTypes.StartLogin);
                //todo: Сделать вывод сообщения об ошибке регистрации
            }
        );
    }

    private void ExecutUpdateParametersFunction()
    {

        var request = new ExecuteCloudScriptRequest
        {

            FunctionName = CloudScriptFunctions.SET_START_PARAMETERS,
            FunctionParameter = new
            {
                currentPlayerId = _playFabID
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, SetAccauntSettings, error => Debug.Log("CloudScriptError"));
    }

    private void SetAccauntSettings(ExecuteCloudScriptResult result)
    {
        JsonObject jsonResult = (JsonObject)result.FunctionResult;

        var wons = "";
        var losses = "";
        var rating = "";
        var experience = "";
        var level = "";
        var playerNumber = "";

        if (jsonResult.TryGetValue(PlayFabConstants.PLAYER_RATING, out var ratingValue))
        {
            rating = Convert.ToString(ratingValue);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.WONS, out var wonsCount))
        {
            wons = Convert.ToString(wonsCount);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.LOSSES, out var lossesCount))
        {
            losses = Convert.ToString(lossesCount);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.EXPERIENCE, out var playerExperience))
        {
            experience = Convert.ToString(playerExperience);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.LEVEL, out var playerLevel))
        {
            level = Convert.ToString(playerLevel);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.PLAYER_NUMBER, out var number))
        {
            playerNumber = Convert.ToString(number);
        }

        var winrate = wons + "/" + losses;


        SetProfileLanguage();

        void SetProfileLanguage()
        {
            var languageRequest = new SetProfileLanguageRequest
            {
                Language = PlayFabConstants.EN_LENGUAGE // сюда добавить установку языка из выбранного в клиенте
            };



            PlayFabProfilesAPI.SetProfileLanguage(languageRequest,
                result =>
                {
                    Debug.Log("Done");
                    SetContactEmail();
                },
                OnError
            );
        }

        void SetContactEmail()
        {
            var request = new AddOrUpdateContactEmailRequest
            {
                EmailAddress = _email
            };

            PlayFabClientAPI.AddOrUpdateContactEmail(request,
                result =>
                {
                    SetDisplayName();
                    Debug.Log("Была обновлена контактная почта");
                },
                OnError
            );
        }

        void SetDisplayName()
        {
            _newPlayerName = "UID#" + playerNumber;
            var displayNameRequest = new PlayFab.ClientModels.UpdateUserTitleDisplayNameRequest
            {
                DisplayName = _newPlayerName
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(
                displayNameRequest,
                result =>
                {
                    Debug.Log("Обновили имя: " + _newPlayerName);

                    OnOpenWindowByType.Invoke(WindowsTypes.Verification);

                    var playerLoginData = new PlayerLoginData(_email, _newPlayerName, _playFabID, PlayFabConstants.DEFAULT_AVATAR_ID, rating, winrate,
                        experience, level);

                    OnRegistrationDone?.Invoke(playerLoginData);
                    _prefsController.SetLoggedPrefs(true);
                    ResetWindowParameters();
                    _createAccountWindow.SetDefaultWindowElementsParameters();

                    var request = new ExecuteCloudScriptRequest
                    {

                        FunctionName = CloudScriptFunctions.SET_REGISTRATION_DONE,
                        FunctionParameter = new
                        {
                            currentPlayerId = _playFabID
                        }
                    };

                    PlayFabClientAPI.ExecuteCloudScript(request, result => Debug.Log("Регистрация завершена"), error => Debug.Log("CloudScriptError"));
                },
                OnError
            );
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogWarning("Что то пошло не так, вот инфо об ошибке:");
        Debug.LogError(error.GenerateErrorReport());

        OnOpenWindowByType?.Invoke(WindowsTypes.StartLogin);
        //todo: Сделать вывод сообщения об ошибке регистрации
    }

    private void BackButtonClick()
    {
        OnOpenWindowByType?.Invoke(WindowsTypes.StartLogin);
        ResetWindowParameters();
        _createAccountWindow.SetDefaultWindowElementsParameters();
    }

    private void ResetWindowParameters()
    {
        _email = default;
        _password = default;
        _confirmPassword = default;

        _isEmailValid = false;
        _isPasswordValid = false;
        _isConfirmPasswordValid = false;
        _isPasswordShow = false;
        _isConfirmPasswordShow = false;

        _onAccountParametersChange?.Invoke();
    }

    private void OpenStartScene()
    {
        OnOpenStartScene?.Invoke(SceneType.StartScene);
    }

    public void UpdateController()
    {

    }

    public void OpenView()
    {
        _createAccountWindow.OpenWindow();
    }

    public void CloseView()
    {
        _createAccountWindow.CloseWindow();
    }


    public void ClearSubscribes()
    {
        _onAccountParametersChange -= ChangeConfirmButtonInteractableStatus;
        _createAccountWindow.OnEmailInputEnd -= CheckEmailInput;

        _createAccountWindow.ExitButton.onClick.RemoveAllListeners();
        _createAccountWindow.PasswordField.onValueChanged.RemoveAllListeners();
        _createAccountWindow.ConfirmPasswordField.onValueChanged.RemoveAllListeners();
        _createAccountWindow.EmailField.onValueChanged.RemoveAllListeners();
        _createAccountWindow.ConfirmButton.onClick.RemoveAllListeners();
        _createAccountWindow.BackButton.onClick.RemoveAllListeners();
        _createAccountWindow.ShowPasswordButton.onClick.RemoveAllListeners();
        _createAccountWindow.ShowConfirmPasswordButton.onClick.RemoveAllListeners();
    }
}
