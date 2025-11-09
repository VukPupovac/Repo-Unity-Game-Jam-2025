using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portalclose : MonoBehaviour
{
    private Animator animator;
    private float animationLength;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Get the current animation clip length
        AnimationClip currentClip = animator.runtimeAnimatorController.animationClips[0];
        animationLength = currentClip.length;

        // Start coroutine to destroy after animation ends
        StartCoroutine(DestroyAfterDelay(animationLength));
    }

    private System.Collections.IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
