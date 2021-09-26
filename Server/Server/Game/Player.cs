using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player
    {
        public PlayerInfo Info { get; set; } = new PlayerInfo() { PositionInfo = new PositionInfo() };
        public GameRoom Room { get; set; }
        public ClientSession Session { get; set; }

        public Vector2Int CellPosition
        {
            get
            {
                return new Vector2Int(Info.PositionInfo.PosX, Info.PositionInfo.PosY);
            }
            set
            {
                Info.PositionInfo.PosX = value.x;
                Info.PositionInfo.PosY = value.y;
            }
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
