using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Party : UI_Base
{
    public ObjectInfo ObjInfo { get; set; } = null;

    [SerializeField]
    private Text _name;
    [SerializeField]
    private Slider _hpBar;
    [SerializeField]
    private Slider _mpBar;
    [SerializeField]
    private Button _quitButton;
    public GameObject LeaderMark;

    public override void Init()
    {

        BindEvent();
    }

    public void SetInfo(ObjectInfo objInfo)
    {
        ObjInfo = objInfo;
        _name.text = objInfo.Name;
        SetHpBar(objInfo.StatInfo.Hp, objInfo.StatInfo.MaxHp);
        SetMpBar(objInfo.StatInfo.Mp, objInfo.StatInfo.MaxMp);
    }

    public void SetHpBar(int hp, int maxHp)
    {
        float ratio = 0.0f;
        if (maxHp > 0)
        {
            ratio = ((float)hp / maxHp);
        }

        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.value = ratio;
    }
    public void SetMpBar(int mp, int maxMp)
    {
        float ratio = 0.0f;
        if (maxMp > 0)
        {
            ratio = ((float)mp / maxMp);
        }

        ratio = Mathf.Clamp(ratio, 0, 1);
        _mpBar.value = ratio;
    }

    void BindEvent()
    {
        BindEvent(_quitButton.gameObject, (e) =>
        {
            if (ObjInfo == null)
                return;

            C_QuitParty quitPartyPacket = new C_QuitParty();
            quitPartyPacket.Id = ObjInfo.ObjectId;
            Managers.Network.Send(quitPartyPacket);

        }, Define.UIEvent.LeftClick);
    }


}
