using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{ 
    public class BanBan : Monster
    {
        public enum SkillState { None, Skill_1, Skill_2, Skill_3 }
        public List<Monster> Minions = new List<Monster>();

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
        }

        public override void Update()
        {
            base.Update();
        }

        public override void UpdateIdle()
        {
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

            // 길찾기 계산 && 추격 범위 검사
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObject: true);
            if (path.Count < 2)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            if (ChooseSkill())
                return;

            // 서버 좌표 갱신
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);

            // 클라 갱신
            BroadcastMove();
        }

        public override void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                // 유효한 타겟인지
                if (_target == null || _target.Room != Room || _target.State == CreatureState.Dead)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                // 스킬이 아직 사용 가능한지
                Vector2Int dir = _target.CellPos - CellPos;
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));
                if (canUseSkill == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
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
                _target.Condition.Stun(3, 25);

                // 스킬 사용 Broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = 1;
                Room.Broadcast(CellPos, skill);

                // 스킬 쿨타임 적용
                int collTick = (int)(1000 * Stat.AttackSpeed);
                _coolTick = Environment.TickCount64 + collTick;

                State = CreatureState.Moving;
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }

        public override void UpdateDead()
        {
            base.UpdateDead();
        }

        public override void UpdateCallback()
        {
            base.UpdateCallback();
        }

        public bool ChooseSkill()
        {
            SkillState skill = SkillState.None;

            skill = ChooseRandomSkill();

            switch (skill)
            {
                case SkillState.None:
                    // 스킬로 넘어갈지 체크
                    Vector2Int dir = _target.CellPos - CellPos;
                    int dist = dir.cellDistFromZero;
                    if (dist <= _skillRange && (dir.x == 0 || dir.y == 0))
                    {
                        _coolTick = 0;
                        State = CreatureState.Skill;
                        BroadcastMove();
                        return true;
                    }
                    else
                        return false;
                case SkillState.Skill_1:
                    Skill_9();
                    return true;
                case SkillState.Skill_2:
                    Skill_1();
                    return true;
                case SkillState.Skill_3:
                    Skill_8();
                    return true;
                default:
                    return false;
            }
        }

        public SkillState ChooseRandomSkill()
        {
            int rand = new Random().Next(0, 101);
            if (rand < 20)
            {
                return SkillState.Skill_1;
            }
            else if(rand < 30)
            {
                return SkillState.Skill_2;
            }
            else if (rand < 35)
            {
                return SkillState.Skill_3;
            }

            return SkillState.None;
        }

        void Skill_1() // 전방으로 아이스볼을 한 번 발사한다
        {
            ShootIceBall(PosInfo.MoveDir);
        }
        void Skill_2() // 십자가 모양으로 아이스볼을 발사한다
        {
            ShootIceBall(MoveDir.Down);
            ShootIceBall(MoveDir.Up);
            ShootIceBall(MoveDir.Left);
            ShootIceBall(MoveDir.Right);
        }
        void Skill_3() // 전방으로 아이스볼을 5번 발사한다
        {
            for (int i = 1; i < 5; i++)
            {
                ShootIceBall(PosInfo.MoveDir);
                Room.PushAfter(i * 300, ShootIceBall, PosInfo.MoveDir);
            }
        }
        void Skill_4() // 독가스
        {
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
            Room.PushAfter(1500, () =>
            {
                Room.EnterGame(poisonSmoke);
            });
        }
        void Skill_5() // 랜덤하게 몬스터를 소환한다
        {
            Monster monster = ObjectManager.Instance.Add<Monster>();

            Vector2Int? pos = GetRandomPos(CellPos);
            if (pos == null)
                return;

            int rand = new Random().Next(1, 4); // 1~3
            monster.Init(rand, pos.Value);
            Minions.Add(monster);

            Room.EnterGame(monster);
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
        void Skill_7() // 원형으로 스턴 + 피 10
        {
            HashSet<CreatureObject> objects = Room.Map.LoopByCircle<CreatureObject>(CellPos, 5);
            foreach (CreatureObject co in objects)
            {
                int damage = co.Hp - 10;
                co.OnDamaged(this, damage, true);
                co.Condition.Stun(5, 100);
            }
        }
        void Skill_8() // 회복
        {
            Skill skillData = new Skill();
            ConditionInfo condition = new ConditionInfo();
            condition.Time = 10;
            condition.TickValue = 500;
            skillData.conditions = new List<ConditionInfo>();
            skillData.conditions.Add(condition);

            Condition.Healing(skillData, 0, this);
        }
        void Skill_9() // 벽소환 Test
        {
            Wall wall = ObjectManager.Instance.Add<Wall>();

            Vector2Int? pos = GetRandomPos(CellPos);
            if (pos == null)
                return;

            wall.Init(5, pos.Value);
            Minions.Add(wall);

            Room.EnterGame(wall);
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


    }
}
