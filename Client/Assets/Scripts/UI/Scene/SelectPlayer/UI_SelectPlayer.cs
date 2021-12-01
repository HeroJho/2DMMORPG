using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SelectPlayer : UI_Base
{
    const int MAXCHAR = 6;
    private List<UI_SelectPlayer_Item> _infos = new List<UI_SelectPlayer_Item>();

    public override void Init()
    {
        _infos.Clear();

        GameObject grid = transform.Find("Grid").gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);
    }

    public void RefreshUI(List<LobbyPlayerInfo> infos)
    {
        if (infos.Count < 0)
            return;

        GameObject grid = transform.Find("Grid").gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        foreach (LobbyPlayerInfo info in infos)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/SelectPlayer/UI_SelectPlayer_Item", grid.transform);
            UI_SelectPlayer_Item infoUI = go.GetComponent<UI_SelectPlayer_Item>();
            _infos.Add(infoUI);
            infoUI.RefreshUI(info);
        }

    }

}
