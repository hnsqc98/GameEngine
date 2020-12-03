using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyCharacter;

public class UpperEmptyState : StateMachineBehaviour
{
    CharacterAnimator charAnim;
    CharacterAnimator GetCharacter(Animator animator)
    {
        if (charAnim == null)
        {
            charAnim = animator.GetComponentInParent<CharacterAnimator>();
        }
        return charAnim;
    }
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GetCharacter(animator);
        charAnim.SetUpperBodyWeight(0.0f);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GetCharacter(animator);
        charAnim.SetUpperBodyWeight(1.0f);
    }
}
