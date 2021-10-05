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
            int value = skillData.conditions[skillLevel].Value;
        }



        IJob _slowJob;
        float _originSpeed; 
        public void SlowSpeed(int slowValue, int timeValue)
        {
            if (_slowJob != null)
                return;

            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;

            // 원래 속도 저장 후 감소
            _originSpeed = _creatureObj.Speed;
            _creatureObj.Speed -= slowValue;

            if (_creatureObj.Speed <= 0)
                _creatureObj.Speed = 1;

            //패킷을 보낸다
            S_Condition conditionPacket = new S_Condition();
            conditionPacket.Id = _creatureObj.Id;
            conditionPacket.ConditionInfo = new ConditionInfo()
            {
                ConditionType = ConditionType.ConditionChilled,
                Time = timeValue,
                Value = slowValue
            };
            room.Broadcast(_creatureObj.CellPos, conditionPacket);            

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

            // 원래 속도 저장 후 감소
            _originAtteckSpeed = _creatureObj.Stat.AttackSpeed;
            _creatureObj.Stat.AttackSpeed += slowValue;

            //패킷을 보낸다
            S_Condition conditionPacket = new S_Condition();
            conditionPacket.Id = _creatureObj.Id;
            conditionPacket.ConditionInfo = new ConditionInfo()
            {
                ConditionType = ConditionType.ConditionChilled,
                Time = timeValue,
                Value = slowValue
            };
            room.Broadcast(_creatureObj.CellPos, conditionPacket);

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


        }
    }
}
