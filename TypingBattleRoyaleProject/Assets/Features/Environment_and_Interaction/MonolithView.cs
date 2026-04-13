using UnityEngine;

public class MonolithView : MonoBehaviour
{
    [SerializeField] private MonolithData monolithData;
    [SerializeField] private ParticleSystem unlockVFX;
    [SerializeField] private Renderer monolithRenderer;
    public bool IsExausted {  get; private set; }
    public System.Action<SpellData> OnMonolithUnlocked;

    public int Level => monolithData != null ? monolithData.Level : 0;

    public bool TryInteract(PlayerStats interactiongPlayer)
    {
        if(monolithData==null || monolithData.spellData == null) { Debug.Log("monolithData o SpellData no asignado."); }
        if (IsExausted) { return false; };
        SpellData spell = monolithData.spellData;
        OnMonolithUnlocked?.Invoke(spell);
        PlayUnlockVFX();
        return true;
    }
    public void PlayUnlockVFX()
    {
        if(unlockVFX != null)
        {
            unlockVFX.Play();
            IsExausted = true;
        }

    }

    public void SetData(MonolithData data)
    {
        monolithData = data;
    }
}
