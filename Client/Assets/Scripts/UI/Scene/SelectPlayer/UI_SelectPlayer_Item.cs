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

    public LobbyPlayerInfo Info { get; private set; } = null;

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
            // 부모에서 애니메이션, 전송 데이터 관리
            (Managers.UI.SceneUI as UI_GameScene).SelectUI.SelectPlayer(this);

        }, Define.UIEvent.LeftClick);
    }

    public void RefreshUI(LobbyPlayerInfo info)
    {
        Info = info;

        if(Info == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            Get<Text>((int)Texts.NameText).text = info.Name;
            Get<Animator>((int)Animators.CharAnim).Play("PlayerIdle");
            Get<Animator>((int)Animators.SelectedEffectAnim).gameObject.SetActive(false);
        }

    }

    public void PlaySelectedAnim()
    {
        Get<Animator>((int)Animators.CharAnim).Play("PlayerMove");
        Get<Animator>((int)Animators.SelectedEffectAnim).gameObject.SetActive(true);
        Get<Animator>((int)Animators.SelectedEffectAnim).Play("SelectedAnim");
    }

    public void StopSelectedAnim()
    {
        Get<Animator>((int)Animators.CharAnim).Play("PlayerIdle");
        Get<Animator>((int)Animators.SelectedEffectAnim).gameObject.SetActive(false);
    }

}
