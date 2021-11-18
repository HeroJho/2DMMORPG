using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class BanBan : Monster
    {
        public enum SkillState { None, Skill_1, Skill_2, Skill_3, Skill_4, Skill_5, Skill_6, Skill_7, Skill_8, Skill_9, Skill_10 }
        public enum BossPage { Page_1, Page_2, Page_3 }
        public List<Monster> Minions = new List<Monster>();
        private BossPage _page;
        private Random _random = new Random();

        public override int Hp
        {
            get { return Stat.Hp; }
            set
            {
                Stat.Hp = Math.Clamp(value, 0, TotalMaxHp);
                if (_page == BossPage.Page_1 && Stat.Hp < TotalMaxHp * 0.7)
                {
                    Console.WriteLine("Page2!!");
                    _page = BossPage.Page_2;
                }
                else if (_page == BossPage.Page_2 && Stat.Hp < TotalMaxHp * 0.4)
                {
                    Console.WriteLine("Page3!!");
                    _page = BossPage.Page_3;
                }

            }
        }

        public virtual void Init(int templateId, Vector2Int beginPos, Spawner spawner = null)
        {
            _spawner = spawner;

            TemplateId = templateId;

            DataManager.MonsterDict.TryGetValue(templateId, out _monsterData);
            Stat.MergeFrom(_monsterData.stat);
            Stat.Hp = _monsterData.stat.MaxHp;
            _searchCellDist = _monsterData.searchCellDist;
            _chaseCellDist = _monsterData.chaseCellDist;
            State = CreatureState.Idle;
            Info.TemplateId = templateId;

            CellPos = beginPos;
            _beginPos = beginPos;

            _page = BossPage.Page_1;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void UpdateIdle()
        {
            if (_isUsingSkill)
                return;

            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000; // 초당 실행

            Player target = Room.FindPlayer(p =>
            {

                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero <= _searchCellDist && p.State != CreatureState.Dead;
            });

            if (target == null)
                return;

            _target = target;
            State = CreatureState.Moving;
        }

        public override void UpdateMoving()
        {
            if (_isUsingSkill)
                return;

            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            if (_target == null || _target.Room == null || _target.State == CreatureState.Dead)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            //추격 범위를 벗어났냐
            if (_spawner.MaxX + _chaseCellDist < CellPos.x || _spawner.MinX - _chaseCellDist > CellPos.x ||
               _spawner.MaxY + _chaseCellDist < CellPos.y || _spawner.MinY - _chaseCellDist > CellPos.y)
            {
                _target = null;
                State = CreatureState.Callback;
                BroadcastMove();
                return;
            }

            // 스킬 사용
            if (ChooseSkill())
                return;

            // 길찾기 계산 && 추격 범위 검사
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObject: true);
            if (path.Count < 2)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 서버 좌표 갱신
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);

            // 클라 갱신
            BroadcastMove();
        }

        public override void UpdateSkill()
        {
            // 반반은 평타를 스킬처럼 사용하도록 구현함
        }

        public override void UpdateDead()
        {
            base.UpdateDead();
        }

        public override void UpdateCallback()
        {
            if (_isUsingSkill)
                return;

            base.UpdateCallback();
        }

        public bool ChooseSkill()
        {
            SkillState skill = SkillState.None;

            skill = ChooseRandomSkill();

            switch (skill)
            {
                case SkillState.None:
                    return Skill_0();
                case SkillState.Skill_1:
                    Skill_1();
                    return true;
                case SkillState.Skill_2:
                    Skill_2();
                    return true;
                case SkillState.Skill_3:
                    Skill_3();
                    return true;
                case SkillState.Skill_4:
                    //Skill_4();
                    return true;
                case SkillState.Skill_5:
                    Skill_5();
                    return true;
                case SkillState.Skill_6:
                    Skill_6();
                    return true;
                case SkillState.Skill_7:
                    Skill_7();
                    return true;
                case SkillState.Skill_8:
                    Skill_8();
                    return true;
                case SkillState.Skill_9:
                    Skill_9();
                    return true;
                case SkillState.Skill_10:
                    Skill_10();
                    return true;
                default:
                    return false;
            }
        }

        public SkillState ChooseRandomSkill()
        {
            int rand = _random.Next(0, 101);

            switch (_page)
            {
                case BossPage.Page_1:
                    if (rand < 20) // 20
                        return SkillState.Skill_2;
                    else if (rand < 25) // 5
                        return SkillState.Skill_4;
                    else if (rand < 35) // 10
                        return SkillState.Skill_3;
                    break; // 60
                case BossPage.Page_2:
                    if (rand < 40) // 40
                        return SkillState.Skill_5; // 몬스터 솬
                    else if (rand < 50) // 10
                        return SkillState.Skill_6; // 버프
                    else if (rand < 70) // 20
                        return SkillState.Skill_8; // 회복
                    else if (rand < 80) // 10
                        return SkillState.Skill_3; // 전방 5번 스킬
                    break; // 20
                case BossPage.Page_3:
                    if (rand < 40) // 40
                        return SkillState.Skill_7; // 원형 즉사
                    else if (rand < 80) // 40
                        return SkillState.Skill_10; // 전방 즉사
                    else if (rand < 90) // 10
                        return SkillState.Skill_9; // 벽 소환
                    else if (rand < 100) // 10
                        return SkillState.Skill_2; // 십자 슬로우
                    break; // 0
                default:
                    break;
            }

            return SkillState.None;
        }

        private bool _isUsingSkill = false;
        bool Skill_0() // 평타 >> 완료 
        {
            _isUsingSkill = true;

            // 유효한 타겟인지
            if (_target == null || _target.Room != Room || _target.State == CreatureState.Dead)
            {
                _target = null;
                _isUsingSkill = false;
                BroadcastMove();
                return false;
            }

            // 스킬이 아직 사용 가능한지
            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));
            if (canUseSkill == false)
            {
                _isUsingSkill = false;
                BroadcastMove();
                return false;
            }

            // 타게팅 방향 주시
            MoveDir lookDir = GetDirFromVec(dir);
            if (Dir != lookDir)
            {
                Dir = lookDir;
                BroadcastMove();
            }

            // 데미지 판정
            _target.OnDamaged(this, Stat.Attack + TotalAttack);
            // 평타에 일정확률로 기절
            _target.Condition.Stun(3, 5);

            // 스킬 사용 Broadcast
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Id;
            skill.Info.SkillId = 0;
            Room.Broadcast(CellPos, skill);

            Room.PushAfter(2000, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;
            });
            return true;
        }
        void Skill_1() // 전방으로 아이스볼을 한 번 발사한다 >> 완료 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 1;
            skill.Info.Point = 0;
            Room.Broadcast(CellPos, skill);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(2000, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                ShootIceBall(PosInfo.MoveDir);
            });
        }
        void Skill_2() // 십자가 모양으로 아이스볼을 발사한다 >> 완료 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 1;
            skill.Info.Point = 0;
            Room.Broadcast(CellPos, skill);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(2000, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                ShootIceBall(MoveDir.Down);
                ShootIceBall(MoveDir.Up);
                ShootIceBall(MoveDir.Left);
                ShootIceBall(MoveDir.Right);
            });
        }
        void Skill_3() // 전방으로 아이스볼을 5번 발사한다 >> 완료 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 1;
            skill.Info.Point = 0;
            Room.Broadcast(CellPos, skill);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(2000, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                for (int i = 1; i < 5; i++)
                {
                    ShootIceBall(PosInfo.MoveDir);
                    Room.PushAfter(i * 300, ShootIceBall, PosInfo.MoveDir);
                }
            });
        }
        void Skill_4() // 독가스 >> 완료 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 1;
            skill.Info.Point = 0;
            Room.Broadcast(CellPos, skill);


            PoisonSmoke poisonSmoke = ObjectManager.Instance.Add<PoisonSmoke>();
            if (poisonSmoke == null)
                return;

            poisonSmoke.Owner = this;
            poisonSmoke.Info.TemplateId = 2003;

            Skill skillData = null;
            DataManager.SkillDict.TryGetValue(2003, out skillData);

            poisonSmoke.PosInfo.State = CreatureState.Idle;
            poisonSmoke.PosInfo.MoveDir = PosInfo.MoveDir;
            poisonSmoke.PosInfo.PosX = PosInfo.PosX;
            poisonSmoke.PosInfo.PosY = PosInfo.PosY;
            poisonSmoke.Stat.Level = 4; // 클라 크기조절 용
            poisonSmoke.Init(skillData, 4);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(2000, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                Room.EnterGame(poisonSmoke);
            });
        }
        void Skill_5() // 뒤틀린 몬스터를 소환한다 >> 완료 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 5;
            skill.Info.Point = 0;
            Room.Broadcast(CellPos, skill);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(2000, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                Monster monster = ObjectManager.Instance.Add<Monster>();

                Vector2Int? pos = GetRandomPos(CellPos);
                if (pos == null)
                    return;

                monster.Init(6, pos.Value);
                Minions.Add(monster);

                Room.EnterGame(monster);
            });

        }
        void Skill_6() // 소환한 몬스터들에게 IronBody버프를 걸어준다 
        {
            for (int i = 0; i < Minions.Count; i++)
            {
                Monster monster = Minions[i];

                if (monster.Room == null)
                    Minions.Remove(monster);

                Skill skillData = null;
                if (DataManager.SkillDict.TryGetValue(2008, out skillData) == false)
                    return;

                monster.Condition.IronBody(skillData, 2);
                Console.WriteLine(monster.TotalDefence);
            }
        }
        void Skill_7() // 원형으로 스턴 + 피 10 >> 완료 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 7;
            skill.Info.Point = 0;
            Room.Broadcast(CellPos, skill);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(1800, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                HashSet<CreatureObject> objects = Room.Map.LoopByCircle<CreatureObject>(CellPos, 4);
                foreach (CreatureObject co in objects)
                {
                    if (co is Wall)
                        continue;

                    // 두번 스턴이 됐다 >> 즉사
                    if (co.Condition.Buffs.ContainsKey(9999))
                    {
                        co.OnDamaged(this, co.TotalMaxHp, true);
                        continue;
                    }

                    int damage = co.Hp - 10;
                    co.OnDamaged(this, damage, true);
                    co.Condition.Stun(2, 100);
                }
            });
        }
        void Skill_8() // 회복 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 5;
            skill.Info.Point = 0;
            Room.Broadcast(CellPos, skill);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(2000, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                Skill skillData = new Skill();
                ConditionInfo condition = new ConditionInfo();
                condition.Time = 10;
                condition.TickValue = 1000;
                skillData.conditions = new List<ConditionInfo>();
                skillData.conditions.Add(condition);

                Condition.Healing(skillData, 0, this);
            });

        }
        void Skill_9() // 벽소환 >> 완료 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 5;
            skill.Info.Point = 0;
            Room.Broadcast(CellPos, skill);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(2000, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                int rand = _random.Next(3, 8);

                HashSet<Vector2Int> vectors = Room.Map.LoopByCircleWithVector(CellPos, rand, false, true);

                foreach (Vector2Int cellPos in vectors)
                {
                    Pos pos = Room.Map.Cell2Pos(cellPos);

                    GameObject obj = Room.Map.Find(pos.Y, pos.X);
                    if (obj != null)
                        continue;

                    Wall wall = ObjectManager.Instance.Add<Wall>();

                    wall.Init(5, cellPos);

                    Room.EnterGame(wall);
                }
            });

        }
        void Skill_10() // 전방으로 강력한 공격 
        {
            _isUsingSkill = true;

            // 클라 애니메이션 실행
            int dir = _random.Next(0, 4);
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = Info.ObjectId;
            skill.Info.SkillId = 10;
            skill.Info.Point = dir;
            Room.Broadcast(CellPos, skill);

            // 스킬시전 시간 후에 생성
            Room.PushAfter(1800, () =>
            {
                if (this == null || Room == null)
                    return;

                _isUsingSkill = false;

                HashSet<CreatureObject> objects = Room.Map.LoopByOval<CreatureObject>(CellPos, (MoveDir)dir, 10);
                foreach (CreatureObject co in objects)
                {
                    if (co is Wall)
                        continue;

                    // 두번 스턴이 됐다 >> 즉사
                    if (co.Condition.Buffs.ContainsKey(9999))
                    {
                        co.OnDamaged(this, co.TotalMaxHp, true);
                        continue;
                    }

                    int damage = co.Hp - 10;
                    co.OnDamaged(this, damage, true);
                    co.Condition.Stun(2, 100);
                }
            });
        }

        private void ShootIceBall(MoveDir dir)
        {
            IceBall iceBall = ObjectManager.Instance.Add<IceBall>();
            if (iceBall == null)
                return;

            Skill skillData = null;
            DataManager.SkillDict.TryGetValue(2001, out skillData);

            iceBall.Owner = this;
            iceBall.Info.TemplateId = 2001;

            iceBall.PosInfo.State = CreatureState.Moving;
            iceBall.PosInfo.MoveDir = dir;
            iceBall.PosInfo.PosX = PosInfo.PosX;
            iceBall.PosInfo.PosY = PosInfo.PosY;
            iceBall.Init(skillData, 4);

            Room.EnterGame(iceBall);
        }

        private Vector2Int? GetRandomPos(Vector2Int cellPos)
        {
            int MinX = cellPos.x - 4;
            int MaxX = cellPos.x + 4;
            int MinY = cellPos.y - 4;
            int MaxY = cellPos.y + 4;

            int radX = new Random().Next(MinX, MaxX);
            int radY = new Random().Next(MinY, MaxY);
            Vector2Int? randPos = new Vector2Int(radX, radY);

            int tryCount = 0; // 10번만 시도
            while (!Room.Map.CanGo(randPos.Value, checkObject: true))
            {
                tryCount++;
                radX = new Random().Next(MinX, MaxX);
                radY = new Random().Next(MinY, MaxY);
                randPos = new Vector2Int(radX, radY);

                if (tryCount > 10)
                    return null;
            }

            return randPos;
        }

        public override void OnDead(GameObject attacker)
        {
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }

            // 버프 전부 해제
            Condition.BackCondition();

            GameObject owner = attacker.GetOwner();

            if (owner.ObjectType == GameObjectType.Player)
            {

                Player player = (Player)owner;
                // 경험치 획득
                player.GetEx(_monsterData.stat.TotalExp);
                // 퀘스트 진행여부 확인
                player.Quest.ProceddWithQuest(TemplateId);
            }

            // 보상 로직
            RewardData rewardData = GetRandomReward();
            if (rewardData != null)
            {
                // TODO
                // 보스의 경우 바로 보상 인벤으로 꼳아줌
            }

            {
                S_Die diePacket = new S_Die();
                diePacket.ObjectId = Id;
                diePacket.AttackerId = attacker.Id;
                Room.Broadcast(CellPos, diePacket);

                GameRoom room = Room;
                // TODO
                // 보스가 죽었을 경우 자기가 속한 던전맵이라면 몇초 후 원래 맵으로 귀한
                if (room.Map.MapId == 2)
                {                    
                    room.PushAfter(10000, () =>
                    {
                        if (room == null)
                            return;

                        room.ChangeRoomAllPlayer();

                        // 방 제거
                        GameLogic.Instance.Push(() =>
                        {
                            GameLogic.Instance.Remove(room.RoomId);
                            Console.WriteLine("맵 제거!");
                        });
                    });
                }


                room.LeaveGame(Id);

                if (_spawner != null)
                    _spawner.Dead(this);

            }
        }


    }
}
