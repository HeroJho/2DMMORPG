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
        if (PartyList.ContainsKey(objInfo.ObjectId))
        {
            PartyList[objInfo.ObjectId] = objInfo;
            return;
        }

        PartyList.Add(objInfo.ObjectId, objInfo);
    }

    public void RemovePlayer(int id)
    {
        PartyList.Remove(id);
    }

}
