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

        }
    }
}
