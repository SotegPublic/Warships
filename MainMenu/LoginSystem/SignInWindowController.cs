using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SignInWindowController: IViewController
{
    public Action<WindowsTypes> OnOpenWindowByType;
    public Action<PlayerLoginData> OnSignIn;
    public Action<SceneType> OnOpenStartScene;

    private SignInWindow _signInWindow;
    private string _email;
    private string _password;
    private bool _isPasswordShow;
    private string _currentPlayerID;
    private PlayFabLoginController _loginController;
    private PlayerPrefsController _prefsController;

    public SignInWindowController(Canvas mainCanvas)
    {
        _prefsController = new PlayerPrefsController();
        _loginController = new PlayFabLoginController();
        _signInWindow = mainCanvas.GetComponentInChildren<SignInWindow>();
        SubscribeElementsUI();
        _signInWindow.SetDefaultWindowElementsParameters();

        _loginController.OnLogin += OnLoginSuccess;
        _loginController.OnLoginError += OnLoginError;
    }

    public void UpdateController()
    {

    }

    private void SubscribeElementsUI()
    {
        _signInWindow.EmailField.onValueChanged.AddListener(UpdateEmail);
        _signInWindow.PasswordField.onValueChanged.AddListener(UpdatePassword);

        _signInWindow.SignInButton.onClick.AddListener(SignIn);
        _signInWindow.BackButton.onClick.AddListener(BackButtonClick);
        _signInWindow.ExitButton.onClick.AddListener(OpenStartScene);
        _signInWindow.ResetPasswordButton.onClick.AddListener(ResetPasswordClick);
        _signInWindow.PasswordVisibilityButton.onClick.AddListener(ShowOrHidePassword);

        _signInWindow.RecoveryStartPanelBackButton.onClick.AddListener(_signInWindow.HideStartRecoveryPanel);
        _signInWindow.RecoveryEndPanelBackButton.onClick.AddListener(_signInWindow.HideEndRecoveryPanel);
        _signInWindow.RecoveryStartPanelResetPasswordButton.onClick.AddListener(TrySendResetPasswordMail);
    }

    private void ShowOrHidePassword()
    {
        if (_isPasswordShow)
        {
            _signInWindow.PasswordField.contentType = TMP_InputField.ContentType.Password;
            _signInWindow.PasswordVisibilityButtonShowImage.enabled = false;
            _signInWindow.PasswordVisibilityButtonHideImage.enabled = true;
            _isPasswordShow = false;
        }
        else
        {
            _signInWindow.PasswordField.contentType = TMP_InputField.ContentType.Standard;
            _signInWindow.PasswordVisibilityButtonShowImage.enabled = true;
            _signInWindow.PasswordVisibilityButtonHideImage.enabled = false;
            _isPasswordShow = true;
        }

        _signInWindow.PasswordField.textComponent.SetAllDirty();
    }

    private void ResetPasswordClick()
    {
        _signInWindow.OpenRecoveryStartPanel();
    }

    private void TrySendResetPasswordMail()
    {
        var recoveryRequest = new SendAccountRecoveryEmailRequest
        {
            Email = _signInWindow.ResetPasswordEmailField.text,
            TitleId = PlayFabConstants.TITLE_ID
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(
            recoveryRequest,
            OnSendDone,
            OnSendError
            );

        void OnSendDone(SendAccountRecoveryEmailResult result)
        {
            Debug.Log("Письмо восстановления было отправлено");
            _signInWindow.OpenRecoveryEndPanel();
            _signInWindow.SetDefaultWindowElementsParameters();
        };

        void OnSendError(PlayFabError error)
        {
            Debug.Log(error.ErrorMessage); // сделать сообщение про неверное мыло для восстановление если понадобится
        }
    }

    private void UpdatePassword(string password)
    {
        _password = password;
    }

    private void UpdateEmail(string email)
    {
        _email = email;
    }

    private void SignIn()
    {
        _signInWindow.SignInButton.interactable = false;
        _signInWindow.EmailField.interactable = false;

        if(_signInWindow.ErrorText.enabled)
        {
            _signInWindow.ErrorText.enabled = false;
        }

        _loginController.Login(_email, _password);
    }

    private void OnLoginSuccess(LoginResult loginResult)
    {
        _currentPlayerID = loginResult.PlayFabId;

        var displayName = loginResult.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        var avatarID = loginResult.InfoResultPayload.UserData[PlayFabConstants.AVATAR].Value;

        var statistics = loginResult.InfoResultPayload.PlayerStatistics;
        string wons = "";
        string losses = "";
        string rating = "";
        string experience = "";
        string level = "";

        for (int i = 0; i < statistics.Count; i++)
        {
            if (statistics[i].StatisticName == PlayFabConstants.PLAYER_RATING)
            {
                rating = statistics[i].Value.ToString();
            }
            if (statistics[i].StatisticName == PlayFabConstants.WONS)
            {
                wons = statistics[i].Value.ToString();
            }
            if (statistics[i].StatisticName == PlayFabConstants.LOSSES)
            {
                losses = statistics[i].Value.ToString();
            }
            if (statistics[i].StatisticName == PlayFabConstants.EXPERIENCE)
            {
                experience = statistics[i].Value.ToString();
            }
            if (statistics[i].StatisticName == PlayFabConstants.LEVEL)
            {
                level = statistics[i].Value.ToString();
            }
        }

        var winrate = wons + "/" + losses;

        var playerLoginData = new PlayerLoginData(_email, displayName, _currentPlayerID, avatarID, rating, winrate,
            experience, level);

        OnSignIn?.Invoke(playerLoginData);
        _prefsController.SetLoggedPrefs(true);
        CheckAndSetAutoLoginPrefs();
        CheckVerificationStatus(loginResult.InfoResultPayload.PlayerStatistics);
    }

    private void OnLoginError(PlayFabError error)
    {
        {
            Debug.Log($"Fail: {error.ErrorMessage}");

            _signInWindow.SignInButton.interactable = true;
            _signInWindow.EmailField.interactable = true;

            if (error.Error == PlayFabErrorCode.InvalidEmailOrPassword)
            {
                _signInWindow.ErrorText.enabled = true;
            }
        }
    }

    private void CheckAndSetAutoLoginPrefs()
    {
        if (!_signInWindow.RememberMeToggle.isOn && PlayerPrefs.GetInt(PlayFabConstants.REMEMBER_LOGIN_PREFS_STRING) == 1)
        {
            _prefsController.SetRememberLogin(false);
            _prefsController.ResetLoginPrefs();
            _prefsController.ResetAvatarPrefs();
            _prefsController.ResetDisplayNamePrefs();
        }
        else if (_signInWindow.RememberMeToggle.isOn && PlayerPrefs.GetInt(PlayFabConstants.REMEMBER_LOGIN_PREFS_STRING) == 0)
        {
            _prefsController.SetRememberLogin(true);
            _prefsController.SetLoginPrefs(_email, _password, _currentPlayerID);
        }
    }

    private void CheckVerificationStatus(List<StatisticValue> playerStatistics)
    {
        _signInWindow.SetDefaultWindowElementsParameters();

        for(int i = 0; i < playerStatistics.Count; i++)
        {
            if (playerStatistics[i].StatisticName == PlayFabConstants.IS_VERIFIED_PARAMETER) 
            {
                if (playerStatistics[i].Value == 0)
                {
                    OnOpenWindowByType?.Invoke(WindowsTypes.Verification);
                }
                else
                {
                    OnOpenWindowByType?.Invoke(WindowsTypes.Lobby);
                }
            }
        }
    }

    private void OpenStartScene()
    {
        _signInWindow.SetDefaultWindowElementsParameters();
        OnOpenStartScene?.Invoke(SceneType.StartScene);
    }

    private void BackButtonClick()
    {
        _signInWindow.SetDefaultWindowElementsParameters();
        OnOpenWindowByType?.Invoke(WindowsTypes.StartLogin);
    }

    public void OpenView()
    {
        _signInWindow.OpenWindow();
    }

    public void CloseView()
    {
        _signInWindow.CloseWindow();
    }

    public void ClearSubscribes()
    {
        _loginController.OnLogin -= OnLoginSuccess;
        _loginController.OnLoginError -= OnLoginError;

        _signInWindow.EmailField.onValueChanged.RemoveAllListeners();
        _signInWindow.PasswordField.onValueChanged.RemoveAllListeners();

        _signInWindow.SignInButton.onClick.RemoveAllListeners();
        _signInWindow.BackButton.onClick.RemoveAllListeners();
        _signInWindow.ExitButton.onClick.RemoveAllListeners();
        _signInWindow.ResetPasswordButton.onClick.RemoveAllListeners();
        _signInWindow.PasswordVisibilityButton.onClick.RemoveAllListeners();

        _signInWindow.RecoveryStartPanelBackButton.onClick.RemoveAllListeners();
        _signInWindow.RecoveryEndPanelBackButton.onClick.RemoveAllListeners();
        _signInWindow.RecoveryStartPanelResetPasswordButton.onClick.RemoveAllListeners();
    }
}
