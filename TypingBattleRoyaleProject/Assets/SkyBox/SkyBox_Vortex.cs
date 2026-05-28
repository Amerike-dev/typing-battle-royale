using UnityEngine;

public class SkyBox_Vortex : MonoBehaviour
{
    [SerializeField] private Vector3 ejeRotacion = new Vector3(0, 1, 0);
    [SerializeField] private float velocidadRotacion = 50f;

    void Update()
    {
        transform.Rotate(ejeRotacion, velocidadRotacion * Time.deltaTime);
    }
}
