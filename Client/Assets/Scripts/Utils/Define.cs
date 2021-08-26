using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
    }

    public enum UIEvent
    {
        RightClick,
        LeftClick,
        Click_Up,
        Drag,
        DragEnd,
        Drop
    }
}
