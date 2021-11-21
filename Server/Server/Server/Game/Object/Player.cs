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
        public SkillManager Skill { get; private set; }
        public QuestManager Quest { get; private set; }
        public ObstacleManager Obstacle { get; private set; }
        public Communication Communication { get; private set; }
        public JobClassType JobClassType 
        { 
            get
            {
                return Stat.JobClassType;
            }
            set
            {
                Stat.JobClassType = value;
            }
        }

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
            set { Stat.Mp = Math.Clamp(value, 0, TotalMaxMp); }
        }

        public int Ex 
        {
            get { return Stat.TotalExp; }
            set { Stat.TotalExp = value; }
        }

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        public override int TotalAttack 
        { 
            get 
            {
                int totalAttack = Stat.Attack + WeaponDamage;

                switch (JobClassType)
                {
                    case JobClassType.Warrior:
                        totalAttack += Stat.Str;
                        break;
                    case JobClassType.Hunter:
                        break;
                    case JobClassType.Mage:
                        totalAttack += Stat.Int;
                        break;
                    default:
                        break;
                }

                return totalAttack;
            } 
        }
        public override int TotalDefence { get { return Stat.Defence + ArmorDefence + Condition.BuffDefence(); } }
        public override int TotalMaxHp { get { return Stat.MaxHp + Condition.BuffMaxHp(); } }
        public int TotalMaxMp { get { return Stat.MaxMp + Condition.BuffMaxMp(); } }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            JobClassType = JobClassType.None;
            Vision = new VisionCube(this);
            Skill = new SkillManager(this);
            Quest = new QuestManager(this);
            Obstacle = new ObstacleManager();
            Communication = new Communication(this);

        }

        public override void OnDamaged(GameObject attacker, int damage, bool trueDamage = false)
        {
            if (Room == null)
                return;

            int totalDamage = damage;

            // 데미지 보정
            if (trueDamage == false)
                totalDamage = Math.Max(damage - TotalDefence, 0);

            // 버프 효과 적용
            totalDamage = Condition.PlayerBuffDamage(totalDamage);
                      
            Hp -= totalDamage;

            UpdateHpMpStat();

            if (Hp <= 0)
            {
                OnDead(attacker);
            }

        }

        public void UseMp(int mp)
        {
            if (Room == null)
                return;

            Mp = Math.Max(Mp - mp, 0);

            if (Mp < 0)
                Mp = 0;

            UpdateHpMpStat();
        }

        public override void OnDead(GameObject attacker)
        {
            if (Room == null)
                return;
            if (State == CreatureState.Dead)
                return;

            Condition.BackCondition();

            // UI 띄우기
            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, diePacket);

            // Zone과 Collision 삭제
            BecomeGhost();
        }

        public bool IsChangedRoom = false;
        public void OnLeaveGame()
        {
            // DB 연동
            DbTransaction.SavePlayerStatus_AllInOne(this, Room);
            DbTransaction.SavePlayerQuests_AllInOne(this, Room);

            // Room이동 때 다시 DB에서 불러오기 때문에 전부 Clear해 줌
            Inven.Clear();
            Obstacle.Clear();
            Vision.Clear();
            Quest.Clear();

            if (!IsChangedRoom && Communication.Party != null)
                Communication.RemoveParty();

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

            // 소모품, 기타탬은 장착 x
            if (item.ItemType == ItemType.Consumable || item.ItemType == ItemType.Collection)
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
                LevelUp();
                if (Room != null)
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

        public void GetGold(int gold)
        {
            if (gold == 0)
                return;

            // 골드의 경우 중요하기 때문에 바로바로 저장
            DbTransaction.ChangeGoldNoti(this, gold);

            S_ChangeGold changeGoldPacket = new S_ChangeGold();
            changeGoldPacket.Gold = Info.Gold;

            Session.Send(changeGoldPacket);
        }

        public void UpClass(int classGrade)
        {
            if(classGrade == 1)
            {// 1차 전직
                Stat.CanUpClass = true;
                S_ClassUp classUpPacket = new S_ClassUp();
                Session.Send(classUpPacket);
            }
            else if(classGrade == 2)
            {// 2차 전직

            }

        }

        public void LevelUp()
        {
            // 풀피 풀마나
            RecoveryHp(TotalMaxHp);
            RecoveryMp(TotalMaxMp);
            // 직업별로 레벨업 스텟상승
            switch (JobClassType)
            {
                case JobClassType.None:
                    {
                        Stat.MaxHp += 5;
                        Stat.MaxMp += 5;
                        Stat.Str += 1;
                        Stat.Int += 1;
                    }
                    break;
                case JobClassType.Warrior:
                    {
                        Stat.MaxHp += 10;
                        Stat.MaxMp += 5;
                        Stat.Str += 2;
                        Stat.Int += 1;
                    }
                    break;
                case JobClassType.Mage:
                    {
                        Stat.MaxHp += 5;
                        Stat.MaxMp += 10;
                        Stat.Str += 1;
                        Stat.Int += 2;
                    }
                    break;
                default:
                    break;
            }



            // 스킬 포인트 5 지급
            Skill.GetSkillPoints(1);
            GetStatPoints(10);
        }

        public void GetStatPoints(int points)
        {
            if (points <= 0)
                return;

            Stat.StatPoints += points;

            // S_StatPoint 패킷 전송
            UpdateClientStat();
        }

        public void RecoveryMp(int mp)
        {
            if (Room == null)
                return;

            Mp += mp;

            UpdateHpMpStat();
        }

        public void BecomeGhost()
        {// Zone과 Collision 삭제
            State = CreatureState.Dead;
            //Room.Map.SetOffCollsion(this);
        }

        public void Respawn()
        {
            if (Vision.job != null)
                Vision.job.Cancel = true;
            Vision.job = null;

            GameRoom room = Room;
            room.LeaveGame(Id);

            Stat.Hp = TotalMaxHp;
            Stat.Mp = TotalMaxMp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;
            PosInfo.PosX = -50;
            PosInfo.PosY = -75;

            room.EnterGame(this);
        }

        public override void UpdateClientStat()
        {
            // S_StatPoint 패킷 전송
            S_StatPoint statPointPacket = new S_StatPoint();
            statPointPacket.StatInfo = new StatInfo(Stat);
            statPointPacket.StatInfo.MaxHp = TotalMaxHp;
            statPointPacket.StatInfo.MaxMp = TotalMaxMp;
            statPointPacket.StatInfo.Defence = Stat.Defence + Condition.BuffDefence();

            Session.Send(statPointPacket);
        }

        public override void UpdateHpMpStat()
        {
            if (Room == null)
                return;

            S_ChangeHp changeHp = new S_ChangeHp();
            changeHp.ObjectId = Id;
            changeHp.Hp = Hp;
            changeHp.MaxHp = TotalMaxHp;
            S_ChangeMp changeMp = new S_ChangeMp();
            changeMp.ObjectId = Id;
            changeMp.Mp = Mp;
            changeMp.MaxMp = TotalMaxMp;

            Room.Broadcast(CellPos, changeHp);
            Room.Broadcast(CellPos, changeMp);

            // 파티의 경우 VisionCube범위가 아니더라도 체력 정보가 정송 돼야함
            Communication.SendPartyInfo();
        }
    }
}
