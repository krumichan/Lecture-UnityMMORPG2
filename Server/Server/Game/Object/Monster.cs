using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    class Monster : GameObject
    {
        public Monster()
        {
            ObjectType = GameObjectType.Monster;
        }
    }
}
