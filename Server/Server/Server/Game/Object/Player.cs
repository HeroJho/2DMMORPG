using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using System;

namespace Server
{
    public class Player : CreatureObject
    {
        public int PlayerDbId { get; set; }
		public ClientSession Session { get; set; }
        public VisionCube Vision { get; private set; }
        public bool CanUseSkill { get; private set; } = true;

        public Inventory Inven { get; private set; } = new Inventory();

        private int Level
        {
            get { return Stat.Level; }
            set { Stat.Level = value; }
        }

        private int LevelUpExp
        {
            get { return Level * 25; }
        }

        public int Mp
        {
            get { return Stat.Mp; }
            set { Stat.Mp = Math.Clamp(value, 0, Stat.MaxMp); }
        }

        public int Ex 
        {
            get { return Stat.TotalExp; }
            set { Stat.TotalExp = value; }
        }

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);

        }

        public void UseMp(int mp)
        {
            if (Room == null)
                return;

            Mp = Math.Max(Mp - mp, 0);

            if (Mp < 0)
                Mp = 0;

            S_ChangeMp changeHpPacket = new S_ChangeMp();
            changeHpPacket.ObjectId = Id;
            changeHpPacket.Mp = Mp;
            Room.Broadcast(CellPos, changeHpPacket);
        }

        public override void OnDead(GameObject attacker)
        {
            if(Vision.job != null)
                Vision.job.Cancel = true;
            Vision.job = null;
            base.OnDead(attacker);
        }

        public void OnLeaveGame()
        {
            // DB 연동

            DbTransaction.SavePlayerStatus_AllInOne(this, Room);
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            // Item이 있는지 확인
            Item item = Inven.Get(equipPacket.ItemDbId);
            if (item == null)
                return;
               
            // 같은 아이템이고 장착에 변화가 없으면 스킵
            if (item.ItemDbId == equipPacket.ItemDbId && item.Equipped == equipPacket.Equipped)
                return;

            // 소모품은 장착 x
            if (item.ItemType == ItemType.Consumable)
                return;

            // 착용 요청이라면, 겹치는 부위 해제
            if (equipPacket.Equipped)
            {
                Item unequipItem = null;

                // 요청한 아이템과 같은 종류인데 착용중인 아이템 찾기
                if (item.ItemType == ItemType.Weapon)
                {
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Weapon);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Armor
                        && ((Armor)i).ArmorType == armorType);
                }
                // 중복 부위 아이템 존재 > 벗긴다
                if (unequipItem != null)
                {
                    // 메모리 선적용
                    unequipItem.Equipped = false;

                    // DB에 던져주기만 함
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    // 클라에 통보
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }

            {
                // 메모리 선적용
                item.Equipped = equipPacket.Equipped;

                // DB에 던져주기만 함
                DbTransaction.EquipItemNoti(this, item);

                // 클라에 통보
                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equipOkItem);
            }

            RefreshAdditionanlStat();
        }

        public void RefreshAdditionanlStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach (Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
        }

        public void GetEx(int exp)
        {
            Ex += exp;

            // LevelUp
            while (Ex >= LevelUpExp)
            {
                Ex -= LevelUpExp;
                Level++;
                if(Room != null)
                {
                    S_LevelUp levelUpPacket = new S_LevelUp();
                    levelUpPacket.Id = Id;
                    levelUpPacket.Level = Level;
                    Room.Broadcast(CellPos, levelUpPacket);
                }
            }

            S_ChangeEx changeExPacket = new S_ChangeEx();
            changeExPacket.Ex = Ex;
            changeExPacket.LevelEx = LevelUpExp;

            Session.Send(changeExPacket);
        }

        public void StartCheckCooltime(float coolTime)
        {
            CanUseSkill = false;
            Room.PushAfter((int)(coolTime * 1000), ResetCooltime);
        }

        public void ResetCooltime()
        {
            CanUseSkill = true;
            S_ManageSkill manageSkillPacket = new S_ManageSkill();
            manageSkillPacket.CanUseSkill = CanUseSkill;
            Session.Send(manageSkillPacket);
        }

        public void RecoveryMp(int mp)
        {
            if (Room == null)
                return;

            Mp += mp;

            S_ChangeMp changeHpMpPacket = new S_ChangeMp();
            changeHpMpPacket.ObjectId = Id;
            changeHpMpPacket.Mp = Mp;
            Room.Broadcast(CellPos, changeHpMpPacket);
        }
    }
}
