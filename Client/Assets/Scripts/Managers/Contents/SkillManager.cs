using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager
{
    private bool _canUseSKill_01 = true;
    private bool _canUseSKill_02 = true;
    private bool _canUseSKill_03 = true;

    public bool UseSkill(int templateId)
    {
        // 단순히 이동상태일 때 스킬을 못쓰는게 아니라
        // Skill을 쓰는 도중에 중첩해서 Skill을 쓰는걸 방지
        // State가 Skill일 때도 Skill사용 금지
        if (Managers.Object.MyPlayer.State != CreatureState.Idle)
            return false;

        switch (templateId)
        {
            case 1:
                {
                    if (!_canUseSKill_01)
                        return false;
                                            
                    _canUseSKill_01 = false;
                }
                break;
            case 2:
                {
                    if (!_canUseSKill_02)
                        return false;
                    
                    _canUseSKill_02 = false;
                }
                break;
            case 3:
                {
                    if (!_canUseSKill_03)
                        return false;
                    
                    _canUseSKill_03 = false;
                }
                break;

        }

        C_Skill skill = new C_Skill() { Info = new SkillInfo() };
        skill.Info.SkillId = templateId;
        Managers.Network.Send(skill);

        return true;
    }

    public void ResetCooltime(int templateId)
    {
        switch (templateId)
        {
            case 1:
                {
                    _canUseSKill_01 = true;
                    Debug.Log("CanUse 01");
                }
                break;
            case 2:
                {
                    _canUseSKill_02 = true;
                    Debug.Log("CanUse 02");
                }
                break;
            case 3:
                {
                    _canUseSKill_03 = true;
                    Debug.Log("CanUse 03");
                }
                break;

        }
    }
}
