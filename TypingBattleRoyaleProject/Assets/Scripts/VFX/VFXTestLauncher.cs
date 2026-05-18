using UnityEngine;
using UnityEngine.InputSystem;

public class VFXTestLauncher : MonoBehaviour
{
    public Spell spell;
    public GameObject vfxPrefab;
    public bool autoFire = false;
    public float autoInterval = 0.5f;

    float _nextSpawn;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) Fire();
        if (autoFire && Time.time >= _nextSpawn)
        {
            _nextSpawn = Time.time + autoInterval;
            Fire();
        }
    }

    void Fire()
    {
        if (spell == null || vfxPrefab == null)
        {
            Debug.LogWarning("[VFXTestLauncher] Asigna Spell y VFX Prefab en el inspector.");
            return;
        }
        var go = Instantiate(vfxPrefab, transform.position, transform.rotation);
        var projectile = go.GetComponent<ProjectileVFX>();
        if (projectile != null) projectile.Launch(spell, transform.forward);
        else Debug.LogWarning("[VFXTestLauncher] El prefab no tiene ProjectileVFX.");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}
