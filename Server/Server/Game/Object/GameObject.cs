using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }

        public GameRoom Room { get; set; }
        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();

        public GameObject()
        {
            Info.PositionInfo = PosInfo;
        }

        public Vector2Int CellPosition
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }
            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }

        public Vector2Int GetFrontCellPosition()
        {
            return GetFrontCellPosition(PosInfo.MoveDir);
        }

        public Vector2Int GetFrontCellPosition(MoveDir direction)
        {
            Vector2Int cellPosition = CellPosition;

            switch (direction)
            {
                case MoveDir.Up:
                    cellPosition += Vector2Int.up;
                    break;

                case MoveDir.Down:
                    cellPosition += Vector2Int.down;
                    break;

                case MoveDir.Left:
                    cellPosition += Vector2Int.left;
                    break;

                case MoveDir.Right:
                    cellPosition += Vector2Int.right;
                    break;
            }

            return cellPosition;
        }
    }
}
