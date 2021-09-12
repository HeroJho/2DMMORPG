using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : BaseController
{
    public ItemInfo itemInfo { get; set; }

	public ItemController()
	{
		CanCollision = false;
	}
}
