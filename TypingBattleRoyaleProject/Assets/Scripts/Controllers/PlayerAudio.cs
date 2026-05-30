using UnityEngine;
using Unity.Netcode;

public class PlayerAudio : NetworkBehaviour
{
    public AudioSource audioSource;
    public AudioEntry[] audioEntries;
    public AudioSource spellAudioSource;

    public void ChangeSound(AudioEntry audio)
    {
        if (audio == null || audio.clip == null) return;
        if (!IsOwner) return;
        PlayLocal(audio);
        PlaySoundServerRpc(audio.id);
    }

    public void ChangeSoundById(string id)
    {
        if (!IsOwner) return;
        AudioEntry audio = FindEntryById(id);
        PlayLocal(audio);
        PlaySoundServerRpc(id);
    }

    [ServerRpc]
    private void PlaySoundServerRpc(string id)
    {
        PlaySoundClientRpc(id);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(string id)
    {
        if (IsOwner) return; // ya lo reprodujo localmente
        AudioEntry audio = FindEntryById(id);
        PlayLocal(audio);
    }

    private void PlayLocal(AudioEntry audio)
    {
        if (audio == null || audio.clip == null) return;
        audioSource.Stop();
        audioSource.clip = audio.clip;
        audioSource.volume = audio.volume;
        audioSource.pitch = audio.randomizePitch ? Random.Range(audio.pitch * 0.9f, audio.pitch * 1.1f) : audio.pitch;
        audioSource.loop = audio.loop;
        audioSource.Play();
    }

    private AudioEntry FindEntryById(string id)
    {
        foreach (AudioEntry entry in audioEntries)
        {
            if (entry.id == id) return entry;
        }
        Debug.LogWarning($"AudioEntry '{id}' no encontrado.");
        return null;
    }

    public void PlaySpellSound(Spell spell)
    {
        if (spell == null || spell.spellSound == null) return;
        if (!IsOwner) return;
        PlaySpellLocal(spell.spellSound);
        PlaySpellServerRpc(spell.spellName);
    }

    [ServerRpc]
    private void PlaySpellServerRpc(string spellName)
    {
        PlaySpellClientRpc(spellName);
    }

    [ClientRpc]
    private void PlaySpellClientRpc(string spellName)
    {
        if (IsOwner) return;

        var cast = GetComponentInChildren<CastInputController>(true);
        if (cast == null || cast.currentSpell == null) return;
        if (cast.currentSpell.spellName != spellName) return;
        if (cast.currentSpell.spellSound == null) return;

        PlaySpellLocal(cast.currentSpell.spellSound);
    }

    private void PlaySpellLocal(AudioClip clip)
    {
        if (clip == null) return;
        spellAudioSource.Stop();
        spellAudioSource.clip = clip;
        spellAudioSource.volume = 1f;
        spellAudioSource.pitch = 1f;
        spellAudioSource.loop = false;
        spellAudioSource.Play();
    }
}