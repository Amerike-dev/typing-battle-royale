using System.Collections;
using UnityEngine;
using Unity.Netcode.Components;

/// <summary>
/// Capa de presentación entre la lógica del jugador y el Animator del modelo.
/// Expone los parámetros reales que tienen los controllers de cada personaje
/// (Berry/Ixia/Klug/Wander): Horizontal, Vertical, Jump, Interact, Cast, OnGround,
/// TakeDamage y Death.
///
/// Si hay un NetworkAnimator en el mismo objeto, los triggers se disparan a través
/// de él para que se repliquen a los demás clientes. Los floats y bools los sincroniza
/// el propio NetworkAnimator de forma automática desde la instancia con autoridad.
/// </summary>
public class PlayerAnimatorView : MonoBehaviour
{
    public Animator playerAnimator;
    public NetworkAnimator networkAnimator;

    [Tooltip("Segundos que el bool Interact permanece activo para reproducir el gesto de interacción una sola vez.")]
    [SerializeField] private float interactGestureDuration = 1f;

    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private static readonly int VerticalHash = Animator.StringToHash("Vertical");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int InteractHash = Animator.StringToHash("Interact");
    private static readonly int CastHash = Animator.StringToHash("Cast");
    private static readonly int OnGroundHash = Animator.StringToHash("OnGround");
    private static readonly int TakeDamageHash = Animator.StringToHash("TakeDamage");
    private static readonly int DeathHash = Animator.StringToHash("Death");

    private Coroutine _interactRoutine;

    private void Awake()
    {
        if (playerAnimator == null) playerAnimator = GetComponentInChildren<Animator>(true);
        if (networkAnimator == null) networkAnimator = GetComponentInChildren<NetworkAnimator>(true);
    }

    /// <summary>Actualiza los parámetros de locomoción (A/D -> Horizontal, W/S -> Vertical).</summary>
    public void SetMovement(float horizontal, float vertical)
    {
        if (playerAnimator == null) return;
        if (HasParameter(HorizontalHash, AnimatorControllerParameterType.Float))
            playerAnimator.SetFloat(HorizontalHash, horizontal);
        if (HasParameter(VerticalHash, AnimatorControllerParameterType.Float))
            playerAnimator.SetFloat(VerticalHash, vertical);
    }

    /// <summary>Marca si el personaje está tocando el suelo (controla salir/entrar del salto).</summary>
    public void SetGrounded(bool grounded)
    {
        if (playerAnimator == null) return;
        if (HasParameter(OnGroundHash, AnimatorControllerParameterType.Bool))
            playerAnimator.SetBool(OnGroundHash, grounded);
    }

    public void TriggerJump() => FireTrigger(JumpHash);

    public void TriggerTakeDamage() => FireTrigger(TakeDamageHash);

    public void TriggerDeath() => FireTrigger(DeathHash);

    /// <summary>Activa/desactiva la pose de casteo (Cast=true -> Ataque, Cast=false -> volver a Idle).</summary>
    public void SetCasting(bool casting)
    {
        if (playerAnimator == null) return;
        if (HasParameter(CastHash, AnimatorControllerParameterType.Bool))
            playerAnimator.SetBool(CastHash, casting);
    }

    // Compatibilidad con el código existente (BattleState / SpellNetworkController).
    public void TriggerCasting() => SetCasting(true);
    public void StopCasting() => SetCasting(false);

    /// <summary>Reproduce una vez el gesto de interacción (Interact bool activo durante un instante).</summary>
    public void TriggerInteract()
    {
        if (playerAnimator == null) return;
        if (!HasParameter(InteractHash, AnimatorControllerParameterType.Bool)) return;
        if (!isActiveAndEnabled)
        {
            playerAnimator.SetBool(InteractHash, true);
            return;
        }
        if (_interactRoutine != null) StopCoroutine(_interactRoutine);
        _interactRoutine = StartCoroutine(InteractGesture());
    }

    private IEnumerator InteractGesture()
    {
        playerAnimator.SetBool(InteractHash, true);
        yield return new WaitForSeconds(interactGestureDuration);
        if (playerAnimator != null) playerAnimator.SetBool(InteractHash, false);
        _interactRoutine = null;
    }

    private void FireTrigger(int hash)
    {
        if (playerAnimator == null) return;
        if (!HasParameter(hash, AnimatorControllerParameterType.Trigger)) return;

        if (networkAnimator != null) networkAnimator.SetTrigger(hash);
        else playerAnimator.SetTrigger(hash);
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
