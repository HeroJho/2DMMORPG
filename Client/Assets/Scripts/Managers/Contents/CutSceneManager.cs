using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutSceneManager : MonoBehaviour
{
    public List<PlayableDirector> Directors;
    private int _playingDirector;

    public void StartCutScene(int id)
    {
        if (Directors[_playingDirector].gameObject.activeSelf)
            return;

        Managers.Object.MyPlayer.State = CreatureState.Cutscene;
        Directors[id].gameObject.SetActive(true);
        Directors[id].Play();
        _playingDirector = id;
    }

    public void EndCutScene()
    {
        Managers.Object.MyPlayer.State = CreatureState.Idle;
        Directors[_playingDirector].Stop();
        Directors[_playingDirector].gameObject.SetActive(false);
    }
}
