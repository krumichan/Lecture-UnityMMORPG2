using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"C_Move ({movePacket.PositionInfo.PosX}, {movePacket.PositionInfo.PosY})");

		// MyPlayer를 null 체크 했을 때,
		// null이 아니었으나, 다음 Check 시, 다른 thread가 null로 바꾸어놔서
		// crash가 날수도 있기 때문.
		Player multiThreadSafetyPlayer = clientSession.MyPlayer;
		if (multiThreadSafetyPlayer == null)
        {
			return;
        }

        GameRoom multiThreadSafetyRoom = multiThreadSafetyPlayer.Room;
        if (multiThreadSafetyRoom == null)
        {
			return;
        }

		multiThreadSafetyRoom.HandleMove(multiThreadSafetyPlayer, movePacket);
	}

	public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
		C_Skill skillPacket = packet as C_Skill;
		ClientSession clientSession = session as ClientSession;

		Player multiThreadSafetyPlayer = clientSession.MyPlayer;
		if (multiThreadSafetyPlayer == null)
		{
			return;
		}

		GameRoom multiThreadSafetyRoom = multiThreadSafetyPlayer.Room;
		if (multiThreadSafetyRoom == null)
		{
			return;
		}

		multiThreadSafetyRoom.HandleSkill(multiThreadSafetyPlayer, skillPacket);
	}
}
