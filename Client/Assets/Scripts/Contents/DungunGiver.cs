using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungunGiver : MonoBehaviour
{
    private int _npcId;


    bool _isInit = false;
    public void Init()
    {
        _isInit = true;

        _npcId = GetComponent<QuestGiver>().NpcId;
    }

    public void GetPanelInfo()
    {
        if (!_isInit)
            Init();

        DungunData dungunData = null;
        Managers.Data.DungunDict.TryGetValue(_npcId, out dungunData);

        (Managers.UI.SceneUI as UI_GameScene).DungunUI.OnPanel(dungunData);
    }
}
