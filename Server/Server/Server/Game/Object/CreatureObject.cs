using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class CreatureObject : GameObject
    {
		public StatInfo Stat { get; private set; } = new StatInfo();
		public Condition Condition { get; private set; }

		public virtual int TotalAttack { get { return Stat.Attack; } }
		public virtual int TotalDefence { get { return Stat.Defence + Condition.BuffDefence(); } }
		public virtual int TotalMaxHp { get { return Stat.MaxHp + Condition.BuffMaxHp(); } }

		public float Speed
		{
			get { return Stat.Speed; }
			set { Stat.Speed = value; }
		}

		public virtual int Hp
		{
			get { return Stat.Hp; }
			set { Stat.Hp = Math.Clamp(value, 0, TotalMaxHp); }
		}

		public MoveDir Dir
		{
			get { return PosInfo.MoveDir; }
			set { PosInfo.MoveDir = value; }
		}

		public CreatureState State
		{
			get { return PosInfo.State; }
			set { PosInfo.State = value; }
		}

		public CreatureObject()
		{
			Info.StatInfo = Stat;
			Condition = new Condition(this);
		}

		public MoveDir GetDirFromVec(Vector2Int dir)
		{
			if (dir.x > 0)
				return MoveDir.Right;
			else if (dir.x < 0)
				return MoveDir.Left;
			else if (dir.y > 0)
				return MoveDir.Up;
			else
				return MoveDir.Down;
		}

		public Vector2Int GetFrontCellPos()
		{
			return GetFrontCellPos(PosInfo.MoveDir);
		}
		public Vector2Int GetFrontCellPos(MoveDir dir)
		{
			Vector2Int cellPos = CellPos;

			switch (dir)
			{
				case MoveDir.Up:
					cellPos += Vector2Int.up;
					break;
				case MoveDir.Down:
					cellPos += Vector2Int.down;
					break;
				case MoveDir.Left:
					cellPos += Vector2Int.left;
					break;
				case MoveDir.Right:
					cellPos += Vector2Int.right;
					break;
			}

			return cellPos;
		}
		public Vector2Int GetBackCellPos()
		{
			return GetBackCellPos(PosInfo.MoveDir);
		}

		public Vector2Int GetBackCellPos(MoveDir dir)
		{
			Vector2Int cellPos = CellPos;

			switch (dir)
			{
				case MoveDir.Up:
					cellPos -= Vector2Int.up;
					break;
				case MoveDir.Down:
					cellPos -= Vector2Int.down;
					break;
				case MoveDir.Left:
					cellPos -= Vector2Int.left;
					break;
				case MoveDir.Right:
					cellPos -= Vector2Int.right;
					break;
			}

			return cellPos;
		}


		public virtual void Update()
		{

		}

		public virtual void OnDamaged(GameObject attacker, int damage, bool trueDamage = false)
		{
			if (Room == null)
				return;

            // 데미지 보정
            if (trueDamage == false)
                damage = Math.Max(damage - TotalDefence, 0);

            Hp -= damage;

			UpdateHpMpStat();

			if (Hp <= 0)
			{
				OnDead(attacker);
			}
		}

		public virtual void OnDead(GameObject attacker)
		{
			if (Room == null)
				return;

			Condition.BackCondition();

			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			diePacket.AttackerId = attacker.Id;
			Room.Broadcast(CellPos, diePacket);

			GameRoom room = Room;
			room.LeaveGame(Id);

			Stat.Hp = Stat.MaxHp;
			PosInfo.State = CreatureState.Idle;
			PosInfo.MoveDir = MoveDir.Down;
			PosInfo.PosX = 0;
			PosInfo.PosY = 0;

			room.EnterGame(this);
		}

		public virtual void RecoveryHp(int recovery)
		{
			if (Room == null)
				return;

			Hp += recovery;

			UpdateHpMpStat();
		}

		public virtual void UpdateClientStat()
		{

		}

		public virtual void UpdateHpMpStat()
		{
			// 몬스터의 경우는 MP가 없다
			S_ChangeHp changeHp = new S_ChangeHp();
			changeHp.ObjectId = Id;
			changeHp.Hp = Hp;
			changeHp.MaxHp = TotalMaxHp;

			Room.Broadcast(CellPos, changeHp);
		}
	}
}
