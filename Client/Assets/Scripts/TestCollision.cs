using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestCollision : MonoBehaviour
{
    public Tilemap _tilemap;
    public TileBase _tile;

    void Start()
    {
        _tilemap.SetTile(new Vector3Int(0, 0, 0), _tile);
    }

    void Update()
    {
        List<Vector3Int> blocked = new List<Vector3Int>();
        foreach (Vector3Int position in _tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = _tilemap.GetTile(position);
            if (tile != null)
            {
                blocked.Add(position);
            }
        }
    }
}
