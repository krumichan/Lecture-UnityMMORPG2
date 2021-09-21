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
		ServerSession serverSession = session as ServerSession;

		Debug.Log("S_EnterGameHandler");
		Debug.Log(enterGamePacket.Player);
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
		ServerSession serverSession = session as ServerSession;

		Debug.Log("S_LeaveGameHandler");
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
		ServerSession serverSession = session as ServerSession;

		Debug.Log("S_SpawnHandler");
		Debug.Log(spawnPacket.Players);
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
		ServerSession serverSession = session as ServerSession;

		Debug.Log("S_DespawnHandler");
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;
		ServerSession serverSession = session as ServerSession;

		Debug.Log("S_MoveHandler");
	}
}
