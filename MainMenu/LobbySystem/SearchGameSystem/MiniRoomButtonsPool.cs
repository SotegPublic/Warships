using System.Collections.Generic;
using UnityEngine;

public class MiniRoomButtonsPool
{
    private Transform _poolHolderTransform;
    private MiniViewRoomFactory _factory;
    private List<RoomMiniView> _roomMiniViews = new List<RoomMiniView>(100);

    public MiniRoomButtonsPool(GameObject poolHolder, GameObject miniRoomPrefab)
    {
        _poolHolderTransform = poolHolder.transform;
        _factory = new MiniViewRoomFactory(poolHolder, miniRoomPrefab);

        InitPool();
    }

    private void InitPool()
    {
        for (int i = 0; i < (_roomMiniViews.Capacity/10); i++)
        {
            var button = _factory.CreateRoomMiniView();
            _roomMiniViews.Add(button);
        }
    }

    public RoomMiniView GetRoomMiniView() 
    {
        if (_roomMiniViews.Count == 0)
        {
            var button = _factory.CreateRoomMiniView();
            return button;
        }
        else
        {
            var button = _roomMiniViews[0];
            _roomMiniViews.Remove(button);

            return button; 
        }
    }

    public void ReturnRoomMiniView(RoomMiniView roomMiniView)
    {
        roomMiniView.ClearRoomMiniView();
        _roomMiniViews.Add(roomMiniView);
        roomMiniView.transform.SetParent(_poolHolderTransform);
        roomMiniView.RectTransform.anchoredPosition3D = Vector3.zero;
        roomMiniView.RectTransform.anchorMin = Vector2.zero;
        roomMiniView.RectTransform.anchorMax = Vector2.zero;
    }
}
