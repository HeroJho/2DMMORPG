using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Quest : UI_Popup
{
    enum Texts
    {
        TitleText,
        ContentsText,
        ButtonText
    }

    enum Buttons
    {
        CloseButton,
        Button
    }

    [SerializeField]
    GameObject _grid;
    Quest _questInfo = null;

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        foreach (Transform child in _grid.transform)
            Destroy(child.gameObject);

        BindEvent();
    }

    void BindEvent()
    {
        // X 버튼
        BindEvent(Get<Button>((int)Buttons.CloseButton).gameObject, (e) =>
        {
            Managers.UI.ClosePopupUI(this);

        }, Define.UIEvent.LeftClick);

        // 수락 버튼
        BindEvent(Get<Button>((int)Buttons.Button).gameObject, (e) =>
        {
            if (_questInfo == null)
                return;

            if (_questInfo.QuestState == QuestState.Canaccapt)
                Managers.Quest.TryReceiveQuest(_questInfo);
            else if (_questInfo.QuestState == QuestState.Cancomplete)
                Managers.Quest.TryCompleteQuest(_questInfo);

            Managers.UI.ClosePopupUI(this);

        });

    }

    public void RefreshUI(Quest questInfo)
    {
        // 퀘스트 상태에 따른 UI상태 변화
        switch (questInfo.QuestState)
        {
            case QuestState.Canaccapt:
                {
                    Get<Button>((int)Buttons.Button).gameObject.SetActive(true);
                    Get<Text>((int)Texts.ButtonText).text = "Accept";

                    // 퀘스트 표현
                    RewardUI(questInfo);

                    Description(questInfo);
                }
                break;
            case QuestState.Proceed:
                {
                    Get<Button>((int)Buttons.Button).gameObject.SetActive(false);

                    // 퀘스트 표현
                    RewardUI(questInfo);

                    Description(questInfo);
                }
                break;
            case QuestState.Cancomplete:
                {
                    Get<Button>((int)Buttons.Button).gameObject.SetActive(true);
                    Get<Text>((int)Texts.ButtonText).text = "Complete";

                    // 퀘스트 표현
                    RewardUI(questInfo);

                    Description(questInfo);
                }
                break;

        }

        _questInfo = questInfo;
    }

    public void Description(Quest questInfo)
    {
        switch (questInfo.QuestType)
        {
            case QuestType.Hunting:
                {
                    HuntingQuest huntingQuest = (HuntingQuest)questInfo;

                    Get<Text>((int)Texts.TitleText).text = questInfo.Title;
                    Get<Text>((int)Texts.ContentsText).text =
                        questInfo.Description +
                        System.Environment.NewLine +
                        System.Environment.NewLine +
                        questInfo.Contents + " " + +huntingQuest.PurposeNumber + " 마리";

                }
                break;
            case QuestType.Collection:
                {
                    CollectionQuest collectionQuest = (CollectionQuest)questInfo;

                    Get<Text>((int)Texts.TitleText).text = questInfo.Title;
                    Get<Text>((int)Texts.ContentsText).text =
                        questInfo.Description +
                        System.Environment.NewLine +
                        System.Environment.NewLine +
                        questInfo.Contents + " " + +collectionQuest.PurposeNumber + " 개";

                }
                break;

        }
    }

    public void RewardUI(Quest questInfo)
    {
        foreach(QuestRewardData rewardData in questInfo.Rewards)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Popup/UI_RewardSlot", _grid.transform);
            UI_RewardSlot rewardSlot = go.GetComponent<UI_RewardSlot>();

            rewardSlot.SetUI(rewardData);
        }
    }

}
