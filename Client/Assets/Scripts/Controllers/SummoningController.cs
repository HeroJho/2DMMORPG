using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummoningController : BaseController
{
	public SummoningController()
	{
		CanCollision = false;
	}

	protected override void Init()
	{
		switch (Dir)
		{
			case MoveDir.Up:
				transform.rotation = Quaternion.Euler(0, 0, 0);
				break;
			case MoveDir.Down:
				transform.rotation = Quaternion.Euler(0, 0, -180);
				break;
			case MoveDir.Left:
				transform.rotation = Quaternion.Euler(0, 0, 90);
				break;
			case MoveDir.Right:
				transform.rotation = Quaternion.Euler(0, 0, -90);
				break;
		}

		base.Init();
	}

	protected override void UpdateAnimation()
	{

	}

	protected override void MoveToNextPos()
	{

	}
}
