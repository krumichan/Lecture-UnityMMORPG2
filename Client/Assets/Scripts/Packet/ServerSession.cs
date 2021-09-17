using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerSession : PacketSession
{
	public override void OnConnected(EndPoint endPoint)
	{
		Debug.Log($"OnConnected : {endPoint}");
	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		Debug.Log($"OnDisconnected : {endPoint}");
	}

	public override void OnRecvPacket(ArraySegment<byte> buffer)
	{
		PacketManager.Instance.OnRecvPacket(this, buffer);
	}

	public override void OnSend(int numOfBytes)
	{
		//Console.WriteLine($"Transferred bytes: {numOfBytes}");
	}
}