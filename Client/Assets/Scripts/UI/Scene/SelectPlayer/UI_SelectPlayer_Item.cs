using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectPlayer_Item : UI_Base
{
    enum Animators
    {
        CharAnim,
        SelectedEffectAnim
    }
    enum Texts
    {
        NameText
    }

    LobbyPlayerInfo _info = null;

    public override void Init()
    {
        Bind<Animator>(typeof(Animators));
        Bind<Text>(typeof(Texts));

        BindEvent();
    }
    
    private void BindEvent()
    {
        BindEvent(gameObject, (e) =>
        {
            Get<Animator>((int)Animators.CharAnim).Play("PlayerMove");
            Get<Animator>((int)Animators.SelectedEffectAnim).gameObject.SetActive(true);
            Get<Animator>((int)Animators.SelectedEffectAnim).Play("SelectedAnim");

        }, Define.UIEvent.LeftClick);
    }

    public void RefreshUI(LobbyPlayerInfo info)
    {
        _info = info;
        Get<Text>((int)Texts.NameText).text = info.Name;
        Get<Animator>((int)Animators.CharAnim).Play("PlayerIdle");
        Get<Animator>((int)Animators.SelectedEffectAnim).gameObject.SetActive(false);
        
    }


}
