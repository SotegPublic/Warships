using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyGamesManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LobbyView _lobbyView;
    [SerializeField] private RoomView _roomView;
    [SerializeField] private GameObject _poolHolder;
    [SerializeField] private GameObject _miniRoomPrefab;
    [SerializeField] private PhotonView _photonView;

    private string _playerName;
    private string _currentUserID;
    private string _currentPlayerWinRate;
    private string _currentPlayerRating;
    private MiniRoomViewsSortingController _miniRoomViewsSortingController;
    private SearchPrivateGameController _searchController;
    private MiniRoomButtonsPool _miniRoomButtonsPool;
    private PlayerPrefsController _prefsController;
    private bool _isConnected;

    public bool IsConnected => _isConnected;

    public void Init()
    {
        _miniRoomButtonsPool = new MiniRoomButtonsPool(_poolHolder, _miniRoomPrefab);
        _miniRoomViewsSortingController = new MiniRoomViewsSortingController(_miniRoomButtonsPool);
        _searchController = new SearchPrivateGameController(_miniRoomButtonsPool);

        _lobbyView.JoinButton.onClick.AddListener(JoinToRoom);
        _lobbyView.SearchButton.onClick.AddListener(StartOrStopSearch);
        _lobbyView.SearchInputField.onValueChanged.AddListener(RestartSearchOnEdit);

        _roomView.StartGameButton.onClick.AddListener(StartGame);
        _roomView.BackButton.onClick.AddListener(LeaveRoom);
        _roomView.DeclineButton.onClick.AddListener(SendKickPlayer);
        _prefsController = new PlayerPrefsController();

        _miniRoomViewsSortingController.OnRoomMiniViewAdd += SubscribeViewOnRoomButton;
        _miniRoomViewsSortingController.OnRoomMiniViewDelete += UnsubscribeViewOnRoomButton;

        _searchController.OnRoomMiniViewAdd += SubscribeViewOnRoomButton;
        _searchController.OnRoomMiniViewDelete += UnsubscribeViewOnRoomButton;
    }

    #region PhotonLogic

    public void Connect(PlayerLoginData playerLoginData)
    {
        _playerName = playerLoginData.PlayerName;
        _currentUserID = playerLoginData.PlayFabID;
        _currentPlayerWinRate = playerLoginData.PlayerWinRate;
        _currentPlayerRating = playerLoginData.PlayerRating;

        PhotonNetwork.AuthValues = new AuthenticationValues(_currentUserID);
        PhotonNetwork.NickName = _playerName;
        PhotonNetwork.GameVersion = PhotonNetwork.AppVersion;

        AppSettings ruSettings = new AppSettings();
        ruSettings.UseNameServer = true;
        ruSettings.Server = "ns.photonengine.io";
        ruSettings.AppIdRealtime = "d3eaecdf-ce66-4783-a6af-2562974b9d51";

        PhotonNetwork.ConnectUsingSettings(ruSettings);
        //PhotonNetwork.ConnectToRegion("ru");
        PhotonNetwork.AutomaticallySyncScene = true;

        var playerCustomProperties = new Hashtable { { PhotonConstants.PLAYER_RATING, _currentPlayerRating }, { PhotonConstants.PLAYER_WINRATE, _currentPlayerWinRate },
            { PhotonConstants.PLAYER_EXPERIENCE, playerLoginData.PlayerExperience }, { PhotonConstants.PLAYER_LEVEL, playerLoginData.PlayerLevel},
            { PhotonConstants.PLAYER_AVATAR_ID, playerLoginData.PlayerAvatarID}, { PhotonConstants.PLAYER_TYPE, ((int)PhotonPlayerTypes.None).ToString() } };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);

        _isConnected = true;
    }

    private void StartGame()
    {
        SceneManager.LoadScene(2);
    }

    public void UpdatePlayerCustomProperties(string displayName, string avatarID)
    {
        PhotonNetwork.NickName = displayName;

        var playerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerCustomProperties[PhotonConstants.PLAYER_AVATAR_ID] = avatarID;

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
    }

    public void CreateRoom(bool isClassic, bool isPrivate, string ownerRating, string ownerWinRate)
    {
        var privateStatus = isPrivate ? PhotonConstants.PRIVATE : PhotonConstants.FREE;

        _currentPlayerRating = ownerRating;
        _currentPlayerWinRate = ownerWinRate;

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true,
            PublishUserId = true,
            EmptyRoomTtl = 5,
            PlayerTtl = 1000
        };

        if (isClassic)
        {
            var customRoomProperties = new Hashtable { { PhotonConstants.GAME_TYPE, PhotonConstants.CLASSIC_GAME }, { PhotonConstants.OWNER, _playerName },
                { PhotonConstants.OWNER_RATING, ownerRating}, { PhotonConstants.OWNER_WINRATE, ownerWinRate}, { PhotonConstants.PRIVATE_STATUS, privateStatus} };
            var customRoomPropertiesForLobby = new[] { PhotonConstants.GAME_TYPE, PhotonConstants.OWNER,
                PhotonConstants.OWNER_RATING, PhotonConstants.OWNER_WINRATE, PhotonConstants.PRIVATE_STATUS };

            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.CustomRoomPropertiesForLobby = customRoomPropertiesForLobby;
        }
        else
        {
            var customRoomProperties = new Hashtable { { PhotonConstants.GAME_TYPE, PhotonConstants.ROYAL_GAME }, { PhotonConstants.OWNER, _playerName },
                { PhotonConstants.OWNER_RATING, ownerRating}, { PhotonConstants.OWNER_WINRATE, ownerWinRate}, { PhotonConstants.PRIVATE_STATUS, privateStatus} };
            var customRoomPropertiesForLobby = new[] { PhotonConstants.GAME_TYPE, PhotonConstants.OWNER,
                PhotonConstants.OWNER_RATING, PhotonConstants.OWNER_WINRATE, PhotonConstants.PRIVATE_STATUS };

            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.CustomRoomPropertiesForLobby = customRoomPropertiesForLobby;
        }

        PhotonNetwork.CreateRoom(_playerName, roomOptions);
        _lobbyView.VisibilityToggle.isOn = false;

        Debug.Log("CreateRoom");
    }

    public void SendKickPlayer()
    {
        var opponent = _roomView.Opponent;

        _photonView.RPC("KickPlayer", opponent);
    }

    [PunRPC]
    private void KickPlayer()
    {
        var ownerName = _roomView.Owner.NickName;

        if (PhotonNetwork.LocalPlayer.NickName != ownerName)
        {
            LeaveRoom();
        }
    }

    #endregion

    #region LobbyLogic

    public void LoadRoomList()
    {
        if (_searchController.IsSearchActive && !_searchController.IsSearchComplite)
        {
            if (_searchController.TryFindPrivateRoom(_lobbyView.SearchInputField.text, out var roomMiniView))
            {
                _lobbyView.UpdateSearchResult(roomMiniView);
            }
        }
        else if (!_searchController.IsSearchActive)
        {
            if (_lobbyView.IsRatingSortUp)
            {
                LoadByRatingRoomList();
            }
            else
            {
                LoadByTypeRoomList();
            }
        }
    }

    private void LoadByRatingRoomList()
    {
        _lobbyView.UpdateRoomListView(_miniRoomViewsSortingController.RoomButtonsByRating);
    }

    private void LoadByTypeRoomList()
    {
        _lobbyView.UpdateRoomListView(_miniRoomViewsSortingController.RoomButtonsByType);
    }

    private void ShowRoomViewOnJoin()
    {
        _roomView.StartInit();

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            if (player.Value.NickName == _playerName)
            {
                _roomView.InitOpponent(_playerName, _currentPlayerWinRate, _currentPlayerRating, player.Value);
                _roomView.HideMasterClientButtons();
            }
            else
            {
                string playerRating = "";
                string playerWinrate = "";

                if (player.Value.CustomProperties.TryGetValue(PhotonConstants.PLAYER_RATING, out object rating))
                {
                    playerRating = $"{rating}";
                }

                if (player.Value.CustomProperties.TryGetValue(PhotonConstants.PLAYER_WINRATE, out object winrate))
                {
                    playerWinrate = $"{winrate}";
                }

                _roomView.InitOwner(player.Value.NickName, playerWinrate, playerRating, player.Value);
            }
        }

        _roomView.OpenWindow();
    }

    private void ShowRoomViewOnCreate()
    {
        _roomView.StartInit();
        _roomView.InitOwner(_playerName, _currentPlayerWinRate, _currentPlayerRating, PhotonNetwork.LocalPlayer);
        _roomView.OpenWindow();
    }

    private void JoinToRoom()
    {
        PhotonNetwork.JoinRoom(_lobbyView.SelectedRoom.RoomInfo.Name);

        if (_searchController.IsSearchActive)
        {
            _lobbyView.ChangeSearchButtonSprite(!_searchController.IsSearchActive);
            _lobbyView.ClearSearchInputField();

            _searchController.ResetSearch();
            LoadRoomList();
        }
    }

    private void LeaveRoom()
    {
        var ownerName = _roomView.Owner.NickName;

        if (PhotonNetwork.LocalPlayer.NickName == ownerName)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
            PhotonNetwork.CurrentRoom.PlayerTtl = 0;
        }

        PhotonNetwork.LeaveRoom();
        _roomView.CloseWindow();
        _roomView.SetDefaultState();
        _lobbyView.ShowCanvas();
    }

    #endregion

    #region PhononEvents
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
        Debug.Log("PlayfabConnected:" + PhotonNetwork.NickName);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        Debug.Log("OnJoinedLobby");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        _searchController.UpdateRoomList(roomList);
        _miniRoomViewsSortingController.UpdateRoomsLists(roomList);

        LoadRoomList();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        string playerRating = "";
        string playerWinrate = "";

        if (newPlayer.CustomProperties.TryGetValue(PhotonConstants.PLAYER_RATING, out object rating))
        {
            playerRating = $"{rating}";
        }

        if (newPlayer.CustomProperties.TryGetValue(PhotonConstants.PLAYER_WINRATE, out object winrate))
        {
            playerWinrate = $"{winrate}";
        }

        _roomView.InitOpponent(newPlayer.NickName, playerWinrate, playerRating, newPlayer);
        _roomView.ActivateMasterClientButtons();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        var ownerName = _roomView.Owner.NickName;

        if (otherPlayer.NickName == ownerName)
        {
            LeaveRoom();
        }
        else
        {
            _roomView.SetWaitingState();
        }
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        ShowRoomViewOnCreate();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        _lobbyView.CloseWindow();
        _lobbyView.CheckAndResetSelectedRoom();

        var ownerName = PhotonNetwork.CurrentRoom.CustomProperties[PhotonConstants.OWNER].ToString();

        if (PhotonNetwork.LocalPlayer.NickName != ownerName)
        {
            ShowRoomViewOnJoin();

            var playerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            playerCustomProperties[PhotonConstants.PLAYER_TYPE] = ((int)PhotonPlayerTypes.Client).ToString();

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
        }
        else
        {
            var playerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            playerCustomProperties[PhotonConstants.PLAYER_TYPE] = ((int)PhotonPlayerTypes.Server).ToString();

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
        }

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        _prefsController.SetLoggedPrefs(false);
        _isConnected = false;
    }

    #endregion

    #region SearchSystem

    private void StartOrStopSearch()
    {
        if (_lobbyView.SearchInputField.text == "") return;

        if (_searchController.IsSearchActive)
        {
            _lobbyView.ChangeSearchButtonSprite(!_searchController.IsSearchActive);
            _lobbyView.ClearSearchInputField();
            _lobbyView.ResetSearchResult();

            _searchController.ResetSearch();
            LoadRoomList();
        }
        else
        {
            _lobbyView.ChangeSearchButtonSprite(!_searchController.IsSearchActive);
            _lobbyView.ChangeRoomsPositions();

            _searchController.SetSearchActive();

            if (_searchController.TryFindPrivateRoom(_lobbyView.SearchInputField.text, out var roomMiniView))
            {
                _lobbyView.UpdateSearchResult(roomMiniView);
            }
        }
    }

    private void RestartSearchOnEdit(string inputText)
    {
        if (_lobbyView.SearchInputField.text == "") return;

        if(_searchController.IsSearchActive)
        {
            if(_searchController.IsSearchComplite)
            {
                _searchController.ResetSearch();
                _lobbyView.CheckAndResetSelectedRoom();
                _searchController.SetSearchActive();
            }

            if (_searchController.TryFindPrivateRoom(inputText, out var roomMiniView))
            {
                _lobbyView.UpdateSearchResult(roomMiniView);
            }
        }
    }

    #endregion

    private void SubscribeViewOnRoomButton(RoomMiniView roomMiniView)
    {
        _lobbyView.SubscribeOnNewButton(roomMiniView);
    }

    private void UnsubscribeViewOnRoomButton(RoomMiniView roomMiniView)
    {
        _lobbyView.UnsubscribeFromRemovedButton(roomMiniView);
    }

    public void ClearSubscribes()
    {
        _lobbyView.JoinButton.onClick.RemoveAllListeners();
        _lobbyView.SearchButton.onClick.RemoveAllListeners();
        _lobbyView.SearchInputField.onValueChanged.RemoveAllListeners();

        _roomView.StartGameButton.onClick.RemoveAllListeners();
        _roomView.BackButton.onClick.RemoveAllListeners();
        _roomView.DeclineButton.onClick.RemoveAllListeners();

        _lobbyView.UnsubscribeFromAllButtons(_miniRoomViewsSortingController.GetAllRooms());

        if (_searchController.IsSearchActive)
        {
            _searchController.ResetSearch();
        }

        _miniRoomViewsSortingController.OnRoomMiniViewAdd -= SubscribeViewOnRoomButton;
        _miniRoomViewsSortingController.OnRoomMiniViewDelete -= UnsubscribeViewOnRoomButton;
        _searchController.OnRoomMiniViewAdd -= SubscribeViewOnRoomButton;
        _searchController.OnRoomMiniViewDelete -= UnsubscribeViewOnRoomButton;

        _roomView.ClearSubscribes();
    }
}
