using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
		Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
		Managers.Object.RemoveMyPlayer();
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
		foreach (ObjectInfo obj in spawnPacket.Objects)
        {
			Managers.Object.Add(obj, myPlayer: false);
        }
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
		foreach (int id in despawnPacket.ObjectIds)
		{
			Managers.Object.Remove(id);
		}
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;

		GameObject go = Managers.Object.FindCreature(movePacket.ObjectId);
		if (go == null)
        {
			return;
        }

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc == null)
        {
			return;
        }

		cc.PositionInfo = movePacket.PositionInfo;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;

		GameObject go = Managers.Object.FindCreature(skillPacket.ObjectId);
		if (go == null)
		{
			return;
		}

		PlayerController pc = go.GetComponent<PlayerController>();
		if (pc == null)
		{
			return;
		}

		pc.UseSkill(skillPacket.SkillInfo.SkillId);
	}
}
