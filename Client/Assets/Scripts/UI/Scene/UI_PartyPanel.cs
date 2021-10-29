using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PartyPanel : UI_Base
{
    [SerializeField]
    private List<UI_Party> _partys;


    List<PlayerController> _pcs = new List<PlayerController>();

    public override void Init()
    {

    }

    public void SetPartyInfos(List<ObjectInfo> ids)
    {
        foreach (UI_Party party in _partys)
        {

        }


    }


}
