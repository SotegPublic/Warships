using PlayFab.ClientModels;
using PlayFab;
using System;
using UnityEngine;
using PlayFab.Json;
using Configs;
using GameUI;
using Photon.Pun;
public class LobbyViewController : IViewController
{
    public Action<WindowsTypes> OnOpenWindowByType;
    public Action<SceneType> OnOpenStartScene;

    private LobbyView _lobbyView;
    private LobbyGamesManager _lobbyGamesManager;
    private SettingsManager _settingsManager;
    private PlayerPrefsController _prefsController;
    private string _currentUserID;
    private AvatarConfigsHolder _avatarsConfig;
    private AvatarConfigWithEmoji _currentAvatarConfig;

    public LobbyViewController (Canvas mainCanvas, AvatarConfigsHolder avatarsConfig, SettingsManager settingsManager, LobbyGamesManager lobbyGamesManager)
    {
        _prefsController = new PlayerPrefsController ();
        _lobbyView = mainCanvas.GetComponentInChildren<LobbyView>();
        _lobbyGamesManager = lobbyGamesManager;
        _settingsManager = settingsManager;
        _avatarsConfig = avatarsConfig;
    }

    public void Init()
    {
        SubscribeOnButtons();

        ChangeStateSortByRatingButton();

        _lobbyView.StartInit();
        _lobbyView.SetDefaultViewState();
    }

    private void SubscribeOnButtons()
    {       
        _lobbyView.SettingsButton.onClick.AddListener(SettingsButtonClick);
        _lobbyView.ShopButton.onClick.AddListener(ShopButtonClick);
        _lobbyView.CastomizationButton.onClick.AddListener(CastomizationButtonClick);

        _lobbyView.AcceptPanelExitButton.onClick.AddListener(AcceptExitButtonClick);
        _lobbyView.MainExitButton.onClick.AddListener(ExitButtonClick);

        _lobbyView.StartClassicButton.onClick.AddListener(StartClassicButtonClick);
        _lobbyView.StartRoyalButton.onClick.AddListener(StartRoyalButtonClick);

        _lobbyView.GameTypeSortButton.onClick.AddListener(ChangeStateSortByGameTypeButton);
        _lobbyView.RaitingSortButton.onClick.AddListener(ChangeStateSortByRatingButton);

    }

    private void AcceptExitButtonClick()
    {
        _lobbyView.SetDefaultViewState();

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.ForgetAllCredentials();
        }

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        _prefsController.SetLoggedPrefs(false);
        OnOpenStartScene?.Invoke(SceneType.StartScene);
    }

    private void ChangeStateSortByRatingButton()
    {
        if(_lobbyView.IsRatingSortUp)
        {
            SetGameTypeUp();
        }
        else
        {
            SetRatingUp();
        }
    }

    private void ChangeStateSortByGameTypeButton()
    {
        if (_lobbyView.IsGameTypeSortUp)
        {
            SetRatingUp();
        }
        else
        {
            SetGameTypeUp();
        }
    }

    private void SetGameTypeUp()
    {
        _lobbyView.SetHighestDownSortingState(SortigButtonsTypes.Raiting);

        _lobbyView.SetHighestUpSortingState(SortigButtonsTypes.GameType);

        _lobbyGamesManager.LoadRoomList();
    }

    private void SetRatingUp()
    {
        _lobbyView.SetHighestUpSortingState(SortigButtonsTypes.Raiting);

        _lobbyView.SetHighestDownSortingState(SortigButtonsTypes.GameType);

        _lobbyGamesManager.LoadRoomList();
    }

    private void StartRoyalButtonClick()
    {
        CloudScriptFunctions.StartCloudScriptAndGetData(_currentUserID, CloudScriptFunctions.GET_PLAYER_WINRATE, result =>
        {
            var rating = "";
            var winrate = "";

            GetWinRateDataFromJSON(result, ref rating, ref winrate);

            _lobbyGamesManager.CreateRoom(false, _lobbyView.VisibilityToggle.isOn, rating, winrate);
        },
        OnError);
        
    }

    private void StartClassicButtonClick()
    {
        CloudScriptFunctions.StartCloudScriptAndGetData(_currentUserID, CloudScriptFunctions.GET_PLAYER_WINRATE, result =>
        {
            var rating = "";
            var winrate = "";

            GetWinRateDataFromJSON(result, ref rating, ref winrate);

            _lobbyGamesManager.CreateRoom(true, _lobbyView.VisibilityToggle.isOn, rating, winrate);
        },
        OnError);
    }

    private void GetWinRateDataFromJSON(ExecuteCloudScriptResult result, ref string rating, ref string winrate)
    {
        JsonObject jsonResult = (JsonObject)result.FunctionResult;

        var wons = "";
        var losses = "";

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

        winrate = wons + "/" + losses;
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log("Ошибка скрипта: " + error.ErrorMessage);
    }

    private void ShopButtonClick()
    {
        OnOpenWindowByType?.Invoke(WindowsTypes.Shop);
    }

    private void CastomizationButtonClick()
    {
        OnOpenWindowByType?.Invoke(WindowsTypes.FullCastomization);
    }

    private void SettingsButtonClick()
    {
        _settingsManager.OpenSettingsScreen();
    }

    private void ExitButtonClick()
    {
        _lobbyView.SetDefaultViewState();

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.ForgetAllCredentials();
        }

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        _prefsController.SetLoggedPrefs(false);
        OnOpenStartScene?.Invoke(SceneType.StartScene);
    }

    public void InitLobbyView(string playerID)
    {
        _currentUserID = playerID;
    }

    public void SetViewInDefault()
    {
        _lobbyView.SetDefaultViewState();
    }

    public void UpdateController()
    {

    }

    public void OpenView()
    {
        LoadPlayerLobbyParameters();
    }

    public void CloseView()
    {
        _lobbyView.CloseWindow();
    }

    public void LoadPlayerLobbyParameters()
    {
        var request = new ExecuteCloudScriptRequest
        {

            FunctionName = CloudScriptFunctions.GET_DATA_FOR_LOBBY,
            FunctionParameter = new
            {
                currentPlayerId = _currentUserID
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, SetLobbyPlayerParameters, Error);
    }

    private void Error(PlayFabError error)
    {
        Debug.LogError(error.ErrorMessage);
    }

    private void SetLobbyPlayerParameters(ExecuteCloudScriptResult result)
    {
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        
        var rating = "";
        var name = "";
        Sprite avatar = default;

        if (jsonResult.TryGetValue(PlayFabConstants.PLAYER_RATING, out var ratingValue))
        {
            rating = Convert.ToString(ratingValue);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.AVATAR, out var avatarID))
        {
            _currentAvatarConfig = _avatarsConfig.GetAvatarConfigByStringKey(Convert.ToString(avatarID));
            avatar = _currentAvatarConfig.Sprite;
        }

        if(jsonResult.TryGetValue("Display_Name", out var displayName))
        {
            name = Convert.ToString(displayName);
        }

        _lobbyView.SetPlayerData(avatar, name, rating);
        _lobbyView.ShowCanvas();
    }

    public void ClearSubscribes()
    {
        _lobbyView.SettingsButton.onClick.RemoveAllListeners();
        _lobbyView.ShopButton.onClick.RemoveAllListeners();
        _lobbyView.CastomizationButton.onClick.RemoveAllListeners();

        _lobbyView.AcceptPanelExitButton.onClick.RemoveAllListeners();
        _lobbyView.MainExitButton.onClick.RemoveAllListeners();

        _lobbyView.StartClassicButton.onClick.RemoveAllListeners();
        _lobbyView.StartRoyalButton.onClick.RemoveAllListeners();

        _lobbyView.GameTypeSortButton.onClick.RemoveAllListeners();
        _lobbyView.RaitingSortButton.onClick.RemoveAllListeners();

        _lobbyView.ClearSubscribes();
    }
}
