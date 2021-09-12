using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void S_ChatHandler(PacketSession session, IPacket packet)
	{
		S_Chat chatPacket = packet as S_Chat;
		ServerSession serverSession = session as ServerSession;

		//if (chatPacket.playerId == 1)
			//Console.WriteLine(chatPacket.chat);
	}
}
