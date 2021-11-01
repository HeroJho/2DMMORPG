using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party
{
    public ObjectInfo LeaderPlayer { get; set; }
    public Dictionary<int, ObjectInfo> PartyList { get; private set; } = new Dictionary<int, ObjectInfo>();

    public void AddPlayer(ObjectInfo objInfo)
    {
        PartyList.Add(objInfo.ObjectId, objInfo);
    }

}
