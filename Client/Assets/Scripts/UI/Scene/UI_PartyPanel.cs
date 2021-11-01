using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PartyPanel : UI_Base
{
    [SerializeField]
    private List<UI_Party> _partys;

    public override void Init()
    {
        foreach (UI_Party party in _partys)
        {
            party.ObjInfo = null;
            party.LeaderMark.SetActive(false);
            party.gameObject.SetActive(false);
        }
            
    }

    public void SetPartyInfos()
    {
        foreach (UI_Party party in _partys)
        {
            party.ObjInfo = null;
            party.LeaderMark.SetActive(false);
            party.gameObject.SetActive(false);
        }

        int index = 0;
        foreach (ObjectInfo objInfo in Managers.Communication.Party.PartyList.Values)
        {        
            _partys[index].SetInfo(objInfo);
            if (objInfo.ObjectId == Managers.Communication.Party.LeaderPlayer.ObjectId)
                _partys[index].LeaderMark.SetActive(true);

            _partys[index++].gameObject.SetActive(true);
        }

    }


}
