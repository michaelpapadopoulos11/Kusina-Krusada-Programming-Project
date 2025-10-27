using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Movement))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Movement movementScript;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponent<Movement>();
    }

    void Update()
    {
        animator.SetBool("isGrounded", movementScript.IsGrounded);
        animator.SetBool("isCrouching", movementScript.IsCrouching);

        if (movementScript.SwipeUp)
        {
            animator.SetTrigger("Jump");
        }
    }
}