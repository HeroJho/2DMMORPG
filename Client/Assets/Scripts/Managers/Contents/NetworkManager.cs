using Google.Protobuf;
using Server;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager
{
    public string AccountId { get; set; }
    public int Token { get; set; }

    ServerSession _session = new ServerSession();

    public void Send(IMessage packet)
    {
        _session.Send(packet);
    }

    public void ConnectToGame(ServerInfo info)
    {
        // DNS (Domain Name System)
        IPAddress ipAddr = IPAddress.Parse(info.IpAddress);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, info.Port);

        Connector connector = new Connector();

        connector.Connect(endPoint,
            () => { return _session; });
    }

    public void Update()
    {
        List<PacketMessage> list = PacketQueue.Instance.PopAll();
        foreach(PacketMessage packet in list)
        {
            Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_session, packet.Message);
        }
    }

}
