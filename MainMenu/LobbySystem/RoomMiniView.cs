using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomMiniView : MonoBehaviour, IPointerClickHandler
{
    public event Action<RoomMiniView> OnClickRoomMiniView;

    [SerializeField] private TMP_Text _roomOwnerText;
    [SerializeField] private TMP_Text _gameTypeText;
    [SerializeField] private TMP_Text _raitingCountText;
    [SerializeField] private Image _backGroundImage;
    [SerializeField] private float _selectedAlpha;

    [field: SerializeField] public RectTransform RectTransform { get; private set; }
    public bool IsSelected { get; private set; }
    public int RoomOwnerID { get; private set; }
    public string RoomName { get; private set; }
    public RoomInfo RoomInfo { get; private set; }
    public int OwnerRating { get; private set; }
    public int GameTypeID { get; private set; }

    private float _idleAlpha;

    public void InitRoomMiniView(RoomInfo roomInfo, int ownerID)
    {

        RoomOwnerID = ownerID;
        RoomInfo = roomInfo;
        RoomName = roomInfo.Name;
        if (roomInfo.CustomProperties.TryGetValue(PhotonConstants.OWNER, out object name))
        {
            _roomOwnerText.text = $"Owner: {name}";
        }

        if (roomInfo.CustomProperties.TryGetValue(PhotonConstants.GAME_TYPE, out object gameType))
        {
            _gameTypeText.text = $"{gameType}";
            GameTypeID = gameType.ToString().Equals(PhotonConstants.CLASSIC_GAME) ? 0 : 1;
        }

        if (roomInfo.CustomProperties.TryGetValue(PhotonConstants.OWNER_RATING, out object ownerRating))
        {
            _raitingCountText.text = $"{ownerRating}";
            OwnerRating = int.Parse(ownerRating.ToString());
        }

        _idleAlpha = _backGroundImage.color.a;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickRoomMiniView?.Invoke(this);
        Debug.Log("OnPointerClick");
    }

    public void SelectView()
    {
        IsSelected = true;
        var color = _backGroundImage.color;
        color.a = _selectedAlpha;
        _backGroundImage.color = color;
    }

    public void DeselectView()
    {
        IsSelected = false;
        var color = _backGroundImage.color;
        color.a = _idleAlpha;
        _backGroundImage.color = color;
    }

    public void ClearRoomMiniView()
    {
        RoomOwnerID = default;
        RoomInfo = null;
        _roomOwnerText.text = default;
        _gameTypeText.text = default;
        GameTypeID = default;
        _raitingCountText.text = default;
        OwnerRating = default;
        _idleAlpha = default;
    }
}
