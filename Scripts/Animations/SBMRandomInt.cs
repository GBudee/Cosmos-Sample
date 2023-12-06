using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBMRandomInt : StateMachineBehaviour
{
    [Tooltip("The parameter name that will be set to random integer value. You must add this into your animator's parameter list as an Integer.")]
    public string parameterName = "RandomInt";

    [Tooltip("Minimum generated random value.")]
    public int minValue = 0; 

    [Tooltip("Maximum generated random value.")]
    public int maxValue = 1; 

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        int value = Random.Range(minValue,maxValue+1);
        animator.SetInteger(parameterName,value);
   }

}