using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaughtyCharacter
{
    public class StateIdle : StateMachineBehaviour
    {
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetInteger(CharacterAnimatorParamId.IdlePose, 0);
        }
    }
}

