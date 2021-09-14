using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestView : MonoBehaviour
{
    [SerializeField]
    Text _text;

    public void SetUI(Quest quest)
    {
        if(quest.QuestState == QuestState.Cancomplete)
            _text.color = new Color(0, 255f, 0);
        else
            _text.color = new Color(255f, 255f, 255f);

        switch (quest.QuestType)
        {
            case QuestType.Hunting:
                {
                    HuntingQuest huntingQuest = (HuntingQuest)quest;

                    _text.text =
                        quest.Contents +
                        System.Environment.NewLine +
                        huntingQuest.CurrentNumber + " / " + huntingQuest.PurposeNumber;
                }
                break;
            case QuestType.Collection:
                {
                    CollectionQuest collectionQuest = (CollectionQuest)quest;

                    _text.text =
                        quest.Contents +
                        System.Environment.NewLine +
                        collectionQuest.CurrentNumber + " / " + collectionQuest.PurposeNumber;
                }
                break;
            case QuestType.Complete:
                {
                    CompletingQuest collectionQuest = (CompletingQuest)quest;
                                        
                    _text.text = quest.Contents +
                        System.Environment.NewLine;

                    foreach (int id in collectionQuest.CompleteQuestIds)
                    {
                        QuestData questData = null;
                        Managers.Data.QuestDict.TryGetValue(id, out questData);

                        Quest tempQuest = null;
                        Managers.Quest.CompletedQuests.TryGetValue(id, out tempQuest);

                        if(tempQuest != null)
                            _text.text += "<color=#1DDB16>" + questData.title + "완료하기" + "</color>" +
                            System.Environment.NewLine;
                        else
                            _text.text += questData.title + "완료하기" +
                            System.Environment.NewLine;


                    }
                }
                break;

        }
    }
}
