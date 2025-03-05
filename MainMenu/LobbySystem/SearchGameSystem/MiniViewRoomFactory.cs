using UnityEngine;

public class MiniViewRoomFactory
{
    private GameObject _poolHolder;
    private GameObject _miniRoomPrefab;

    public MiniViewRoomFactory(GameObject poolHolder, GameObject miniRoomPrefab)
    {
        _poolHolder = poolHolder;
        _miniRoomPrefab = miniRoomPrefab;
    }

    public RoomMiniView CreateRoomMiniView()
    {
        var roomObject = Object.Instantiate(_miniRoomPrefab, _poolHolder.transform);
        var roomButton = roomObject.GetComponent<RoomMiniView>();

        return roomButton;
    }
}