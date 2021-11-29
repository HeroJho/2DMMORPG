using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AccountServer.DB
{
    [Table("Account")]
    public class AccountDb
    {
        // Convention 방법 PK설정 (클래스 이름 + Id)
        public int AccountDbId { get; set; }
        public string AccountName { get; set; }
        public string password { get; set; }
    }
}
