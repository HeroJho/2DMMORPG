using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectPlayer : UI_Base
{
    enum GameObjects
    {
        ShowingInfoPanel,
        MakingPlayerPanel
    }
    enum Texts
    {
        InfoNameText,
        LevelValueText,
        JobClassText,
        AttackValueText,
        DefenceValueText,
        HpValueText,
        MpValueText,
        GoldValueText
    }
    enum Buttons
    {
        MakingPlayerButton,
        DeletePlayerButton,
        StartingButton,
        MakingButton,
        XButton
    }
    enum InputFields
    {
        NameInput
    }


    [SerializeField]
    private List<UI_SelectPlayer_Item> _items = new List<UI_SelectPlayer_Item>();
    private UI_SelectPlayer_Item _selectedItem;

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
        Bind<InputField>(typeof(InputFields));

        BindEvent();

        Get<GameObject>((int)GameObjects.ShowingInfoPanel).SetActive(false);
        Get<GameObject>((int)GameObjects.MakingPlayerPanel).gameObject.SetActive(false);
    }

    private void BindEvent()
    {
        // 시작 버튼
        BindEvent(Get<Button>((int)Buttons.StartingButton).gameObject, (e) =>
        {
            if (_selectedItem == null)
            {
                (Managers.UI.SceneUI as UI_GameScene).MassageUI.WriteMassage("캐릭터를 선택해 주세요!", false);
                return;
            }

            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = _selectedItem.Info.Name;
            Managers.Network.Send(enterGamePacket);

            (Managers.UI.SceneUI as UI_GameScene).ChangeUI.PadeIns();

        }, Define.UIEvent.LeftClick);

        // 생성 시작 버튼
        BindEvent(Get<Button>((int)Buttons.MakingPlayerButton).gameObject, (e) =>
        {
            // 닉네임 입력창
            Get<GameObject>((int)GameObjects.MakingPlayerPanel).gameObject.SetActive(true);

            ClearSelectPlayer();
            Get<GameObject>((int)GameObjects.ShowingInfoPanel).gameObject.SetActive(false);

        }, Define.UIEvent.LeftClick);

        // 생성 버튼
        BindEvent(Get<Button>((int)Buttons.MakingButton).gameObject, (e) =>
        {
            C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
            string name = Get<InputField>((int)InputFields.NameInput).text;
            if(name == null || name == "")
            {
                (Managers.UI.SceneUI as UI_GameScene).MassageUI.WriteMassage("닉네임을 입력해 주세요.", false);
                return;
            }

            (Managers.UI.SceneUI as UI_GameScene).MassageUI.WriteMassage("생성 중입니다...", true);
            Get<InputField>((int)InputFields.NameInput).text = "";
            Get<GameObject>((int)GameObjects.MakingPlayerPanel).gameObject.SetActive(false);

            createPlayerPacket.Name = name;
            Managers.Network.Send(createPlayerPacket);

        }, Define.UIEvent.LeftClick);

        // 삭제 버튼
        BindEvent(Get<Button>((int)Buttons.DeletePlayerButton).gameObject, (e) =>
        {
            if (_selectedItem == null)
            {
                (Managers.UI.SceneUI as UI_GameScene).MassageUI.WriteMassage("캐릭터를 선택해 주세요!", false);
                return;
            }

            C_DeletePlayer deletePlayerPacket = new C_DeletePlayer();

            deletePlayerPacket.Name = _selectedItem.Info.Name;
            Managers.Network.Send(deletePlayerPacket);

        }, Define.UIEvent.LeftClick);

        // X버튼
        BindEvent(Get<Button>((int)Buttons.XButton).gameObject, (e) =>
        {
            Get<GameObject>((int)GameObjects.MakingPlayerPanel).gameObject.SetActive(false);

        }, Define.UIEvent.LeftClick);

    }

    public void RefreshUI(List<LobbyPlayerInfo> infos)
    {
        if (infos.Count < 0)
            return;

        int count = -1;
        foreach (LobbyPlayerInfo info in infos)
        {
            ++count;
            _items[count].RefreshUI(info);
        }

        if (count == _items.Count - 1)
            return;

        ++count;
        for (int i = count; i < _items.Count; i++)
        {
            _items[i].RefreshUI(null);
        }

    }

    public void SelectPlayer(UI_SelectPlayer_Item aItem)
    {
        if (aItem == _selectedItem)
            return;

        _selectedItem = aItem;

        foreach (UI_SelectPlayer_Item item in _items)
        {
            if (item.Info == null)
                continue;
            item.StopSelectedAnim();
        }


        _selectedItem.PlaySelectedAnim();
        LobbyPlayerInfo info = _selectedItem.Info;

        Get<Text>((int)Texts.InfoNameText).text = "이름: " + info.Name;
        Get<Text>((int)Texts.LevelValueText).text = "레벨: " + info.StatInfo.Level;
        Get<Text>((int)Texts.JobClassText).text = "직업: " + info.StatInfo.JobClassType;
        Get<Text>((int)Texts.AttackValueText).text = "공격력: " + info.StatInfo.Attack;
        Get<Text>((int)Texts.DefenceValueText).text = "방어력: " + info.StatInfo.Defence;
        Get<Text>((int)Texts.HpValueText).text = "체력: " + info.StatInfo.MaxHp;
        Get<Text>((int)Texts.MpValueText).text = "마나: " + info.StatInfo.MaxMp;
        Get<Text>((int)Texts.GoldValueText).text = "골드: " + info.Gold;

        Get<GameObject>((int)GameObjects.ShowingInfoPanel).SetActive(true);

    }

    public void ClearSelectPlayer()
    {
        foreach (UI_SelectPlayer_Item item in _items)
        {
            if (item.Info == null)
                continue;
            item.StopSelectedAnim();
        }

        _selectedItem = null;
    }

}

