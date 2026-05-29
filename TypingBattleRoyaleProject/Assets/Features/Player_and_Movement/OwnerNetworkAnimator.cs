using Unity.Netcode.Components;

/// <summary>
/// NetworkAnimator con autoridad del cliente dueño (owner) en lugar del servidor.
/// El jugador local dispara sus propias animaciones (movimiento, salto, cast,
/// interacción, daño, muerte) y se replican al resto de clientes.
/// </summary>
[UnityEngine.DisallowMultipleComponent]
public class OwnerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative() => false;
}
