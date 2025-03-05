using Configs;
using DG.Tweening;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : MonoBehaviour
{
    [SerializeField] private Canvas _lobbyCanvas;
    [SerializeField] private Image _avatar;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _ratingCounterText;
    [SerializeField] private GameObject _acceptPanel;
    [SerializeField] private GameObject _battleSettingsPanel;
    [SerializeField] private Button _logOutButton;
    [SerializeField] private Button _acceptPanelStayButton;
    [SerializeField] private Image _raitingHighestDownImage;
    [SerializeField] private Image _raitingHighestUpImage;
    [SerializeField] private Image _gameTypeHighestDownImage;
    [SerializeField] private Image _gameTypeHighestUpImage;
    [SerializeField] private Button _battleSettingsExitButton;
    [SerializeField] private Button _createBattleButton;
    [SerializeField] private AvatarsConfig _avatarHolder;
    [SerializeField] private Transform _roomListTransform;
    [SerializeField] private Transform _poolTransform;

    [field: SerializeField] public TMP_InputField SearchInputField { get; private set; }
    [field: SerializeField] public Button SettingsButton { get; private set; }
    [field: SerializeField] public Button CastomizationButton { get; private set; }
    [field: SerializeField] public Button ShopButton { get; private set; }
    [field: SerializeField] public Button RaitingSortButton { get; private set; }
    [field: SerializeField] public Button GameTypeSortButton { get; private set; }
    [field: SerializeField] public Button JoinButton { get; private set; }
    [field: SerializeField] public Button MainExitButton { get; private set; }
    [field: SerializeField] public Button AcceptPanelExitButton { get; private set; }
    [field: SerializeField] public Toggle VisibilityToggle { get; private set; }
    [field: SerializeField] public Button StartClassicButton { get; private set; }
    [field: SerializeField] public Button StartRoyalButton { get; private set; }
    [field: SerializeField] public Button SearchButton { get; private set; }
    [field: SerializeField] public Image SearchImage { get; private set; }
    [field: SerializeField] public Sprite SearchSprite { get; private set; }
    [field: SerializeField] public Sprite CancelSearchSprite { get; private set; }

    public RoomMiniView SelectedRoom => _selectedRoom;
    public bool IsGameTypeSortUp { get; private set; }
    public bool IsRatingSortUp { get; private set; }

    private MissTapDetector _battleSettingsPanelMissTapDetector;
    private List<RoomMiniView> _roomButtons = new List<RoomMiniView>();
    private List<RoomMiniView> _searchingRoomButtons = new List<RoomMiniView>(1);
    private RoomMiniView _selectedRoom;


    public void StartInit() //еще _raitingCounterText и _playerNameText
    {
        _battleSettingsPanelMissTapDetector = _battleSettingsPanel.GetComponentInChildren<MissTapDetector>();
        SetJoinButtonInteractableState(false);

        _logOutButton.onClick.AddListener(ShowAcceptPanel);
        _acceptPanelStayButton.onClick.AddListener(() => _acceptPanel.SetActive(false));
        _createBattleButton.onClick.AddListener(() => _battleSettingsPanel.SetActive(true));
        _battleSettingsPanelMissTapDetector.OnMissClick += CloseBattleSettingsPanel;
        _battleSettingsExitButton.onClick.AddListener(CloseBattleSettingsPanel);
        StartClassicButton.onClick.AddListener(CloseBattleSettingsPanel);
        StartRoyalButton.onClick.AddListener(CloseBattleSettingsPanel);
    }

    public void SetDefaultViewState()
    {
        SetJoinButtonInteractableState(false);
        _acceptPanel.SetActive(false);
        _battleSettingsPanel.SetActive(false);
        VisibilityToggle.isOn = false;
    }

    public void SetPlayerData (Sprite avatar, string name, string rating)
    {
        SetUserName(name);
        SetRating(rating);
        SetAvatar(avatar);
    }

    public void ChangeSearchButtonSprite(bool isSearchStarted)
    {
        if(isSearchStarted)
        {
            SearchImage.sprite = CancelSearchSprite;
        }
        else
        {
            SearchImage.sprite = SearchSprite;
        }
    }

    public void ClearSearchInputField()
    {
        SearchInputField.text = "";
        SearchInputField.interactable = true;
    }

    public void ShowAcceptPanel()
    {
        _acceptPanel.SetActive(true);
    }

    public void ShowCanvas()
    {
        _lobbyCanvas.enabled = true;
    }

    public void CloseWindow()
    {
        _lobbyCanvas.enabled = false;
    }

    public void SetHighestUpSortingState(SortigButtonsTypes buttonType)
    {
        var (button, highestDownImage, highestUpImage) = InitButtonParameters(buttonType);

        if(buttonType == SortigButtonsTypes.Raiting)
        {
            IsRatingSortUp = true;
        }
        else
        {
            IsGameTypeSortUp = true;
        }

        DOTween.Kill("HighestDown " + button.GetHashCode());
        var sequence = DOTween.Sequence();
        sequence.SetId("HighestUp " + button.GetHashCode());
        sequence.Append(button.transform.DOLocalRotate(new Vector3(0, 0, 180), 0.3f));
        sequence.Join(highestDownImage.DOFade(0, 0.3f));
        sequence.Join(highestUpImage.DOFade(1, 0.3f));
    }

    public void SetHighestDownSortingState(SortigButtonsTypes buttonType)
    {
        var (button, highestDownImage, highestUpImage) = InitButtonParameters(buttonType);

        if (buttonType == SortigButtonsTypes.Raiting)
        {
            IsRatingSortUp = false;
        }
        else
        {
            IsGameTypeSortUp = false;
        }

        DOTween.Kill("HighestUp " + button.GetHashCode());
        var sequence = DOTween.Sequence();
        sequence.SetId("HighestDown " + button.GetHashCode());
        sequence.Append(button.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f));
        sequence.Join(highestDownImage.DOFade(1, 0.3f));
        sequence.Join(highestUpImage.DOFade(0, 0.3f));
    }

    private (Button button, Image highestDownImage, Image highestUpImage) InitButtonParameters(SortigButtonsTypes buttonType)
    {
        return buttonType switch
        {
            SortigButtonsTypes.None => throw new System.Exception("type None in InitButtonParameters"),
            SortigButtonsTypes.Raiting => (RaitingSortButton, _raitingHighestDownImage, _raitingHighestUpImage),
            SortigButtonsTypes.GameType => (GameTypeSortButton, _gameTypeHighestDownImage, _gameTypeHighestUpImage),
            _ => throw new System.Exception("type Default in InitButtonParameters"),
        };
    }

    private void CloseBattleSettingsPanel()
    {
        _battleSettingsPanel.SetActive(false);
    }
    
    public void SetRating(string rating)
    {
        _ratingCounterText.text = rating;
    }

    public void SetAvatar(Sprite avatarSprite)
    {
        _avatar.sprite = avatarSprite;
    }

    public void SetUserName(string userName)
    {
        _playerNameText.text = userName;
    }

    public void UpdateRoomListView(List<RoomMiniView> roomList)
    {

        _roomButtons = roomList;

        for(int i = roomList.Count - 1; i >= 0; i--)
        {
            roomList[i].transform.SetParent(_roomListTransform);
            roomList[i].transform.SetAsFirstSibling();
        }
    }

    public void SubscribeOnNewButton(RoomMiniView roomButton)
    {
        roomButton.OnClickRoomMiniView += SelectPickedRoom;
    }

    public void UnsubscribeFromRemovedButton(RoomMiniView roomButton)
    {
        roomButton.OnClickRoomMiniView -= SelectPickedRoom;
    }

    public void UnsubscribeFromAllButtons(List<RoomMiniView> roomMiniViews)
    {
        for(int i = 0; i < roomMiniViews.Count; i++)
        {
            roomMiniViews[i].OnClickRoomMiniView -= SelectPickedRoom;
        }
    }

    public void CheckAndResetSelectedRoom()
    {
        if (_selectedRoom != null)
        {
            _selectedRoom.DeselectView();
            _selectedRoom = null;
        }

        SetJoinButtonInteractableState(false);
    }

    private void SelectPickedRoom(RoomMiniView pickedRoom)
    {
        _selectedRoom = pickedRoom;

        for (int i = 0; i < _roomButtons.Count; i++)
        {
            if (_roomButtons[i] == pickedRoom)
            {
                if (!_roomButtons[i].IsSelected)
                {
                    _roomButtons[i].SelectView();
                    SetJoinButtonInteractableState(true);
                }
                else
                {
                    _roomButtons[i].DeselectView();
                    _selectedRoom = null;
                    SetJoinButtonInteractableState(false);
                }
            }
            else
            {
                if (_roomButtons[i].IsSelected)
                {
                    _roomButtons[i].DeselectView();
                }
            }
        }
    }

    private void SetJoinButtonInteractableState(bool isInteractable)
    {
        JoinButton.interactable = isInteractable;
    }

    public void UpdateSearchResult(RoomMiniView roomMiniView)
    {
        _searchingRoomButtons.Clear();
        _searchingRoomButtons.Add(roomMiniView);

        _roomButtons = _searchingRoomButtons;
        
        roomMiniView.transform.SetParent(_roomListTransform);
        roomMiniView.transform.SetAsFirstSibling();
    }

    public void ResetSearchResult()
    {
        _searchingRoomButtons.Clear();
        CheckAndResetSelectedRoom();
    }

    public void ChangeRoomsPositions()
    {
        for (int i = _roomButtons.Count - 1; i >= 0; i--)
        {
            _roomButtons[i].transform.SetParent(_poolTransform);
            _roomButtons[i].RectTransform.anchoredPosition3D = Vector3.zero;
            _roomButtons[i].RectTransform.anchorMin = Vector2.zero;
            _roomButtons[i].RectTransform.anchorMax = Vector2.zero;
        }
    }

    public void ClearSubscribes() 
    {
        _logOutButton.onClick.RemoveAllListeners();
        _acceptPanelStayButton.onClick.RemoveAllListeners();
        _createBattleButton.onClick.RemoveAllListeners();
        _battleSettingsPanelMissTapDetector.OnMissClick -= CloseBattleSettingsPanel;
        _battleSettingsExitButton.onClick.RemoveAllListeners();
        StartClassicButton.onClick.RemoveAllListeners();
        StartRoyalButton.onClick.RemoveAllListeners();
    }
}
