using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutSceneManager : MonoBehaviour
{
    public PlayableDirector director;

    public void StartCutScene()
    {
        director.gameObject.SetActive(true);
        director.Play();
    }

    public void EndCutScene()
    {
        director.Stop();
        director.gameObject.SetActive(false);
    }
}
