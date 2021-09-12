using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShortcutKeys : UI_Base
{
    enum ShortcutSlot
    {
        Slot_Q_BG,
        Slot_W_BG,
        Slot_E_BG,
        Slot_R_BG,
        Slot_A_BG,
        Slot_S_BG,
        Slot_D_BG,
        Slot_F_BG
    }


    bool _init = false;
    public override void Init()
    {
        Bind<UI_ShortcutKey>(typeof(ShortcutSlot));

        _init = true;
    }

    public void RefreshUI()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_Q_BG).RefleshCount();
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_W_BG).RefleshCount();
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_E_BG).RefleshCount();
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_R_BG).RefleshCount();
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_A_BG).RefleshCount();
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_S_BG).RefleshCount();
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_D_BG).RefleshCount();
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_F_BG).RefleshCount();
    }


    // 퀵슬롯
    public void GetKeyQ()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_Q_BG).GetKey();
    }
    public void GetKeyW()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_W_BG).GetKey();
    }
    public void GetKeyE()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_E_BG).GetKey();
    }
    public void GetKeyR()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_R_BG).GetKey();
    }
    public void GetKeyA()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_A_BG).GetKey();
    }
    public void GetKeyS()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_S_BG).GetKey();
    }
    public void GetKeyD()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_D_BG).GetKey();
    }
    public void GetKeyF()
    {
        Get<UI_ShortcutKey>((int)ShortcutSlot.Slot_F_BG).GetKey();
    }
}
