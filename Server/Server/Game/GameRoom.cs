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

        Map _map = new Map();

        public void Init(int mapId)
        {
            _map.LoadMap(mapId);
        }

        public void EnterGame(Player newPlayer)
        {
            if (newPlayer == null)
            {
                return;
            }

            lock (_lock)
            {
                _players.Add(newPlayer.Info.PlayerId, newPlayer);
                newPlayer.Room = this;

                // 본인에게 전송.
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (newPlayer != p)
                        {
                            spawnPacket.Players.Add(p.Info);
                        }
                    }
                    newPlayer.Session.Send(spawnPacket);
                }

                // 타인에게 전송.
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (newPlayer != p)
                        {
                            p.Session.Send(spawnPacket);
                        }
                    }
                }
            }
        }

        public void LeaveGame(int playerId)
        {
            lock (_lock)
            {
                Player player = null;
                if (_players.Remove(playerId, out player))
                {
                    return;
                }

                player.Room = null;

                // 본인에게 전송.
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                // 타인에게 전송.
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.Info.PlayerId);
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
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
                PlayerInfo info = player.Info;

                // 다른 좌표로 이동할 경우, 갈 수 있는지 검증.
                if (movePositionInfo.PosX != info.PositionInfo.PosX || movePositionInfo.PosY != info.PositionInfo.PosY)
                {
                    if (_map.CanMove(new Vector2Int(movePositionInfo.PosX, movePositionInfo.PosY)) == false)
                    {
                        return;
                    }
                }

                info.PositionInfo.State = movePositionInfo.State;
                info.PositionInfo.MoveDir = movePositionInfo.MoveDir;
                _map.ApplyMove(player, new Vector2Int(movePositionInfo.PosX, movePositionInfo.PosY));

                // 타 Player에게 전송.
                S_Move responseMovePacket = new S_Move();
                responseMovePacket.PlayerId = player.Info.PlayerId;
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
                PlayerInfo info = player.Info;
                if (info.PositionInfo.State != CreatureState.Idle)
                {
                    return;
                }

                // TODO: Skill 사용 가능 여부 체크.

                // 통과.
                info.PositionInfo.State = CreatureState.Skill;

                S_Skill skill = new S_Skill() { SkillInfo = new SkillInfo() };
                skill.PlayerId = info.PlayerId;
                skill.SkillInfo.SkillId = 1;

                Broadcast(skill);

                // TODO: Damage 판정.
                Vector2Int skillPosition = player.GetFrontCellPosition(info.PositionInfo.MoveDir);
                Player target = _map.Find(skillPosition);
                if (target != null)
                {
                    Console.WriteLine("Hit Player !!");
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
