using Photon.Realtime;
using System;
using System.Collections.Generic;

public class SearchPrivateGameController
{
    public Action<RoomMiniView> OnRoomMiniViewAdd;
    public Action<RoomMiniView> OnRoomMiniViewDelete;

    private MiniRoomButtonsPool _miniRoomButtonsPool;
    private List<RoomInfo> _roomInfos = new List<RoomInfo>(100);
    private bool _isSearchActive;
    private bool _isSearchComplite;
    private RoomMiniView _currentRoomMiniView;

    public bool IsSearchActive => _isSearchActive;
    public bool IsSearchComplite => _isSearchComplite;

    public SearchPrivateGameController(MiniRoomButtonsPool miniRoomButtonsPool)
    {
        _miniRoomButtonsPool = miniRoomButtonsPool;
    }

    public void UpdateRoomList(List<RoomInfo> roomInfos)
    {
        for(int i = 0; i < roomInfos.Count; i++)
        {
            var roomInfo = _roomInfos.Find(roomInfo => roomInfo.Name == roomInfos[i].Name);

            if (roomInfo == null)
            {
                _roomInfos.Add(roomInfos[i]);
            }
            else
            {
                if (_currentRoomMiniView != null)
                {
                    if (roomInfos[i].Name == _currentRoomMiniView.RoomInfo.Name)
                    {
                        _currentRoomMiniView.InitRoomMiniView(roomInfos[i], roomInfos[i].masterClientId);
                    }

                    if (_currentRoomMiniView.RoomInfo.RemovedFromList)
                    {
                        _miniRoomButtonsPool.ReturnRoomMiniView(_currentRoomMiniView);
                        OnRoomMiniViewDelete?.Invoke(_currentRoomMiniView);
                        _currentRoomMiniView = null;

                        _isSearchComplite = false;
                    }
                }

                _roomInfos.Remove(roomInfo);

                if (!roomInfos[i].RemovedFromList)
                {
                    _roomInfos.Add(roomInfos[i]);
                }
            }
        }
    }

    public bool TryFindPrivateRoom(string roomName, out RoomMiniView roomMiniView)
    {
        var isRoomFound = false;
        roomMiniView = null;

        for(int i = 0; i < _roomInfos.Count; i++)
        {
            if (_roomInfos[i].IsOpen && _roomInfos[i].Name == roomName && !_roomInfos[i].RemovedFromList)
            {
                roomMiniView = _miniRoomButtonsPool.GetRoomMiniView();

                roomMiniView.InitRoomMiniView(_roomInfos[i], _roomInfos[i].masterClientId);
                isRoomFound = true;
                _isSearchComplite = true;
                _currentRoomMiniView = roomMiniView;
                OnRoomMiniViewAdd?.Invoke(roomMiniView);
                break;
            }
        }

        return isRoomFound;
    }

    public void SetSearchActive()
    {
        _isSearchActive = true;
    }

    public void ResetSearch()
    {
        if(_isSearchComplite)
        {
            _miniRoomButtonsPool.ReturnRoomMiniView(_currentRoomMiniView);
            _currentRoomMiniView = null;
        }

        _isSearchActive = false;
        _isSearchComplite = false;
    }
}