using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public struct BuffOrConditionTime
    {
        public int SkillId { get; set; }
        public BuffType BuffType { get; set; }
        public ConditionType ConditionType { get; set; }
        public ConditionInfo conditionInfo { get; set; }
        public int Cooldown { get; set; }
        public int StartTime { get; set; }

        public int RemindTick 
        { 
            get
            {
                return ((StartTime + Cooldown * 1000) - Environment.TickCount)/1000;
            }
        }


    }

    public class Condition
    {
        public Dictionary<int, BuffOrConditionTime> Buffs = new Dictionary<int, BuffOrConditionTime>();
        private CreatureObject _creatureObj;

        public Condition(CreatureObject creatureObj)
        {
            _creatureObj = creatureObj;
           
        }

        public void Chilled(Skill skillData, int skillLevel)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;
            if (_creatureObj.State == CreatureState.Dead)
                return;

            int time = skillData.conditions[skillLevel].Time;
            int moveSlowValue = skillData.conditions[skillLevel].MoveSpeedValue;
            int attackSlowValue = skillData.conditions[skillLevel].AttackSpeedValue;
            ConditionInfo info = new ConditionInfo()
            {
                Time = time,
                MoveSpeedValue = moveSlowValue,
                AttackSpeedValue = attackSlowValue
            };

            SlowSpeed(moveSlowValue, time);
            SlowAttackSpeed(attackSlowValue, time);

            // 쿨타임을 추격해줌 >> vision에서 벗어나서 들어올 때 버프가 사라짐
            // 다시 vision에 들어온 player의 버프상태도 같이 보내줘야 함.
            ManageBuffOrCondition(skillData, time, info, ConditionType.ConditionChilled);
            SendConditionPacket(ConditionType.ConditionChilled, time);
        }
        public void Poison(Skill skillData, int skillLevel, GameObject speller)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;
            if (speller == null || speller.Room == null)
                return;
            if (_creatureObj.State == CreatureState.Dead)
                return;

            int time = skillData.conditions[skillLevel].Time;
            int damageValue = skillData.conditions[skillLevel].TickValue;
            ConditionInfo info = new ConditionInfo()
            {
                Time = time,
                TickValue = damageValue
            };

            TickDamage(damageValue, time, speller);

            // 쿨타임을 추격해줌 >> vision에서 벗어나서 들어올 때 버프가 사라짐
            // 다시 vision에 들어온 player의 버프상태도 같이 보내줘야 함.
            ManageBuffOrCondition(skillData, time, info, ConditionType.ConditionPoison);
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
            ConditionInfo info = new ConditionInfo()
            {
                Time = time,
                TickValue = healValue
            };

            TickHeal(healValue, time, speller);

            // 쿨타임을 추격해줌 >> vision에서 벗어나서 들어올 때 버프가 사라짐
            // 다시 vision에 들어온 player의 버프상태도 같이 보내줘야 함.
            ManageBuffOrCondition(skillData, time, info, ConditionType.ConditionHealing);
            SendConditionPacket(ConditionType.ConditionHealing, time);
        }


        IJob _magicGuardJob;
        float _magicGuardValue = 0;
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
            ConditionInfo info = new ConditionInfo()
            {
                Time = time
            };

            // 시간후에 원래속도 되돌림
            _magicGuardJob = room.PushAfter(time * 1000, () =>
            {
                _magicGuardValue = 0;
                _magicGuardJob = null;
            });

            ManageBuffOrCondition(skillData, time, info, ConditionType.ConditionBuff, BuffType.BuffMagicguard);
            SendConditionPacket(ConditionType.ConditionBuff, time, skillData.id);
        }
        IJob _hyperBodyJob;
        int _hyperMaxHp = 0;
        public void HyperBody(Skill skillData, int skillLevel)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;
            if (_creatureObj.State == CreatureState.Dead)
                return;

            // 중복되면 이전거는 취소 되고 시간도 초기화하고 진행
            if (_hyperBodyJob != null)
            {
                _hyperMaxHp = 0;
                _hyperBodyJob.Cancel = true;
                _hyperBodyJob = null;
            }

            int time = skillData.conditions[skillLevel].Time;
            int plusValue = (int)(_creatureObj.Stat.MaxHp * (skillData.conditions[skillLevel].CommonValue * 0.01f));
            ConditionInfo info = new ConditionInfo()
            {
                Time = time
            };

            if (plusValue != 0)
            {
                _hyperMaxHp = plusValue;
                _creatureObj.UpdateHpMpStat();
            }

            // 시간후에 체력으로 되돌림
            _hyperBodyJob = room.PushAfter(time * 1000, () =>
            {
                _hyperMaxHp = 0;
                if (_creatureObj.TotalMaxHp < _creatureObj.Stat.Hp)
                    _creatureObj.Stat.Hp = _creatureObj.TotalMaxHp;

                _creatureObj.UpdateHpMpStat();
                _hyperBodyJob = null;
            });

            ManageBuffOrCondition(skillData, time, info, ConditionType.ConditionBuff, BuffType.BuffHyperbody);
            SendConditionPacket(ConditionType.ConditionBuff, time, skillData.id);
        }
        IJob _ironBodyJob;
        int _ironBodyValue = 0;
        public void IronBody(Skill skillData, int skillLevel)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;
            if (_creatureObj.State == CreatureState.Dead)
                return;

            // 중복되면 이전거는 취소 되고 시간도 초기화하고 진행
            if (_ironBodyJob != null)
            {
                _ironBodyValue = 0;
                _ironBodyJob.Cancel = true;
                _ironBodyJob = null;
            }

            int time = skillData.conditions[skillLevel].Time;
            int plusValue = (int)(_creatureObj.Stat.MaxHp * (skillData.conditions[skillLevel].CommonValue * 0.01f));
            ConditionInfo info = new ConditionInfo()
            {
                Time = time
            };

            if (plusValue != 0)
            {
                _ironBodyValue = plusValue;
                _creatureObj.UpdateClientStat();
            }

            // 시간후에 원래속도 되돌림
            _ironBodyJob = room.PushAfter(time * 1000, () =>
            {
                _ironBodyValue = 0;

                _creatureObj.UpdateClientStat();
                _ironBodyJob = null;
            });

            ManageBuffOrCondition(skillData, time, info, ConditionType.ConditionBuff, BuffType.BuffIronbody);
            SendConditionPacket(ConditionType.ConditionBuff, time, skillData.id);
        }



        public void ManageBuffOrCondition(Skill skillData, int time, ConditionInfo info, ConditionType conditionType = ConditionType.ConditionNone, BuffType buffType = BuffType.BuffNone)
        {
            GameRoom room = _creatureObj.Room;
            if (_creatureObj == null || room == null)
                return;

            // 또 쓰면 기존꺼 삭제하고 갱신
            if (Buffs.ContainsKey(skillData.id))
                Buffs.Remove(skillData.id);

            BuffOrConditionTime buffOrConditionTime = new BuffOrConditionTime();
            {
                buffOrConditionTime.conditionInfo = info;
                buffOrConditionTime.SkillId = skillData.id;
                buffOrConditionTime.ConditionType = conditionType;
                buffOrConditionTime.BuffType = buffType;
                buffOrConditionTime.Cooldown = time;
                buffOrConditionTime.StartTime = Environment.TickCount;
            }
            Buffs.Add(skillData.id, buffOrConditionTime);
            room.PushAfter(time * 1000, () =>
            {
                Buffs.Remove(skillData.id);
            });
        }


        public void SendConditionPacket(ConditionType conditionType, int timeValue, int skillId = 0)
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
            changeCondition.SkillId = skillId;

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
                _creatureObj.State = CreatureState.Idle;
                _stunJob.Cancel = true;
                _stunJob = null;
            }

            _creatureObj.State = CreatureState.Stun;

            // 지속시간이 끝나면 종료
            _stunJob = room.PushAfter(timeValue * 1000, () =>
            {
                _stunJob = null;
                _creatureObj.State = CreatureState.Idle;
            });

            Skill skillData = new Skill();
            skillData.id = 9999;
            ConditionInfo condition = new ConditionInfo();
            condition.Time = timeValue;
            skillData.conditions = new List<ConditionInfo>();
            skillData.conditions.Add(condition);

            ConditionInfo info = new ConditionInfo()
            {
                Time = timeValue
            };

            ManageBuffOrCondition(skillData, timeValue, info, ConditionType.ConditionStun);
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
                _creatureObj.State = CreatureState.Idle;
                _stunJob.Cancel = true;
                _stunJob = null;
            }
            if (_magicGuardJob != null)
            {
                _magicGuardValue = 0;
                _magicGuardJob.Cancel = true;
                _magicGuardJob = null;
            }
            if (_hyperBodyJob != null)
            {
                _hyperMaxHp = 0;
                _hyperBodyJob.Cancel = true;
                _hyperBodyJob = null;
            }
            if (_ironBodyJob != null)
            {
                _ironBodyValue = 0;
                _ironBodyJob.Cancel = true;
                _ironBodyJob = null;
            }


        }


        public int PlayerBuffDamage(int damage)
        {
            if (!(_creatureObj is Player))
                return 0;

            Player player = (Player)_creatureObj;
            int totalDamage = damage;

            if(_magicGuardJob != null && _magicGuardValue != 0)
            {
                int minusDamage = (int)(totalDamage * (_magicGuardValue * 0.01f));
                int minusMp = damage - minusDamage;

                Console.WriteLine($"totalDamage: {damage}, minusDamage: {minusDamage}, minusMp: {minusMp}");

                if(player.Mp >= minusMp && minusDamage != 0)
                {
                    player.UseMp(minusMp);
                    totalDamage -= minusDamage;
                }
            }

            return totalDamage;
        }

        public int BuffDefence()
        {
            int totalDefence = 0;
            totalDefence += _ironBodyValue;

            return totalDefence;
        }
        public int BuffMaxHp()
        {
            int totalMaxHp = 0;
            totalMaxHp += _hyperMaxHp;

            return totalMaxHp;
        }
        public int BuffMaxMp()
        {
            int totalMaxMp = 0;

            return totalMaxMp;
        }


    }
}
