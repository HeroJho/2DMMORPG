using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BuffPair
{
    public UI_Buff Buff;
    public Coroutine Co;

    public BuffPair(UI_Buff buff, Coroutine co)
    {
        Buff = buff;
        Co = co;
    }
}

public class UI_BuffPanel : UI_Base
{
    Dictionary<int, BuffPair> _buff = new Dictionary<int, BuffPair>();

    public override void Init()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    public void AddBuff(int skillId, int timeValue)
    {
        if (_buff.ContainsKey(skillId))
            RemoveBuff(skillId);
        Skill skillData = null;
        Managers.Data.SkillDict.TryGetValue(skillId, out skillData);

        GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Buff", transform);
        UI_Buff buff = go.GetComponent<UI_Buff>();
        buff.Image.sprite = Managers.Resource.Load<Sprite>(skillData.iconPath);
        Coroutine co = StartCoroutine(StartBuffCo(buff, timeValue, skillId));

        BuffPair buffPair = new BuffPair(buff, co);
        _buff.Add(skillId, buffPair);

    }

    public void RemoveBuff(int skillId)
    {
        BuffPair buffPair;

        if (_buff.TryGetValue(skillId, out buffPair) == false)
            return;

        StopCoroutine(buffPair.Co);
        Destroy(buffPair.Buff.gameObject);

        _buff.Remove(skillId);
    }

    IEnumerator StartBuffCo(UI_Buff buff, int timeValue, int skillId)
    {
        float currentTime = 0;
        while (timeValue > currentTime)
        {
            currentTime += 0.1f;
            yield return new WaitForSeconds(0.1f);

            float ratio = 0.0f;
            ratio = ((float)currentTime / timeValue);

            buff.SetRatio(ratio);
        }

        RemoveBuff(skillId);
    }



}
