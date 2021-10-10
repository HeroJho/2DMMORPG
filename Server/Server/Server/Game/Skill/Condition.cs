using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Condition
    {
        private CreatureObject _creatureObj;

        public Condition(CreatureObject creatureObj)
        {
            _creatureObj = creatureObj;
           
        }

        public void Chilled(Skill skillData, int skillLevel)
        {
            int time = skillData.conditions[skillLevel].Time;
            int moveSlowValue = skillData.conditions[skillLevel].MoveSpeedValue;
            int AttackSlowValue = skillData.conditions[skillLevel].AttackSpeedValue;

            SlowSpeed(moveSlowValue, time);
            SlowAttackSpeed(AttackSlowValue, time);

            SendConditionPacket(ConditionType.ConditionChilled, time);
        }

        public void Poison(Skill skillData, int skillLevel, GameObject speller)
        {
            if(speller == null || speller.Room == null)
                return;

            int time = skillData.conditions[skillLevel].Time;
            int damageValue = skillData.conditions[skillLevel].TickDamageValue;

            TickDamage(damageValue, time, speller);

            SendConditionPacket(ConditionType.ConditionPoison, time);
        }

        public void SendConditionPacket(ConditionType conditionType, int timeValue)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;

            S_ChangeConditionInfo changeCondition = new S_ChangeConditionInfo();

            changeCondition.Id = _creatureObj.Id;

            changeCondition.ConditionType = conditionType;
            changeCondition.Time = timeValue;
            changeCondition.MoveSpeed = _creatureObj.Speed;
            changeCondition.AttackSpeed = _creatureObj.Stat.Attack;

            room.Broadcast(_creatureObj.CellPos, changeCondition);
        }


        IJob _slowJob;
        float _originSpeed; 
        public void SlowSpeed(int slowValue, int timeValue)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;

            // 중복되면 이전거는 취소 되고 시간도 초기화하고 진행
            if (_slowJob != null)
            {
                _creatureObj.Speed = _originSpeed;
                _slowJob.Cancel = true;
                _slowJob = null;
            }

            // 속도 감소
            _originSpeed = _creatureObj.Speed;
            _creatureObj.Speed -= slowValue;

            if (_creatureObj.Speed <= 0)
                _creatureObj.Speed = 1;           

            // 시간후에 원래속도 되돌림
            _slowJob = room.PushAfter(timeValue * 1000, () =>
            {
                _creatureObj.Speed = _originSpeed;
                _slowJob = null;
            });
        }

        IJob _slowAtteckJob;
        float _originAtteckSpeed;
        public void SlowAttackSpeed(int slowValue, int timeValue)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;

            // 중복되면 이전거는 취소 되고 시간도 초기화하고 진행
            if (_slowAtteckJob != null)
            {
                _creatureObj.Stat.AttackSpeed = _originAtteckSpeed;
                _slowAtteckJob.Cancel = true;
                _slowAtteckJob = null;
            }

            // 원래 속도 저장 후 감소
            _originAtteckSpeed = _creatureObj.Stat.AttackSpeed;
            _creatureObj.Stat.AttackSpeed += slowValue;

            // 시간후에 원래속도 되돌림
            _slowAtteckJob = room.PushAfter(timeValue * 1000, () =>
            {
                _creatureObj.Stat.AttackSpeed = _originAtteckSpeed;
                _slowAtteckJob = null;
            });
        }

        IJob _tickDamageJob;
        IJob _tickJob;
        int _currentTickDamage = 0;
        public void TickDamage(int damageValue, int timeValue, GameObject speller)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null || speller == null)
                return;

            // 현재 데미지 보다 작거나 같으면 중첩 X > 종료
            if(_currentTickDamage >= damageValue)
                return;

            // 속도 감소
            _currentTickDamage = damageValue;

            // 초당 틱 반복 감소 시작
            UpdateTick(_currentTickDamage, speller);

            // 지속시간이 끝나면 종료
            _tickDamageJob = room.PushAfter(timeValue * 1000, () =>
            {
                _tickDamageJob = null;
                _tickJob.Cancel = true;
                _tickJob = null;
                _currentTickDamage = 0;
            });
        }
        void UpdateTick(int damageValue, GameObject speller)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null || speller == null)
                return;

            _tickJob = room.PushAfter(1000, UpdateTick, damageValue, speller);

            _creatureObj.OnDamaged(speller, damageValue);
        }

        public void Stun(int timeValue)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;


        }

        public void BackCondition()
        {
            if(_slowJob != null)
            {
                _creatureObj.Speed = _originSpeed;
                _slowJob.Cancel = true;
                _slowJob = null;
            }    
            if(_slowAtteckJob != null)
            {
                _creatureObj.Stat.AttackSpeed = _originAtteckSpeed;
                _slowAtteckJob.Cancel = true;
                _slowAtteckJob = null;
            }
            if (_tickDamageJob != null)
            {
                _currentTickDamage = 0;
                _tickDamageJob.Cancel = true;
                _tickJob.Cancel = true;
                _tickDamageJob = null;
                _tickJob = null;
            }


        }
    }
}
