using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    enum Images
    {
        DropPanel
    }

    public UI_Stat StatUI { get; private set; }
    public UI_Inventory InvenUI { get; private set; }
    public UI_StatBar StatBarUI { get; private set; }
    public UI_ShortcutKeys ShortcutKeyUI { get; private set; }
    public UI_Skill SkillUI { get; private set; }
    public UI_QuestPanel QuestUI { get; private set; }
    public UI_BuffPanel BuffUI { get; private set; }
    public UI_InteractionPanel InteractionUI { get; private set; }
    public UI_PartyPanel PartyPanelUI { get; private set; }
    public UI_ChatInputBox ChatInputBoxUI { get; private set; }
    public UI_BossHp BossHpUI { get; private set; }
    public UI_DungunPanel DungunUI { get; private set; }

    public UI_CanClassUp ClassUp { get; private set; }
    public UI_DescriptionBox DescriptionBox { get; private set; }

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        BindEvent();

        StatUI = GetComponentInChildren<UI_Stat>();
        InvenUI = GetComponentInChildren<UI_Inventory>();
        StatBarUI = GetComponentInChildren<UI_StatBar>();
        ShortcutKeyUI = GetComponentInChildren<UI_ShortcutKeys>();
        SkillUI = GetComponentInChildren<UI_Skill>();
        QuestUI = GetComponentInChildren<UI_QuestPanel>();
        BuffUI = GetComponentInChildren<UI_BuffPanel>();
        InteractionUI = GetComponentInChildren<UI_InteractionPanel>();
        PartyPanelUI = GetComponentInChildren<UI_PartyPanel>();
        ChatInputBoxUI = GetComponentInChildren<UI_ChatInputBox>();
        BossHpUI = GetComponentInChildren<UI_BossHp>();
        DungunUI = GetComponentInChildren<UI_DungunPanel>();

        ClassUp = GetComponentInChildren<UI_CanClassUp>();
        DescriptionBox = GetComponentInChildren<UI_DescriptionBox>();

        StatUI.gameObject.SetActive(false);
        InvenUI.gameObject.SetActive(false);
        SkillUI.gameObject.SetActive(false);
        ClassUp.gameObject.SetActive(false);
        InteractionUI.gameObject.SetActive(false);
        BossHpUI.gameObject.SetActive(false);
        DungunUI.gameObject.SetActive(false);
    }

    void BindEvent()
    {
        // 바닥에 퀵슬롯 뿌리면 큇슬롯 비워줌
        BindEvent(Get<Image>((int)Images.DropPanel).gameObject, (e) =>
        {
            UI_ShortcutKey keyInfo = e.pointerDrag.GetComponentInParent<UI_ShortcutKey>();

            if (keyInfo != null)
                keyInfo.RemoveSlot();

        }, Define.UIEvent.Drop);

        BindEvent(Get<Image>((int)Images.DropPanel).gameObject, (e) =>
        {
            UI_Inventory_Item itemInfo = e.pointerDrag.GetComponentInParent<UI_Inventory_Item>();

            if (itemInfo != null)
                Managers.Inven.TryToRemoveItem(itemInfo.ItemDbId);

        }, Define.UIEvent.Drop);
    }

}
