using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MiniRoomViewsSortingController
{
    public Action<RoomMiniView> OnRoomMiniViewAdd;
    public Action<RoomMiniView> OnRoomMiniViewDelete;

    private List<RoomMiniView> _roomButtons = new List<RoomMiniView>(100);
    private List<RoomMiniView> _roomButtonsByRating = new List<RoomMiniView>(100);
    private List<RoomMiniView> _roomButtonsByType = new List<RoomMiniView>(100);
    private MiniRoomButtonsPool _pool;

    private int[] _occurrences = new int[10];
    private List<RoomMiniView> _byRatingTMP = new List<RoomMiniView>(100);
    private List<RoomMiniView> _byRoyalTypeTMP = new List<RoomMiniView>(50);

    public List<RoomMiniView> RoomButtonsByRating => _roomButtonsByRating;
    public List<RoomMiniView> RoomButtonsByType => _roomButtonsByType;

    public MiniRoomViewsSortingController(MiniRoomButtonsPool pool)
    {
        _pool = pool;
    }

    public void UpdateRoomsLists(List<RoomInfo> roomList)
    {
        var isRecalculateNeeded = false;

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomMiniView roomButton;

            bool isPrivate = false;

            if (roomList[i].CustomProperties.TryGetValue(PhotonConstants.PRIVATE_STATUS, out object isPrivateStatus))
            {
                isPrivate = isPrivateStatus.Equals(PhotonConstants.FREE) ? false : true;
            }

            roomButton = _roomButtons.Find(room => room.RoomName == roomList[i].Name);

            if (roomButton == null)
            {

                if (roomList[i].IsOpen && roomList[i].IsVisible && !isPrivate && !roomList[i].RemovedFromList)
                {
                    AddButton(roomList[i]);
                    isRecalculateNeeded = true;
                }
            }
            else
            {
                if (isPrivate)
                {
                    RemoveRoomButton(roomButton);
                    isRecalculateNeeded = true;
                }

                if (roomList[i].IsOpen && roomList[i].IsVisible)
                {
                    RenewRoomButton(roomList[i], roomButton);
                }

                if (roomList[i].RemovedFromList)
                {
                    RemoveRoomButton(roomButton);
                    isRecalculateNeeded = true;
                }
            }
        }

        if (isRecalculateNeeded)
        {
            RecalculateLists();
        }
    }

    public List<RoomMiniView> GetAllRooms()
    {
        return _roomButtons;
    }

    private void RenewRoomButton(RoomInfo roomInfo, RoomMiniView roomButton)
    {
        roomButton.InitRoomMiniView(roomInfo, roomInfo.masterClientId);
    }

    private void RecalculateLists()
    {
        _roomButtonsByRating.Clear();
        _roomButtonsByType.Clear();

        if(_roomButtons.Count > 0)
        {
            LSDRatingSort();
            GameTypeSort();
        }
    }

    private void GameTypeSort()
    {
        _roomButtonsByType.Clear();

        for (int i = 0; i < _roomButtons.Count; i++)
        {
            if (_roomButtons[i].GameTypeID == 0)
            {
                _roomButtonsByType.Add(_roomButtons[i]);
            }
            else
            {
                _byRoyalTypeTMP.Add(_roomButtons[i]);
            }
        }

        _roomButtonsByType.AddRange(_byRoyalTypeTMP);
        _byRoyalTypeTMP.Clear();
    }

    //LSD Methods

    private void LSDRatingSort()
    {
        _roomButtonsByRating.Clear();

        int maxRating = GetMaxRating(_roomButtons);

        for (int i = 0; i < _roomButtons.Count; i++)
        {
            _roomButtonsByRating.Add(_roomButtons[i]);
        }

        for (int exponent = 1; maxRating / exponent > 0; exponent *= 10)
            SortList(_roomButtonsByRating, exponent);

        _byRatingTMP.Clear();
    }

    private int GetMaxRating(List<RoomMiniView> roomButtons)
    {
        int maxRating = roomButtons[0].OwnerRating;

        for (int i = 0; i < roomButtons.Count; i++)
        {
            if (roomButtons[i].OwnerRating > maxRating)
            {
                maxRating = roomButtons[i].OwnerRating;
            }
        }
        return maxRating;
    }

    private void SortList(List<RoomMiniView> roomList, int exponent)
    {
        _byRatingTMP.Clear();

        var listLenth = roomList.Count;
        int i;

        for (i = 0; i < listLenth; i++)
        {
            _byRatingTMP.Add(roomList[i]);
        }

        CountOccurrencesQueue(roomList, exponent);

        for (i = listLenth - 1; i >= 0; i--)
        {
            var index = _occurrences[(roomList[i].OwnerRating / exponent) % 10] - 1;
            _byRatingTMP[index] = roomList[i];
            _occurrences[(roomList[i].OwnerRating / exponent) % 10]--;
        }

        for(i = 0; i < listLenth; i++)
        {
            _roomButtonsByRating[i] = _byRatingTMP[i];
        }
    }

    private void CountOccurrencesQueue(List<RoomMiniView> roomList, int exponent)
    {
        int i;
        var listLenth = roomList.Count;

        for (i = 0; i < 10; i++)
        {
            _occurrences[i] = 0;
        }

        for (i = 0; i < listLenth; i++)
        {
            _occurrences[(roomList[i].OwnerRating / exponent) % 10]++;
        }

        for (i = 1; i < 10; i++)
        {
            _occurrences[i] += _occurrences[i - 1];
        }
    }

    private void AddButton(RoomInfo roomInfo)
    {
        var roomButton = _pool.GetRoomMiniView();
        roomButton.InitRoomMiniView(roomInfo, roomInfo.masterClientId);
        _roomButtons.Add(roomButton);
        OnRoomMiniViewAdd?.Invoke(roomButton);
    }



    private void RemoveRoomButton(RoomMiniView roomButton)
    {
        OnRoomMiniViewDelete?.Invoke(roomButton);
        _roomButtons.Remove(roomButton);
        _pool.ReturnRoomMiniView(roomButton);
    }
}
