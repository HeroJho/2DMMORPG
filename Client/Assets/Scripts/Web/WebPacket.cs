using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 시작하면 클라 > 서버
public class CreateAccountPacketReq
{
    public string AccountName;
    public string Password;
}

// 응답 서버 > 클라
public class CreateAccountPacketRes
{
    public bool CreateOk;
}

public class LoginAccountPacketReq
{
    public string AccountName;
    public string Password;
}

// 서버 혼잡도 정보
public class ServerInfo
{
    public string Name;
    public string IpAddress;
    public int Port;
    public int BusyScore;
}

public class LoginAccountPacketRes
{
    public bool LoginOk;
    public int AccountId;
    public int Token;
    public List<ServerInfo> ServerList = new List<ServerInfo>();
}
