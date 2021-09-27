using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        
        switch (objectType)
        {
            case GameObjectType.Player:
                if (myPlayer)
                {
                    GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
                    go.name = info.Name;
                    _objects.Add(info.ObjectId, go);

                    MyPlayer = go.GetComponent<MyPlayerController>();
                    MyPlayer.Id = info.ObjectId;
                    MyPlayer.PositionInfo = info.PositionInfo;
                    MyPlayer.SyncPosition();
                }
                else
                {
                    GameObject go = Managers.Resource.Instantiate("Creature/Player");
                    go.name = info.Name;
                    _objects.Add(info.ObjectId, go);

                    PlayerController pc = go.GetComponent<PlayerController>();
                    pc.Id = info.ObjectId;
                    pc.PositionInfo = info.PositionInfo;
                    pc.SyncPosition();
                }
                break;

            case GameObjectType.Monster:
                break;

            case GameObjectType.Projectile:
                {
                    GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
                    go.name = "Arrow";
                    _objects.Add(info.ObjectId, go);

                    ArrowController ac = go.GetComponent<ArrowController>();
                    ac.Dir = info.PositionInfo.MoveDir;
                    ac.CellPosition = new Vector3Int(info.PositionInfo.PosX, info.PositionInfo.PosY, 0);
                    ac.SyncPosition();
                }
                break;
        }
    }

    public void Remove(int id)
    {
        GameObject go = FindCreature(id);
        if (go == null)
        {
            return;
        }

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public void RemoveMyPlayer()
    {
        if (MyPlayer == null)
        {
            return;
        }

        Remove(MyPlayer.Id);
        MyPlayer = null;
    }

    public GameObject FindCreature(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject FindCreature(Vector3Int cellPosition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            CreatureController controller = obj.GetComponent<CreatureController>();
            if (controller == null)
            {
                continue;
            }

            if (controller.CellPosition == cellPosition)
            {
                return obj;
            }
        }

        return null;
    }

    public GameObject FindCreature(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
            {
                return obj;
            }
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
        {
            Managers.Resource.Destroy(obj);
        }

        _objects.Clear();
    }
}
