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
            if (_creatureObj.State == CreatureState.Dead)
                return;

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
            if (_creatureObj.State == CreatureState.Dead)
                return;

            int time = skillData.conditions[skillLevel].Time;
            int damageValue = skillData.conditions[skillLevel].TickValue;

            TickDamage(damageValue, time, speller);

            SendConditionPacket(ConditionType.ConditionPoison, time);
        }

        public void Healing(Skill skillData, int skillLevel, GameObject speller)
        {
            if (speller == null || speller.Room == null)
                return;
            if (_creatureObj.State == CreatureState.Dead)
                return;

            int time = skillData.conditions[skillLevel].Time;
            int healValue = skillData.conditions[skillLevel].TickValue;

            TickHeal(healValue, time, speller);

            SendConditionPacket(ConditionType.ConditionHealing, time);
        }

        IJob _magicGuardJob;
        int _magicGuardValue = 0;
        public void MagicGuard(Skill skillData, int skillLevel)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;
            if (_creatureObj.State == CreatureState.Dead)
                return;

            // 중복되면 이전거는 취소 되고 시간도 초기화하고 진행
            if (_magicGuardJob != null)
            {
                _magicGuardValue = 0;
                _magicGuardJob.Cancel = true;
                _magicGuardJob = null;
            }

            int time = skillData.conditions[skillLevel].Time;
            _magicGuardValue = skillData.conditions[skillLevel].CommonValue;

            // 시간후에 원래속도 되돌림
            _magicGuardJob = room.PushAfter(time * 1000, () =>
            {
                _magicGuardValue = 0;
                _magicGuardJob = null;
            });
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

            if (_creatureObj.State == CreatureState.Dead)
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

            if (_creatureObj.State == CreatureState.Dead)
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
        IJob _updateTickDamageJob;
        int _currentTickDamage = 0;
        public void TickDamage(int damageValue, int timeValue, GameObject speller)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null || speller == null)
                return;

            if (_creatureObj.State == CreatureState.Dead)
                return;

            // 현재 데미지 보다 작거나 같으면 중첩 X > 종료
            if (_currentTickDamage >= damageValue)
                return;

            _currentTickDamage = damageValue;

            // 초당 틱 반복 감소 시작
            UpdateTickDamage(_currentTickDamage, speller);

            // 지속시간이 끝나면 종료
            _tickDamageJob = room.PushAfter(timeValue * 1000, () =>
            {
                _tickDamageJob = null;
                _updateTickDamageJob.Cancel = true;
                _updateTickDamageJob = null;
                _currentTickDamage = 0;
            });
        }
        void UpdateTickDamage(int damageValue, GameObject speller)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null || speller == null)
                return;

            _updateTickDamageJob = room.PushAfter(1000, UpdateTickDamage, damageValue, speller);

            CreatureObject co = (CreatureObject)speller;

            // 틱 데미지는 백분율 해서 고정피해로 적용
            int damage = Math.Max((damageValue / 100) * co.TotalAttack, 1); 
            _creatureObj.OnDamaged(speller, damage, true);
        }

        IJob _tickHealJob;
        IJob _updateTickHealJob;
        int _currentTickHeal = 0;
        public void TickHeal(int healValue, int timeValue, GameObject speller)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null || speller == null)
                return;

            if (_creatureObj.State == CreatureState.Dead)
                return;

            // 현재 데미지 보다 작거나 같으면 중첩 X > 종료
            if (_currentTickHeal >= healValue)
                return;

            _currentTickHeal = healValue;

            // 초당 틱 반복 감소 시작
            UpdateTickHeal(_currentTickHeal, speller);

            // 지속시간이 끝나면 종료
            _tickHealJob = room.PushAfter(timeValue * 1000, () =>
            {
                _tickHealJob = null;
                _updateTickHealJob.Cancel = true;
                _updateTickHealJob = null;
                _currentTickHeal = 0;
            });
        }
        void UpdateTickHeal(int healValue, GameObject speller)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null || speller == null)
                return;

            _updateTickHealJob = room.PushAfter(1000, UpdateTickHeal, healValue, speller);

            CreatureObject co = (CreatureObject)speller;

            // 틱 힐은 백분율 해서 적용
            int heal = Math.Max((healValue / 100) * co.TotalAttack, 1);
            _creatureObj.RecoveryHp(heal);
        }

        IJob _stunJob;
        CreatureState _previousState;
        public void Stun(int timeValue, int stunChanceValue)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;
            if (_creatureObj.State == CreatureState.Dead)
                return;

            // 확률
            int rand = new Random().Next(0, 101);
            if (rand >= stunChanceValue)
                return;

            if (_stunJob != null)
            {
                _creatureObj.State = _previousState;
                _stunJob.Cancel = true;
                _stunJob = null;
            }

            _previousState = _creatureObj.State;
            _creatureObj.State = CreatureState.Stun;

            // 지속시간이 끝나면 종료
            _stunJob = room.PushAfter(timeValue * 1000, () =>
            {
                _stunJob = null;
                _creatureObj.State = _previousState;
            });

            SendConditionPacket(ConditionType.ConditionStun, timeValue);
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
                _updateTickDamageJob.Cancel = true;
                _tickDamageJob = null;
                _updateTickDamageJob = null;
            }
            if (_tickHealJob != null)
            {
                _currentTickHeal = 0;
                _tickHealJob.Cancel = true;
                _updateTickHealJob.Cancel = true;
                _tickHealJob = null;
                _updateTickHealJob = null;
            }
            if (_stunJob != null)
            {
                _creatureObj.State = _previousState;
                _stunJob.Cancel = true;
                _stunJob = null;
            }


        }



        public int PlayerBuffDamage(int damage)
        {
            if (!(_creatureObj is Player))
                return 0;

            Player player = (Player)_creatureObj;
            int totalDamage = damage;

            if(_magicGuardJob != null)
            {
                int minusDamage = totalDamage * (_magicGuardValue/100);
                int minusMp = damage - minusDamage;

                if(player.Mp >= minusMp && minusDamage != 0)
                {
                    player.UseMp(minusMp);
                    totalDamage -= minusDamage;
                }
            }

            return totalDamage;
        }

    }
}
