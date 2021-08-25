using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server.DB
{
    [Table("Account")]
    public class AccountDb
    {
        // Convention 방법 PK설정 (클래스 이름 + Id)
        public int AccountDbId { get; set; }
        public string AccountName { get; set; }
        // 한 계정에 여러개의 캐릭터 1 : M 관계
        public ICollection<PlayerDb> Players { get; set; }
    }

    [Table("Player")]
    public class PlayerDb
    {
        public int PlayerDbId { get; set; }
        public string PlayerName { get; set; }

        [ForeignKey("Account")]
        public int AccountDbId { get; set; }
        public AccountDb Account { get; set; }

        public int Level { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHp { get; set; }
        public int MaxMp { get; set; }
        public int Attack { get; set; }
        public float Speed { get; set; }
        public int TotalExp { get; set; }

        public ICollection<ItemDb> Items { get; set; }
    }

    [Table("Item")]
    public class ItemDb
    {
        public int ItemDbId { get; set; } // PK
        public int TemplateId { get; set; } // ItemId
        public int Count { get; set; } // 소유 갯수
        public int Slot { get; set; } // 아이템 배치 위치
        public bool Equipped { get; set; } = false;
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int RoomId { get; set; } // 어떤 방에 떨어져 있냐

        [ForeignKey("Owner")]
        public int? OwnerDbId { get; set; } // 바닥템 고려 null 허용
        public PlayerDb Owner { get; set; }
    }

}
