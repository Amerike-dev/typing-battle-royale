using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimatorView : MonoBehaviour
{
    public Animator playerAnimator;

    public void SetPlayerMovement(bool isMoving = false, float speed = 0)
    {
        playerAnimator.SetBool("IsMoving", isMoving);
        playerAnimator.SetFloat("Speed", speed);
    }
}
