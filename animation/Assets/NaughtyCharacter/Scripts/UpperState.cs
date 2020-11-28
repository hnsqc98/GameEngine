using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyCharacter;

public class UpperState : StateMachineBehaviour
{
    Character character;
    Character GetCharacter(Animator animator)
    {
        if (character == null)
        {
            character = animator.GetComponentInParent<Character>();
        }
        return character;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GetCharacter(animator);
        if (character)
        {
            character.UpperState = 0;
        }
    }
}
