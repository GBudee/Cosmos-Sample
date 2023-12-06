using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBMRandomSpeed : StateMachineBehaviour
{
    [Tooltip("Minimum generated random value.")]
    public float minValue = .5F; 

    [Tooltip("Maximum generated random value.")]
    public float maxValue = 1.5F; 

    // when entering a state machine, choose a random value
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        float value = Random.Range(minValue,maxValue);
        animator.SetFloat("Speed", value);
   }

}