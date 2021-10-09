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

        public void TickDamage(int damageValue, int timeValue)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;


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


        }
    }
}
