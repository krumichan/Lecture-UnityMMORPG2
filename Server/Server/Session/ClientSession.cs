using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;

namespace Server
{
	public class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		public void Send(IMessage packet)
        {
			// Class Name 추출.
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
			MsgId msgId = (MsgId) Enum.Parse(typeof(MsgId), msgName);

			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 4];

			// size
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));

			// id
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));

			// using Google.Protobuf; 필요.
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

			Send(new ArraySegment<byte>(sendBuffer));
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			// PROTO
			MyPlayer = PlayerManager.Instance.Add();
            {
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.PlayerId}";
				MyPlayer.Info.PositionInfo.State = CreatureState.Idle;
				MyPlayer.Info.PositionInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PositionInfo.PosX = 0;
				MyPlayer.Info.PositionInfo.PosY = 0;
				MyPlayer.Session = this;
			}

			RoomManager.Instance.Find(1).EnterGame(MyPlayer);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.PlayerId);

			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
