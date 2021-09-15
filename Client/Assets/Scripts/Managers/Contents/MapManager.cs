using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Position
{
    public Position(int y, int x) { Y = y; X = x; }
    public int Y;
    public int X;
}

public struct PQNode : IComparable<PQNode>
{
    public int F;
    public int G;
    public int Y;
    public int X;

    public int CompareTo(PQNode other)
    {
        if (F == other.F)
        {
            return 0;
        }

        return F < other.F ? 1 : -1;
    }
}

public class MapManager
{
    public Grid CurrentGrid { get; private set; }

    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }

    bool[,] _collision;

    public int SizeX { get { return MaxX - MinX + 1; } }
    public int SizeY { get { return MaxY - MinY + 1; } }

    public bool CanMove(Vector3Int cellPosition)
    {
        if (cellPosition.x < MinX || cellPosition.x > MaxX)
        {
            return false;
        }
        if (cellPosition.y < MinY || cellPosition.y > MaxY)
        {
            return false;
        }

        // _collision array������ ��ǥ ����.
        int x = cellPosition.x - MinX;
        int y = MaxY - cellPosition.y;

        return !_collision[y, x];
    }

    public void LoadMap(int mapId)
    {
        DestroyMap();

        string mapName = "Map_" + mapId.ToString("000");
        GameObject go = Managers.Resource.Instantiate($"Map/{mapName}");
        go.name = "Map";

        GameObject collision = Util.FindChild(go, "Tilemap_Collision", true);
        if (collision != null)
        {
            collision.SetActive(false);
        }

        CurrentGrid = go.GetComponent<Grid>();

        // Collision ���� ����.
        TextAsset mapTxt = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
        StringReader reader = new StringReader(mapTxt.text);

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new bool[yCount, xCount];

        for (int y = 0; y < yCount; ++y)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; ++x)
            {
                _collision[y, x] = (line[x] == '1' ? true : false);
            }
        }
    }

    public void DestroyMap()
    {
        GameObject map = GameObject.Find("Map");
        if (map != null)
        {
            GameObject.Destroy(map);
            CurrentGrid = null;
        }
    }

    #region A* PathFinding
    int[] _deltaY = new int[] { 1, -1, 0, 0 };  // Up, Down
    int[] _deltaX = new int[] { 0, 0, -1, 1 };  // Left, Right
    int[] _cost = new int[] { 10, 10, 10, 10 };

    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int destination, bool ignoreDestinationCollision = false)
    {
        List<Position> path = new List<Position>();

        // ���� �ű��
        // F = G + H
        // F = ���� ����( �������� ����, ��ο� ���� �޶��� )
        // G = ���������� �ش� ��ǥ���� �̵��ϴµ� ��� ���( �������� ����, ��ο� ���� �޶��� )
        // H = ���������� �󸶳� ����( �������� ����, ���� )

        // (y, x) �̹� �湮�ߴ��� ����( �湮 = closed ���� )
        bool[,] closed = new bool[SizeY, SizeX]; // Close List

        // (y, x) ���� ���� �� ���̶� �߰� �Ͽ��°�.
        // �߰�X �� MaxValue
        // �߰�O �� F = G + H
        int[,] open = new int[SizeY, SizeX]; // Open List
        for (int y = 0; y < SizeY; ++y)
        {
            for (int x = 0; x < SizeX; ++x)
            {
                open[y, x] = Int32.MaxValue;
            }
        }

        Position[,] parent = new Position[SizeY, SizeX];

        // Open List�� �ִ� ������ �߿���, ���� ���� �ĺ��� ������ �̾ƿ��� ���� ����
        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

        // Cell Position �� Array Position
        Position pos = Cell2Position(start);
        Position dest = Cell2Position(destination);

        // ������ �߰�( ���� ���� )
        open[pos.Y, pos.X] = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X));
        pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
        parent[pos.Y, pos.X] = new Position(pos.Y, pos.X);

        while (pq.Count > 0)
        {
            // ���� ���� �ĺ� Ž��
            PQNode node = pq.Pop();

            // ������ ��ǥ�� ���� ��η� ã�Ƽ�,
            // �� ���� ��η� ���Ͽ� �̹� �湮( closed )�� ������ ��� ����.
            if (closed[node.Y, node.X])
            {
                continue;
            }

            // �湮
            closed[node.Y, node.X] = true;

            // ������ ���� ��, ����.
            if (node.Y == dest.Y && node.X == dest.X)
            {
                break;
            }

            // �����¿� �� �̵��� �� �ִ� ��ǥ���� Ȯ���Ͽ� ����( open ) ����.
            for (int i = 0; i < _deltaY.Length; ++i)
            {
                Position next = new Position(node.Y + _deltaY[i], node.X + _deltaX[i]);

                // ��ȿ ������ ������ ��� ����.
                // ������ ������ �� �� ���� ��쵵 ����.
                if (!ignoreDestinationCollision || next.Y != dest.Y || next.X != dest.X)
                {
                    if (CanMove(Position2Cell(next)) == false) // Cell Position
                    {
                        continue;
                    }
                }

                // �̹� �湮�Ͽ��� ��� ����.
                if (closed[next.Y, next.X])
                {
                    continue;
                }

                // ��� ���.
                int g = 0; // node.G + _cost[i]
                int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                int f = g + h;

                // �ٸ� ��ο� �� ���� ���� ������ ����.
                if (open[next.Y, next.X] < f)
                {
                    continue;
                }

                // ���� ����.
                open[dest.Y, dest.X] = f;
                pq.Push(new PQNode() { F = f, G = g, Y = next.Y, X = next.X });
                parent[next.Y, next.X] = new Position(node.Y, node.X);
            }
        }

        return CalcCellPathFromParent(parent, dest);
    }

    List<Vector3Int> CalcCellPathFromParent(Position[,] parent, Position destination)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        int y = destination.Y;
        int x = destination.X;
        while (parent[y, x].Y != y || parent[y, x].X != x)
        {
            cells.Add(Position2Cell(new Position(y, x)));
            Position position = parent[y, x];
            y = position.Y;
            x = position.X;
        }

        cells.Add(Position2Cell(new Position(y, x)));
        cells.Reverse();

        return cells;
    }

    Position Cell2Position(Vector3Int cell)
    {
        // Cell Position �� Array Position
        return new Position(MaxY - cell.y, cell.x - MinX);
    }

    Vector3Int Position2Cell(Position pos)
    {
        // Array Position �� Cell Position
        return new Vector3Int(pos.X + MinX, MaxY - pos.Y, 0);
    }
    #endregion
}