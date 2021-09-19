using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Playables;

public class Test : MonoBehaviour
{
    public PlayableDirector director;
    double second = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            director.gameObject.SetActive(true);
            director.Play();
        }

    }

    public void EndCutScene()
    {
        director.Stop();
        director.gameObject.SetActive(false);
    }
}
