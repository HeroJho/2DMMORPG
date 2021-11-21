using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShopPanel : UI_Base
{
    enum Texts
    {
        TalkText,
        ShoperNameText,
        PlayerNameText,
        MyGoldText
    }

    enum Transforms
    {
        ShoperGrid,
        PlayerGrid
    }

    enum Buttons
    {
        XButton
    }

    bool _isInit = false;
    public override void Init()
    {
        if (_isInit)
            return;

        Bind<Text>(typeof(Texts));
        Bind<Transform>(typeof(Transforms));
        Bind<Button>(typeof(Buttons));

        BindEvent();
        _isInit = true;
    }

    private void BindEvent()
    {
        BindEvent(Get<Button>((int)Buttons.XButton).gameObject, (e) =>
        {
            UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
            gameScene.ShopPanelUI.gameObject.SetActive(false);

        }, Define.UIEvent.LeftClick);

    }

    public void RefreshUI(int npcId)
    {
        Init();

        // 구매하거나 아이템 사용시 아이템 갯수, 골드, UI 초기화
        SetShoperGrid(npcId);
        SetPlayerGrid();

    }

    public void SetShoperGrid(int npcId)
    {
        Init();

        Transform grid = Get<Transform>((int)Transforms.ShoperGrid);
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        // Npc 정보
        NpcData npcData = null;
        Managers.Data.NpcDict.TryGetValue(npcId, out npcData);
        Get<Text>((int)Texts.ShoperNameText).text = npcData.name;
        Get<Text>((int)Texts.TalkText).text = npcData.Description;

        // Npc의 판매 아이템 데이터를 뽑는다.
        ShoperData shoperData = null;
        Managers.Data.ShoperDict.TryGetValue(npcId, out shoperData);

        List<ItemData> itemDatas = new List<ItemData>();
        foreach (int id in shoperData.itemIds)
        {
            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(id, out itemData);
            itemDatas.Add(itemData);
        }

        // 아이템 판넬을 추가한다.
        foreach (ItemData data in itemDatas)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/Shop/UI_Shop_Shoper_Item", grid.transform);
            UI_Shop_Shoper_Item itemUI = go.GetComponent<UI_Shop_Shoper_Item>();

            itemUI.SetInfo(data);
        }

    }
    public void SetPlayerGrid()
    {
        Init();

        Transform grid = Get<Transform>((int)Transforms.PlayerGrid);
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        // Player 정보
        MyPlayerController mPc = Managers.Object.MyPlayer;
        Get<Text>((int)Texts.PlayerNameText).text = mPc.name;
        Get<Text>((int)Texts.MyGoldText).text = "골드: " + Managers.Inven.Gold;

        // Player 인벤 데이터를 뽑는다.
        // 아이템 판넬을 추가한다.
        foreach (Item info in Managers.Inven.Items.Values)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/Shop/UI_Shop_Player_Item", grid.transform);
            UI_Shop_Player_Item itemUI = go.GetComponent<UI_Shop_Player_Item>();

            itemUI.SetInfo(info);
        }

    }

}
