using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
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

		if (clientSession.MyPlayer == null)
        {
			return;
        }

		if (clientSession.MyPlayer.Room == null)
        {
			return;
        }

		// TODO: 검증

		// Server 측에서 좌표 이동.
		PlayerInfo info = clientSession.MyPlayer.Info;
		info.PositionInfo = movePacket.PositionInfo;

		// 타 Player에게 전송.
		S_Move responseMovePacket = new S_Move();
		responseMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		responseMovePacket.PositionInfo = movePacket.PositionInfo;

		clientSession.MyPlayer.Room.Broadcast(responseMovePacket);
	}
}
