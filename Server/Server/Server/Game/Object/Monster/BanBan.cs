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

        public override void Init(int templateId, Spawner spawner, Vector2Int beginPos)
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

                // 스킬 사용 Broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = 1;
                Room.Broadcast(CellPos, skill);

                State = CreatureState.Moving;

                // 스킬 쿨타임 적용
                int collTick = (int)(1000 * Stat.AttackSpeed);
                _coolTick = Environment.TickCount64 + collTick;

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
                    Skill_1();
                    return true;
                case SkillState.Skill_2:
                    Skill_2();
                    return true;
                case SkillState.Skill_3:
                    Skill_3();
                    return true;
                default:
                    return false;
            }
        }

        public SkillState ChooseRandomSkill()
        {
            int rand = new Random().Next(0, 100);
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

        void Skill_1()
        {
            ShootIceBall(PosInfo.MoveDir);
        }
        void Skill_2()
        {
            ShootIceBall(MoveDir.Down);
            ShootIceBall(MoveDir.Up);
            ShootIceBall(MoveDir.Left);
            ShootIceBall(MoveDir.Right);
        }
        void Skill_3()
        {
            for (int i = 1; i < 5; i++)
            {
                ShootIceBall(PosInfo.MoveDir);
                Room.PushAfter(i * 300, ShootIceBall, PosInfo.MoveDir);
            }
        }

        void ShootIceBall(MoveDir dir)
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
    }
}
