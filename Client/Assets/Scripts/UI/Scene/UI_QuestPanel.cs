using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestPanel : UI_Base
{

    [SerializeField]
    GameObject _grid;

    public override void Init()
    {
        foreach (Transform child in _grid.transform)
            Destroy(child.gameObject);
    }

    public void RefreshUI()
    {
        foreach (Transform child in _grid.transform)
            Destroy(child.gameObject);


        foreach (Quest questInfo in Managers.Quest.Quests.Values)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_QuestView", _grid.transform);
            UI_QuestView questViewSlot = go.GetComponent<UI_QuestView>();

            questViewSlot.SetUI(questInfo);
        }
       
    }
}
