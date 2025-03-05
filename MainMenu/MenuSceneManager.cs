using Configs;
using GameUI;
using Localization;
using SoundSystem;
using System.Collections.Generic;
using UnityEngine;
using ShopSystem;
using PlayFab.ClientModels;
using PlayFab;
using System;

public class MenuSceneManager : MonoBehaviour
{  
    [SerializeField] private Canvas _mainCanvas;

    private ConfigsHolder _configs;
    private SoundManager _soundManager;
    private SettingsManager _settingsManager;
    private LocalizationManager _localizationManager;

    private StartLoginWindowController _startLoginWindowController;
    private CreateAccountWindowController _createAccountWindowController;
    private EmailVerificationWindowController _emailVerificationWindowController;
    private SignInWindowController _siginWindowController;
    private LobbyViewController _lobbyViewController;
    private LobbyGamesManager _lobbyGamesManager;
    private CastomizationController _castomizationController;
    private PlayFabLoginController _loginController;
    private ShopManager _shopManager;
    private ScenesLoaderController _scenesLoader;
    private IViewController _activeViewController;
    //private ILoginWindow _currentOpenedWindow;
    //private Dictionary<LoginWindowsTypes ,ILoginWindow> _loginWindowsByType;
    private string _currentUserEmail;
    private string _currentUserPlayFabID;
    private string _currentUserDisplayName;
    private string _currentUserAvatarID;
    private string _currentUserRating;
    private string _currentUserWinrate;
    private PlayerPrefsController _prefsController = new PlayerPrefsController();

    private void Awake()
    {
        _loginController = new PlayFabLoginController();
        _loginController.OnLogin += OnLoginSuccess;
        _loginController.OnLoginError += OnLoginError;

        _configs = Resources.Load<ConfigsHolder>("ConfigsHolder");
        _soundManager = FindObjectOfType<SoundManager>();
        _localizationManager = new LocalizationManager(_configs, _mainCanvas);
        _settingsManager = new SettingsManager(_soundManager, _localizationManager);
        _settingsManager.OnLogout += PlayerClickLogoutButton;

        _startLoginWindowController = new StartLoginWindowController(_mainCanvas);
        _createAccountWindowController = new CreateAccountWindowController(_mainCanvas);
        _emailVerificationWindowController = new EmailVerificationWindowController(_mainCanvas);
        _siginWindowController = new SignInWindowController(_mainCanvas);
        _castomizationController = new CastomizationController(_mainCanvas, _configs, _localizationManager);

        _lobbyGamesManager = _mainCanvas.GetComponentInChildren<LobbyGamesManager>();
        _lobbyGamesManager.Init();

        _lobbyViewController = new LobbyViewController(_mainCanvas, _configs.AvatarConfigsHolder, _settingsManager, _lobbyGamesManager);
        _shopManager = new ShopManager(_mainCanvas, _configs, _localizationManager);

        _scenesLoader = new ScenesLoaderController(_mainCanvas);
    }

    private void PlayerClickLogoutButton()
    {
        OpenWindowOfType(WindowsTypes.StartLogin);
        _lobbyViewController.SetViewInDefault();
    }

    private void Start()
    {
        _settingsManager.StartInit(_mainCanvas.transform);
        _localizationManager.FindAllObjectsWithLocalizationType();
        _settingsManager.CheckVolumeSettings();
        _settingsManager.CheckLocalizationSettings();
        _soundManager.SubscribeUIElements(_mainCanvas);
        _settingsManager.SetDefaultState();
        _scenesLoader.CompleteLoadScene();

        _startLoginWindowController.OnOpenWindowByType += OpenWindowOfType;                         //todo не забыть запихать отписки
        _startLoginWindowController.OnOpenStartScene += _scenesLoader.LoadScene;

        _siginWindowController.OnOpenWindowByType += OpenWindowOfType;
        _siginWindowController.OnSignIn += SetCurrentUserParameters;
        _siginWindowController.OnOpenStartScene += _scenesLoader.LoadScene;

        _createAccountWindowController.OnOpenWindowByType += OpenWindowOfType;
        _createAccountWindowController.OnOpenStartScene += _scenesLoader.LoadScene;
        _createAccountWindowController.OnRegistrationDone += SetCurrentUserParameters;

        _emailVerificationWindowController.OnOpenWindowByType += OpenWindowOfType;
        _emailVerificationWindowController.OnOpenStartScene += _scenesLoader.LoadScene;

        _castomizationController.OnOpenWindowByType += OpenWindowOfType;
        _castomizationController.OnAcceptCastomizationChanges += AcceptCastomizationChanges;

        _lobbyViewController.Init();
        _lobbyViewController.OnOpenWindowByType += OpenWindowOfType;
        _lobbyViewController.OnOpenStartScene += _scenesLoader.LoadScene;

        _shopManager.OnOpenWindowByType += OpenWindowOfType;

        if (!PlayerPrefs.HasKey(PlayFabConstants.REMEMBER_LOGIN_PREFS_STRING))
        {
            _prefsController.SetRememberLogin(false);
            _prefsController.ResetLoginPrefs();
        }

        var isLoginRemembered = PlayerPrefs.GetInt(PlayFabConstants.REMEMBER_LOGIN_PREFS_STRING) == 0 ? false : true;

        if (isLoginRemembered)
        {
            AutoSignIn();
        }
        else
        {
            OpenWindowOfType(WindowsTypes.StartLogin);
        }
    }

    private void Update()
    {
        if (_activeViewController == null) return;

        _activeViewController.UpdateController();
    }

    private void AutoSignIn()
    {
        var email = PlayerPrefs.GetString(PlayFabConstants.LOGIN_PREFS_STRING);
        var password = PlayerPrefs.GetString(PlayFabConstants.PASSWORD_PREFS_STRING);
        _currentUserEmail = email;
        _loginController.Login(email, password);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"Success: {_currentUserEmail}");

        var statistics = result.InfoResultPayload.PlayerStatistics;
        string wons = "";
        string losses = "";
        string currentUserRating = "";
        string currentUserExperience = "";
        string currentUserLevel = "";

        for (int i = 0; i < statistics.Count; i++)
        {
            if (statistics[i].StatisticName == PlayFabConstants.PLAYER_RATING)
            {
                currentUserRating = statistics[i].Value.ToString();
            }
            if(statistics[i].StatisticName == PlayFabConstants.WONS)
            {
                wons = statistics[i].Value.ToString();
            }
            if (statistics[i].StatisticName == PlayFabConstants.LOSSES)
            {
                losses = statistics[i].Value.ToString();
            }
            if (statistics[i].StatisticName == PlayFabConstants.EXPERIENCE)
            {
                currentUserExperience = statistics[i].Value.ToString();
            }
            if (statistics[i].StatisticName == PlayFabConstants.LEVEL)
            {
                currentUserLevel = statistics[i].Value.ToString();
            }
        }

        var currentUserWinrate = wons + "/" + losses;
        var currentUserDisplayName = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        var currentUserAvatarID = result.InfoResultPayload.UserData[PlayFabConstants.AVATAR].Value;

        var playerLoginData = new PlayerLoginData(_currentUserEmail, currentUserDisplayName, result.PlayFabId, currentUserAvatarID, currentUserRating,
            currentUserWinrate, currentUserExperience, currentUserLevel);

        _prefsController.SetLoggedPrefs(true);
        SetCurrentUserParameters(playerLoginData);
        CheckVerificationStatus(result.InfoResultPayload.PlayerStatistics);
    }

    private void OnLoginError(PlayFabError error)
    {
        Debug.Log($"Fail: {error.ErrorMessage}");

        //ToDo: Сделать вывод сообщения в случае ошибки верификации

        OpenWindowOfType(WindowsTypes.StartLogin);
    }

    private void CheckVerificationStatus(List<StatisticValue> playerStatistics)
    {
        for (int i = 0; i < playerStatistics.Count; i++)
        {
            if (playerStatistics[i].StatisticName == PlayFabConstants.IS_VERIFIED_PARAMETER)
            {
                if (playerStatistics[i].Value == 0)
                {
                    OpenWindowOfType(WindowsTypes.Verification);
                }
                else
                {
                    OpenWindowOfType(WindowsTypes.Lobby);
                }
            }
        }
    }

    private void SetCurrentUserParameters(PlayerLoginData playerLoginData)
    {
        _currentUserEmail = playerLoginData.PlayerEmail;
        _currentUserPlayFabID = playerLoginData.PlayFabID;
        _currentUserDisplayName = playerLoginData.PlayerName;
        _currentUserAvatarID = playerLoginData.PlayerAvatarID;

        _prefsController.SetDisplayNamePrefs(_currentUserDisplayName);
        _prefsController.SetAvatarPrefs(_currentUserAvatarID);
        _lobbyGamesManager.Connect(playerLoginData);
        _lobbyViewController.InitLobbyView(_currentUserPlayFabID);
        SetCurrentUserParametersForEmailVerificationWindowController(_currentUserEmail, _currentUserPlayFabID);
    }

    private void SetCurrentUserParametersForEmailVerificationWindowController(string email, string playFabID)
    {
        _emailVerificationWindowController.SetCurrentUserParameters(email, playFabID);
    }

    private void AcceptCastomizationChanges(string avatarID, string displayName)
    {
        _currentUserAvatarID = avatarID;
        _currentUserDisplayName = displayName;

        if(_lobbyGamesManager.IsConnected)
        {
            _lobbyGamesManager.UpdatePlayerCustomProperties(displayName, avatarID);
        }
    }

    private void OpenWindowOfType(WindowsTypes type)
    {
        if (_activeViewController != null)
            _activeViewController.CloseView();

        switch (type)
        {
            case WindowsTypes.None:
                throw new System.Exception($"Type None in MenuSceneManager -> OpenWindowOfType");
            case WindowsTypes.StartScene:
                //_scenesLoader.LoadScene(SceneType.StartScene);
                break;
            case WindowsTypes.StartLogin:
                _startLoginWindowController.OpenView();
                _activeViewController = _startLoginWindowController;
                break;
            case WindowsTypes.SignIn:
                _siginWindowController.OpenView();
                _activeViewController = _siginWindowController;
                break;
            case WindowsTypes.CreateAccount:
                _createAccountWindowController.OpenView();
                _activeViewController = _createAccountWindowController;
                break;
            case WindowsTypes.Characters:
                //помойму оно уже накуй не упало
                break;
            case WindowsTypes.Lobby:
                _lobbyViewController.OpenView();
                _activeViewController = _lobbyViewController;
                break;
            case WindowsTypes.CreateRoom:
                break;
            case WindowsTypes.Room:
                break;
            case WindowsTypes.Verification:
                _emailVerificationWindowController.OpenView();
                _activeViewController = _emailVerificationWindowController;
                break;
            case WindowsTypes.SimpleCastomization:
                _castomizationController.SetSimpleActiveView();
                _castomizationController.InitPlayerInfo(_currentUserPlayFabID, _currentUserAvatarID, _currentUserDisplayName);
                _castomizationController.OpenView();
                _activeViewController = _castomizationController;
                break;
            case WindowsTypes.FullCastomization:
                _castomizationController.SetFullActiveView();
                _castomizationController.InitPlayerInfo(_currentUserPlayFabID, _currentUserAvatarID, _currentUserDisplayName);
                _castomizationController.OpenView();
                _activeViewController = _castomizationController;
                break;
            case WindowsTypes.Shop:
                _shopManager.InitPlayerInfo(_currentUserPlayFabID, _currentUserAvatarID);
                _shopManager.OpenView();
                _activeViewController = _shopManager;
                break;
            case WindowsTypes.StartGameScene:
                //_scenesLoader.LoadScene(SceneType.GameScene);
                break;
            default:
                throw new System.Exception($"Type default in MenuSceneManager -> OpenWindowOfType");
        }   
    }

    private void OnDestroy()
    {
        _startLoginWindowController.ClearSubscribes();
        _settingsManager.OnLogout -= PlayerClickLogoutButton;
        _createAccountWindowController.ClearSubscribes();
        _siginWindowController.ClearSubscribes();
        _emailVerificationWindowController.ClearSubscribes();
        _lobbyGamesManager.ClearSubscribes();
        _lobbyViewController.ClearSubscribes();
        _shopManager.ClearSubscribes();
    }
}