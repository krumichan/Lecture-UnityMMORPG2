using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    // Dictionary<int, GameObject> _object = new Dictionary<int, GameObject>();
    List<GameObject> _objects = new List<GameObject>();

    public void Add(GameObject go)
    {
        _objects.Add(go);
    }

    public void Remove(GameObject go)
    {
        _objects.Remove(go);
    }

    public GameObject FindCreature(Vector3Int cellPosition)
    {
        foreach (GameObject obj in _objects)
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
        foreach (GameObject obj in _objects)
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
        _objects.Clear();
    }
}
