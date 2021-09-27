﻿using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Game
{
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

    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y) { this.x = x; this.y = y; }

        public static Vector2Int up { get { return new Vector2Int(0, 1); } }
        public static Vector2Int down { get { return new Vector2Int(0, -1); } }
        public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
        public static Vector2Int right { get { return new Vector2Int(1, 0); } }

        public static Vector2Int operator+(Vector2Int from, Vector2Int to)
        {
            return new Vector2Int(from.x + to.x, from.y + to.y);
        }
    }

    public class Map
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        bool[,] _collision;
        GameObject[,] _objects;

        public int SizeX { get { return MaxX - MinX + 1; } }
        public int SizeY { get { return MaxY - MinY + 1; } }

        public bool CanMove(Vector2Int cellPosition, bool checkObjects = true)
        {
            if (cellPosition.x < MinX || cellPosition.x > MaxX)
            {
                return false;
            }
            if (cellPosition.y < MinY || cellPosition.y > MaxY)
            {
                return false;
            }

            // _collision array에서의 좌표 추출.
            int x = cellPosition.x - MinX;
            int y = MaxY - cellPosition.y;

            return !_collision[y, x] && (!checkObjects || _objects[y, x] == null);
        }

        public GameObject Find(Vector2Int cellPosition)
        {
            if (cellPosition.x < MinX || cellPosition.x > MaxX)
            {
                return null;
            }

            if (cellPosition.y < MinY || cellPosition.y > MaxY)
            {
                return null;
            }

            int x = cellPosition.x - MinX;
            int y = MaxY - cellPosition.y;
            return _objects[y, x];
        }

        public bool ApplyLeave(GameObject gameObject)
        {
            PositionInfo positionInfo = gameObject.PosInfo;
            if (positionInfo.PosX < MinX || positionInfo.PosX > MaxX)
            {
                return false;
            }

            if (positionInfo.PosY < MinY || positionInfo.PosY > MaxY)
            {
                return false;
            }

            {
                int beforeX = positionInfo.PosX - MinX;
                int beforeY = MaxY - positionInfo.PosY;
                if (_objects[beforeY, beforeX] == gameObject)
                {
                    _objects[beforeY, beforeX] = null;
                }
            }

            return true;
        }

        public bool ApplyMove(GameObject gameObject, Vector2Int destination)
        {
            ApplyLeave(gameObject);

            PositionInfo positionInfo = gameObject.PosInfo;
            if (CanMove(destination, true) == false)
            {
                return false;
            }

            {
                int afterX = destination.x - MinX;
                int afterY = MaxY - destination.y;
                _objects[afterY, afterX] = gameObject;
            }

            // 실제 좌표 이동.
            positionInfo.PosX = destination.x;
            positionInfo.PosY = destination.y;

            return true;
        }

        public void LoadMap(int mapId, string pathPrefix = "../../../../../Common/MapData")
        {
            string mapName = "Map_" + mapId.ToString("000");

            // Collision 관련 파일.
            string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
            StringReader reader = new StringReader(text);

            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());
            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            int xCount = MaxX - MinX + 1;
            int yCount = MaxY - MinY + 1;
            _collision = new bool[yCount, xCount];
            _objects = new GameObject[yCount, xCount];

            for (int y = 0; y < yCount; ++y)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; ++x)
                {
                    _collision[y, x] = (line[x] == '1' ? true : false);
                }
            }
        }

        #region A* PathFinding
        int[] _deltaY = new int[] { 1, -1, 0, 0 };  // Up, Down
        int[] _deltaX = new int[] { 0, 0, -1, 1 };  // Left, Right
        int[] _cost = new int[] { 10, 10, 10, 10 };

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int destination, bool ignoreDestinationCollision = false)
        {
            List<Position> path = new List<Position>();

            // 점수 매기기
            // F = G + H
            // F = 최종 점수( 작을수록 좋음, 경로에 따라 달라짐 )
            // G = 시작점에서 해당 좌표까지 이동하는데 드는 비용( 작을수록 좋음, 경로에 따라 달라짐 )
            // H = 목적지에서 얼마나 가까운가( 작을수록 좋음, 고정 )

            // (y, x) 이미 방문했는지 여부( 방문 = closed 상태 )
            bool[,] closed = new bool[SizeY, SizeX]; // Close List

            // (y, x) 가는 길을 한 번이라도 발견 하였는가.
            // 발견X ⇒ MaxValue
            // 발견O ⇒ F = G + H
            int[,] open = new int[SizeY, SizeX]; // Open List
            for (int y = 0; y < SizeY; ++y)
            {
                for (int x = 0; x < SizeX; ++x)
                {
                    open[y, x] = Int32.MaxValue;
                }
            }

            Position[,] parent = new Position[SizeY, SizeX];

            // Open List에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
            PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

            // Cell Position ⇒ Array Position
            Position pos = Cell2Position(start);
            Position dest = Cell2Position(destination);

            // 시작점 발견( 예약 진행 )
            open[pos.Y, pos.X] = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X));
            pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
            parent[pos.Y, pos.X] = new Position(pos.Y, pos.X);

            while (pq.Count > 0)
            {
                // 제일 좋은 후보 탐색
                PQNode node = pq.Pop();

                // 동일한 좌표를 여러 경로로 찾아서,
                // 더 빠른 경로로 인하여 이미 방문( closed )된 상태인 경우 생략.
                if (closed[node.Y, node.X])
                {
                    continue;
                }

                // 방문
                closed[node.Y, node.X] = true;

                // 목적지 도착 시, 종료.
                if (node.Y == dest.Y && node.X == dest.X)
                {
                    break;
                }

                // 상하좌우 등 이동할 수 있는 좌표인지 확인하여 예약( open ) 수행.
                for (int i = 0; i < _deltaY.Length; ++i)
                {
                    Position next = new Position(node.Y + _deltaY[i], node.X + _deltaX[i]);

                    // 유효 범위를 지났을 경우 생략.
                    // 벽으로 막혀서 갈 수 없는 경우도 생략.
                    if (!ignoreDestinationCollision || next.Y != dest.Y || next.X != dest.X)
                    {
                        if (CanMove(Position2Cell(next)) == false) // Cell Position
                        {
                            continue;
                        }
                    }

                    // 이미 방문하였을 경우 생략.
                    if (closed[next.Y, next.X])
                    {
                        continue;
                    }

                    // 비용 계산.
                    int g = 0; // node.G + _cost[i]
                    int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                    int f = g + h;

                    // 다른 경로에 더 빠른 길이 있으면 생략.
                    if (open[next.Y, next.X] < f)
                    {
                        continue;
                    }

                    // 예약 진행.
                    open[dest.Y, dest.X] = f;
                    pq.Push(new PQNode() { F = f, G = g, Y = next.Y, X = next.X });
                    parent[next.Y, next.X] = new Position(node.Y, node.X);
                }
            }

            return CalcCellPathFromParent(parent, dest);
        }

        List<Vector2Int> CalcCellPathFromParent(Position[,] parent, Position destination)
        {
            List<Vector2Int> cells = new List<Vector2Int>();

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

        Position Cell2Position(Vector2Int cell)
        {
            // Cell Position ⇒ Array Position
            return new Position(MaxY - cell.y, cell.x - MinX);
        }

        Vector2Int Position2Cell(Position pos)
        {
            // Array Position ⇒ Cell Position
            return new Vector2Int(pos.X + MinX, MaxY - pos.Y);
        }
        #endregion
    }
}
