using UnityEngine;

public class PlayerAnimatorView : MonoBehaviour
{
    public Animator playerAnimator;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int CastingHash = Animator.StringToHash("Casting");

    public void SetPlayerMovement(bool isMoving = false, float speed = 0)
    {
        if (playerAnimator == null) return;
        if (HasParameter(IsMovingHash, AnimatorControllerParameterType.Bool))
            playerAnimator.SetBool(IsMovingHash, isMoving);
        if (HasParameter(SpeedHash, AnimatorControllerParameterType.Float))
            playerAnimator.SetFloat(SpeedHash, speed);
    }

    public void TriggerCasting()
    {
        if (playerAnimator == null) return;
        if (HasParameter(CastingHash, AnimatorControllerParameterType.Trigger))
            playerAnimator.SetTrigger(CastingHash);
    }

    public void StopCasting()
    {
        if (playerAnimator == null) return;
        if (HasParameter(CastingHash, AnimatorControllerParameterType.Trigger))
            playerAnimator.ResetTrigger(CastingHash);
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
