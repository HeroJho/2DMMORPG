using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager
{
    private Dictionary<int, Skill> _coolTimeList = new Dictionary<int, Skill>();
    public Dictionary<int, int> SkillPointInfos { get; private set; } = new Dictionary<int, int>();
    public int MyPoints { get; set; }

    public bool UseSkill(int templateId)
    {
        // 단순히 이동상태일 때 스킬을 못쓰는게 아니라
        // Skill을 쓰는 도중에 중첩해서 Skill을 쓰는걸 방지
        // State가 Skill일 때도 Skill사용 금지
        if (Managers.Object.MyPlayer.State != CreatureState.Idle)
            return false;

        if (_coolTimeList.ContainsKey(templateId))
            return false;

        int point = 0;
        if (!SkillPointInfos.TryGetValue(templateId, out point))
            return false;

        // 마나 확인
        Skill skillData = null;
        Managers.Data.SkillDict.TryGetValue(templateId, out skillData);
        if (Managers.Object.MyPlayer.Mp < skillData.skillPointInfos[point - 1].mp)
            return false;

        _coolTimeList.Add(templateId, skillData);

        C_Skill skill = new C_Skill() { Info = new SkillInfo() };
        skill.Info.SkillId = templateId;
        Managers.Network.Send(skill);

        return true;
    }

    public void ResetCooltime(int templateId)
    {
        _coolTimeList.Remove(templateId);
    }

    public void RefreshSkillPointInfo(int skillId, int point)
    {
        if (SkillPointInfos.ContainsKey(skillId))
            SkillPointInfos[skillId] = point;
        else
            SkillPointInfos.Add(skillId, point);
       
    }

    public int GetSkillPoint(int skillId)
    {
        return SkillPointInfos[skillId] - 1;
    }
}
