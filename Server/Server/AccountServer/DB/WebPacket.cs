using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 시작하면 클라 > 서버
public class CreateAccountPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

// 응답 서버 > 클라
public class CreateAccountPacketRes
{
    public bool CreateOk { get; set; }
}

public class LoginAccountPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

// 서버 혼잡도 정보
public class ServerInfo
{
    public string Name { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int BusyScore { get; set; }
}

public class LoginAccountPacketRes
{
    public bool LoginOk { get; set; }
    public int LoginFalse { get; set; }
    public string AccountId { get; set; }
    public int Token { get; set; }
    public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
}

public class LogoutAccountPacketReq 
{
    public string AccountName { get; set; }
}

public class LogoutAccountPacketRes
{
    
}