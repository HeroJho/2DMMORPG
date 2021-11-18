using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutSceneManager : MonoBehaviour
{
    SkipDirectorData _skipDirectorData;

    public List<PlayableDirector> Directors;
    private int _playingDirector = -1;
    private bool _isPosCut;

    public void Init()
    {
        _skipDirectorData = Managers.Data.SkipDirectorData;
    }

    public void StartCutScene(int id, bool isPosCut = false)
    {
        _isPosCut = isPosCut;

        if (_playingDirector != -1 && Directors[_playingDirector].gameObject.activeSelf)
            return;

        if (_isPosCut)
        {
            if (_skipDirectorData.skipDirectors.Contains(id))
                return;
        }

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

        if(_isPosCut)
        {
            _skipDirectorData.skipDirectors.Add(_playingDirector);
            Managers.Data.WriteData<SkipDirectorData>(_skipDirectorData, "SkipDirectorData");
        }
    }
}
