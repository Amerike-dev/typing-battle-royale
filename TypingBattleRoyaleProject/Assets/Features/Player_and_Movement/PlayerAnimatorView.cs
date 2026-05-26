using UnityEngine;

public class PlayerAnimatorView : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;

    private static readonly int _horizontal = Animator.StringToHash("Horizontal");
    private static readonly int _vertical = Animator.StringToHash("Vertical");
    private static readonly int _OnGround = Animator.StringToHash("OnGround");
    private static readonly int _cast = Animator.StringToHash("Cast");
    private static readonly int _interact = Animator.StringToHash("Interact");
    private static readonly int _jump = Animator.StringToHash("Jump");
    private static readonly int _takeDamage = Animator.StringToHash("TakeDamage");
    private static readonly int _death = Animator.StringToHash("Death");

    private void Awake()
    {
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
    }

    public void SetMovement(float horizontal, float vertical)
    {
        if (playerAnimator == null) return;
        if (HasParameter(_horizontal, AnimatorControllerParameterType.Float)) playerAnimator.SetFloat(_horizontal, horizontal);
        if (HasParameter(_vertical, AnimatorControllerParameterType.Float)) playerAnimator.SetFloat(_vertical, vertical);
    }

    public void SetOnGround(bool isOnGround)
    {
        if (playerAnimator == null) return;
        if (HasParameter(_OnGround, AnimatorControllerParameterType.Bool))
            playerAnimator.SetBool(_OnGround, isOnGround);
    }

    public void SetCast(bool isCasting)
    {
        if (playerAnimator == null) return;
        if (HasParameter(_cast, AnimatorControllerParameterType.Bool))
            playerAnimator.SetBool(_cast, isCasting);
    }

    public void SetInteract(bool isInteracting)
    {
        if (playerAnimator == null) return;
        if (HasParameter(_interact, AnimatorControllerParameterType.Bool))
            playerAnimator.SetBool(_interact, isInteracting);
    }

    public void TriggerJump()
    {
        if (playerAnimator == null) return;
        if (HasParameter(_jump, AnimatorControllerParameterType.Trigger))
            playerAnimator.SetTrigger(_jump);
    }

    public void TriggerTakeDamage()
    {
        if (playerAnimator == null) return;
        if (HasParameter(_takeDamage, AnimatorControllerParameterType.Trigger))
            playerAnimator.SetTrigger(_takeDamage);
    }

    public void TriggerDeath()
    {
        if (playerAnimator == null) return;
        if (HasParameter(_death, AnimatorControllerParameterType.Trigger))
            playerAnimator.SetTrigger(_death);
    }
    private bool HasParameter(int nameHash, AnimatorControllerParameterType type)
    {
        var parameters = playerAnimator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            if (p.nameHash == nameHash && p.type == type) return true;
        }
        return false;
    }
}