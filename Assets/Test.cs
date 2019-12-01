using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    [SerializeField] private AnimationClip clip;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(clip.length








            ); // maybe 0
        foreach (var ce in clip.events)
        {
            Debug.Log(ce.time);
        }
    }
}
