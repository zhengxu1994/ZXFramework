using Bepop.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimatorStateTest : MonoBehaviour
{
    private Animator animator;
    private AnimatorStateInfo stateInfo;
    private float speed = 0;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetFloat("PlaySpeed", speed);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            //获取名称为base layer的层级 并通过层级去获取animation state info
            stateInfo = animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Layer"));
            speed += 1;
            animator.SetFloat("PlaySpeed", speed);
            if (stateInfo.IsTag("TagTest"))
            {
                Log.BASE.LogInfo("Tag is Trigger");
                return;
            }
        }
    }
    public float rightRootWeight;

    private void OnAnimatorIK(int layerIndex)
    {
        //将右脚像0，0，0的位置靠拢
        animator.SetIKPosition(AvatarIKGoal.RightFoot, Vector3.zero);
        //设置权重 1为做大 
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightRootWeight);
    }
}
