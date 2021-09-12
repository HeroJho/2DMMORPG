using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RewardSlot : UI_Base
{
    [SerializeField]
    Image _icon;

    [SerializeField]
    Text _countText;

    public override void Init()
    {
        
    }

    public void SetUI(QuestRewardData rewardData)
    {
        _countText.gameObject.SetActive(false);

        if (rewardData.itemId != 0)
        {
            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(rewardData.itemId, out itemData);

            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            _icon.sprite = icon;

            if (rewardData.count > 0)
            {
                _countText.text = "X" + rewardData.count;
                _countText.gameObject.SetActive(true);
            }
        }
        else if(rewardData.exp != 0)
        {
            Sprite icon = Managers.Resource.Load<Sprite>("Textures/ExpUI");
            _icon.sprite = icon;
            _countText.text = "+" + rewardData.exp;
            _countText.gameObject.SetActive(true);
        }


    }
}
