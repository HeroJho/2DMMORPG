using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountServer;
using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedDB;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    AppDbContext _context;
    SharedDbContext _shared;

    public AccountController(AppDbContext context, SharedDbContext shared)
    {
        _context = context;
        _shared = shared;
    }

    [HttpPost]
    [Route("create")]
    public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
    {
        CreateAccountPacketRes res = new CreateAccountPacketRes();

        AccountDb account = _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.AccountName == req.AccountName)
                                    .FirstOrDefault();

        if(account == null)
        {
            _context.Accounts.Add(new AccountDb()
            {
                AccountName = req.AccountName,
                password = req.Password
            });

            // 찰나의 순간 닉네임 중복
            bool success = _context.SaveChangeEx();
            res.CreateOk = true;
        }
        else
        {// TODO : false => 닉네임 중복
            res.CreateOk = false;
        }

        // return 하면 알아서 json으로 변환해서 클라에 전송
        return res;
    }

    [HttpPost]
    [Route("login")]
    public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
    {
        LoginAccountPacketRes res = new LoginAccountPacketRes();

        AccountDb account = _context.Accounts
            .AsNoTracking()
            .Where(a => a.AccountName == req.AccountName && a.password == req.Password)
            .FirstOrDefault();

        if(account == null)
        {
            res.LoginOk = false;
        }
        else
        {
            res.LoginOk = true;

            // 토큰 발급
            DateTime expired = DateTime.UtcNow;
            expired.AddSeconds(600); // 600초 후에 만료

            TokenDb tokenDb = _shared.Tokens.Where(t => t.AccountDbId == account.AccountDbId).FirstOrDefault();
            if (tokenDb != null) // 토큰이 이미 있다 > 갱신
            {
                tokenDb.Token = new Random().Next(Int32.MinValue, Int32.MaxValue);
                tokenDb.Expired = expired;
                _shared.SaveChangeEx();
            }
            else // 토큰이 없다 > 처음 로그인 했다
            {
                tokenDb = new TokenDb()
                {
                    AccountDbId = account.AccountDbId,
                    Token = new Random().Next(Int32.MinValue, Int32.MaxValue),
                    Expired = expired
                };
                _shared.Add(tokenDb);
                _shared.SaveChangeEx();
            }

            res.AccountId = account.AccountDbId;
            res.Token = tokenDb.Token;
            res.ServerList = new List<ServerInfo>();

            // 서버 정보를 보낸다
            foreach(ServerDb serverDb in _shared.Servers)
            {
                res.ServerList.Add(new ServerInfo()
                {
                    Name = serverDb.Name,
                    IpAddress = serverDb.IpAddress,
                    Port = serverDb.Port,
                    BusyScore = serverDb.BusyScore
                });
            }
        }

        return res;
    }



}



