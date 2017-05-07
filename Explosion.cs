using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    private Animator animator;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
        if(!AnimatorIsPlaying())
        {
            Destroy(gameObject);
        }

	}

    bool AnimatorIsPlaying()
    {
        //return animator.GetCurrentAnimatorStateInfo(0).length >
        //       animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        //checks if progress of current animation state has reached 100%
        //explosion only has one animation state and only plays once (doesn't loop)
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }
}
