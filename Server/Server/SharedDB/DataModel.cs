using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedDB
{
    [Table("Token")]
    public class TokenDb
    {
        public int TokenDbId { get; set; }
        public int AccountDbId { get; set; }
        public int Token { get; set; }
        public DateTime Expired { get; set; }
    }

    [Table("ServerInfo")]
    public class ServerDb
    {
        public int ServerDbId { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int BusyScore { get; set; }

    }
}
