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
            //��ȡ����Ϊbase layer�Ĳ㼶 ��ͨ���㼶ȥ��ȡanimation state info
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
        //���ҽ���0��0��0��λ�ÿ�£
        animator.SetIKPosition(AvatarIKGoal.RightFoot, Vector3.zero);
        //����Ȩ�� 1Ϊ���� 
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightRootWeight);
    }
}
