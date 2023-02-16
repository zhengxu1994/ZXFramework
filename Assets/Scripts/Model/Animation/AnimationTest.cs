using System.Collections;
using System.Collections.Generic;
using Bepop.Core;
using UnityEngine;

/*
 * Animation可以通过Animation.Play("ClipName")去播放某个动画
 * 
 */


public class AnimationTest : MonoBehaviour
{
    private Animation animation;
    // Start is called before the first frame update
    void Start()
    {
        animation = this.GetComponent<Animation>();
        AnimationState state1 = null;
        foreach (AnimationState state in animation)
        {
            Log.BASE.LogInfo(state.name);
            Log.BASE.LogInfo(state.clip);
            if (state1 == null)
                state1 = state;
        }
        var clip = animation.GetClip("AnimationTest 1");
        animation.Play("AnimationTest 1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
