using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId);
        }

        public void Update()
        {
            lock (_lock)
            {
                foreach (Projectile projectile in _projectiles.Values)
                {
                    projectile.Update();
                }
            }
        }

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            lock (_lock)
            {
                switch (type)
                {
                    case GameObjectType.Player:
                        Player player = gameObject as Player;

                        _players.Add(gameObject.Id, player);
                        player.Room = this;

                        // 본인에게 전송.
                        {
                            S_EnterGame enterPacket = new S_EnterGame();
                            enterPacket.Player = player.Info;
                            player.Session.Send(enterPacket);

                            S_Spawn spawnPacket = new S_Spawn();
                            foreach (Player p in _players.Values)
                            {
                                if (player != p)
                                {
                                    spawnPacket.Objects.Add(p.Info);
                                }
                            }
                            player.Session.Send(spawnPacket);
                        }
                        break;

                    case GameObjectType.Monster:
                        Monster monster = gameObject as Monster;
                        _monsters.Add(gameObject.Id, monster);
                        monster.Room = this;
                        break;

                    case GameObjectType.Projectile:
                        Projectile projectile = gameObject as Projectile;
                        _projectiles.Add(gameObject.Id, projectile);
                        projectile.Room = this; 
                        break;
                }

                // 타인에게 전송.
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(gameObject.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                        {
                            p.Session.Send(spawnPacket);
                        }
                    }
                }
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            lock (_lock)
            {
                switch (type)
                {
                    case GameObjectType.Player:
                        Player player = null;
                        if (_players.Remove(objectId, out player) == false)
                        {
                            return;
                        }

                        player.Room = null;
                        Map.ApplyLeave(player);

                        // 본인에게 전송.
                        {
                            S_LeaveGame leavePacket = new S_LeaveGame();
                            player.Session.Send(leavePacket);
                        }
                        break;

                    case GameObjectType.Monster:
                        Monster monster = null;
                        if (_monsters.Remove(objectId, out monster) == false)
                        {
                            return;
                        }

                        monster.Room = null;
                        Map.ApplyLeave(monster);
                        break;

                    case GameObjectType.Projectile:
                        Projectile projectile = null;
                        if (_projectiles.Remove(objectId, out projectile) == false)
                        {
                            return;
                        }

                        projectile.Room = null;
                        break;
                }


                // 타인에게 전송.
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.ObjectIds.Add(objectId);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != objectId)
                        {
                            p.Session.Send(despawnPacket);
                        }
                    }
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
            {
                return;
            }

            lock (_lock)
            {
                // TODO: 검증
                PositionInfo movePositionInfo = movePacket.PositionInfo;
                ObjectInfo info = player.Info;

                // 다른 좌표로 이동할 경우, 갈 수 있는지 검증.
                if (movePositionInfo.PosX != info.PositionInfo.PosX || movePositionInfo.PosY != info.PositionInfo.PosY)
                {
                    if (Map.CanMove(new Vector2Int(movePositionInfo.PosX, movePositionInfo.PosY)) == false)
                    {
                        return;
                    }
                }

                info.PositionInfo.State = movePositionInfo.State;
                info.PositionInfo.MoveDir = movePositionInfo.MoveDir;
                Map.ApplyMove(player, new Vector2Int(movePositionInfo.PosX, movePositionInfo.PosY));

                // 타 Player에게 전송.
                S_Move responseMovePacket = new S_Move();
                responseMovePacket.ObjectId = player.Info.ObjectId;
                responseMovePacket.PositionInfo = movePacket.PositionInfo;

                Broadcast(responseMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
            {
                return;
            }

            lock (_lock)
            {
                ObjectInfo info = player.Info;
                if (info.PositionInfo.State != CreatureState.Idle)
                {
                    return;
                }

                // TODO: Skill 사용 가능 여부 체크.

                info.PositionInfo.State = CreatureState.Skill;

                S_Skill skill = new S_Skill() { SkillInfo = new SkillInfo() };
                skill.ObjectId = info.ObjectId;
                skill.SkillInfo.SkillId = skillPacket.SkillInfo.SkillId;

                Broadcast(skill);

                if (skillPacket.SkillInfo.SkillId == 1)
                {
                    // TODO: Damage 판정.
                    Vector2Int skillPosition = player.GetFrontCellPosition(info.PositionInfo.MoveDir);
                    GameObject target = Map.Find(skillPosition);
                    if (target != null)
                    {
                        Console.WriteLine("Hit GameObject !!");
                    }
                }
                else if (skillPacket.SkillInfo.SkillId == 2)
                {
                    // TODO: Arrow.
                    Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                    if (arrow == null)
                    {
                        return;
                    }

                    arrow.Owner = player;
                    arrow.PosInfo.State = CreatureState.Moving;
                    arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                    arrow.PosInfo.PosX = player.PosInfo.PosX;
                    arrow.PosInfo.PosY = player.PosInfo.PosY;
                    
                    EnterGame(arrow);
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}
