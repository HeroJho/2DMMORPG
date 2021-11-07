using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Monster : CreatureObject
    {
        public int TemplateId { get; protected set; }
        protected Player _target;
        protected MonsterData _monsterData;
        protected Spawner _spawner;

        protected Vector2Int _beginPos;

        public Monster()
        {
            ObjectType = GameObjectType.Monster;

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
        }

        // FSM (Finite State Machine)
        private IJob _job;
        public override void Update()
        {
            switch (State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
                case CreatureState.Callback:
                    UpdateCallback();
                    break;
            }

            // 10프레임 (0.1초마다 한번씩 Update)
            if (Room != null)
                _job = Room.PushAfter(100, Update);
        }

        protected long _nextSearchTick = 0;
        protected int _searchCellDist = 0;
        public virtual void UpdateIdle()
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

        protected long _nextMoveTick = 0;
        protected int _chaseCellDist = 0;
        public virtual void UpdateMoving()
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
            if(_spawner != null)
            {
                if (_spawner.MaxX + _chaseCellDist < CellPos.x || _spawner.MinX - _chaseCellDist > CellPos.x ||
                    _spawner.MaxY + _chaseCellDist < CellPos.y || _spawner.MinY - _chaseCellDist > CellPos.y)
                {
                    _target = null;
                    State = CreatureState.Callback;
                    BroadcastMove();
                    return;
                }
            }
            else
            {
                if (_beginPos.x + 5 + _chaseCellDist < CellPos.x || _beginPos.x - 5 - _chaseCellDist > CellPos.x ||
                    _beginPos.y + 5 + _chaseCellDist < CellPos.y || _beginPos.y - 5 - _chaseCellDist > CellPos.y)
                {
                    _target = null;
                    State = CreatureState.Callback;
                    BroadcastMove();
                    return;
                }
            }


            // 길찾기 계산 && 추격 범위 검사
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObject: true);
            if(path.Count < 2)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 스킬로 넘어갈지 체크
            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            if (dist <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                BroadcastMove();
                return;
            }

            // 서버 좌표 갱신
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);

            // 클라 갱신
            BroadcastMove();
        }

        protected int _skillRange = 1;
        protected long _coolTick = 0;
        public virtual void UpdateSkill()
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
                if(canUseSkill == false)
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

                // 스킬 쿨타임 적용
                int collTick = (int)(1000 * Stat.AttackSpeed);
                _coolTick = Environment.TickCount64 + collTick;
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }

        public virtual void UpdateDead()
        {

        }

        protected long _callMoveTick = 0;
        public virtual void UpdateCallback()
        {
            if (_callMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / Speed);
            _callMoveTick = Environment.TickCount64 + moveTick;

            if (CellPos.x == _beginPos.x && CellPos.y == _beginPos.y)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 길찾기 계산 && 추격 범위 검사
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _beginPos, checkObject: true);
            if (path == null || path.Count < 2)
            {
                _target = null;
                State = CreatureState.Callback;
                BroadcastMove();
                return;
            }

            // 서버 좌표 갱신
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);

            // 클라 갱신
            BroadcastMove();
        }

        protected void BroadcastMove()
        {
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(CellPos, movePacket);
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

            if(owner.ObjectType == GameObjectType.Player)
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
                GameRoom room = Room;
                if (room == null)
                    return;
                Vector2Int pos = CellPos;

                // 선DB   후메모리 & 전송
                DbTransaction.DropItem(pos, rewardData, room);
            }

            {
                S_Die diePacket = new S_Die();
                diePacket.ObjectId = Id;
                diePacket.AttackerId = attacker.Id;
                Room.Broadcast(CellPos, diePacket);

                GameRoom room = Room;
                room.LeaveGame(Id);

                if(_spawner != null)
                    _spawner.Dead(this);
            }
        }

        public void ReSpawn(Vector2Int randPos, GameRoom room)
        {
            if (room == null)
                return;

            Stat.Hp = TotalMaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;
            CellPos = randPos;
            _beginPos = randPos;

            room.EnterGame(this);
        }

        RewardData GetRandomReward()
        {
            int rand = new Random().Next(0, 101);
            int sum = 0;
            foreach (RewardData rewardData in _monsterData.rewards)
            {
                sum += rewardData.probability;
                if (rand <= sum)
                    return rewardData;
            }

            return null;
        }
    }
}
