using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager
{

    public void SendChatToServer()
    {
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        string str = gameSceneUI.ChatInputBoxUI.GetChat();

        if (str.Length <= 0)
            return;

        C_Chat chatPacket = new C_Chat();
        chatPacket.Str = str;
        Managers.Network.Send(chatPacket);
    }

    public void SendChatToBox(int id, string str)
    {
        GameObject obj = Managers.Object.FindById(id);
        if (obj == null)
            return;
        CreatureController cc = obj.GetComponent<CreatureController>();
        if (cc == null)
            return;

        cc.ChatBox.Write(str);
    }

}
