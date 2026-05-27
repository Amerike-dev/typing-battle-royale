using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class MonolithController : NetworkBehaviour
{
    public static readonly Elements[] PlayableElements = new[]
    {
        Elements.Fire, Elements.Water, Elements.Earth, Elements.Wind,
        Elements.Nature, Elements.Thunder, Elements.Ice, Elements.Lava
    };
    private static readonly HashSet<Elements> PlayableElementsSet = new HashSet<Elements>(PlayableElements);

    [Header("Intanciar monolito")]
    public MonolithData data;
    public string id;
    public int level;
    public string runeChallenge;

    [Header("Revision monolito")]
    public List<string> idPlayers = new List<string>();
    public List<Spell> spells = new List<Spell>();

    [Header("Datos Elementos")]
    public List<Spell> allSpells = new List<Spell>();

    [HideInInspector] public Elements forcedTargetElement = Elements.None;

    [Header("Configuración de Hundimiento")]
    public float sinkDepth = 5f;
    public float sinkDuration = 2f;

    public NetworkList<FixedString64Bytes> syncedSpellNames;
    public NetworkList<bool> syncedSpellClaimed;

    void Awake()
    {
        data = new MonolithData(id, level, runeChallenge);
        syncedSpellNames = new NetworkList<FixedString64Bytes>();
        syncedSpellClaimed = new NetworkList<bool>();
        
        if (allSpells == null || allSpells.Count == 0)
        {
            allSpells = Resources.LoadAll<Spell>("Spells").ToList();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        PopulateSpells();
    }

    public void AddIdPlayer(string id)
    {
        idPlayers.Add(id);
    }

    public bool IdPlayerExist(string id)
    {
        if (!idPlayers.Contains(id))
        {
            AddIdPlayer(id);
            return false;
        }
        else
        {
            Debug.Log("Este player ya agarro un hechizo");
            return true;
        }
    }

    public void RemoveSpellData(Spell spellName)
    {
        if (!IsServer) return;

        if (spells.Contains(spellName)) spells.Remove(spellName);
        
        if (spells.Count == 0) TriggerSinkEffectClientRpc();
    }
    public void PopulateSpells()
    {
        spells.Clear();

        Elements targetElement = forcedTargetElement != Elements.None
            ? forcedTargetElement
            : PlayableElements[Random.Range(0, PlayableElements.Length)];

        List<Spell> playablePool = allSpells
            .Where(s => s != null && PlayableElementsSet.Contains(s.elementType))
            .ToList();

        System.Array allTiers = System.Enum.GetValues(typeof(SpellTiers));

        foreach (SpellTiers currentTier in allTiers)
        {
            List<Spell> spellsInThisTier = playablePool
                .Where(s => s.tier == currentTier)
                .ToList();

            List<Spell> perfectMatch = spellsInThisTier.Where(s => s.elementType == targetElement).ToList();

            if (perfectMatch.Count > 0)
            {
                spells.Add(perfectMatch[Random.Range(0, perfectMatch.Count)]);
            }
            else if (spellsInThisTier.Count > 0)
            {
                spells.Add(spellsInThisTier[Random.Range(0, spellsInThisTier.Count)]);
            }
            else
            {
                if (playablePool.Count > 0)
                    spells.Add(playablePool[Random.Range(0, playablePool.Count)]);
            }
        }

        foreach (var spell in spells)
        {
            syncedSpellNames.Add(spell.spellName);
            syncedSpellClaimed.Add(false);
        }
    }
    
    public void MarkSpellAsClaimed(int index)
    {
        if (!IsServer) return;

        syncedSpellClaimed[index] = true;
        bool allClaimed = true;
        
        for (int i = 0; i < syncedSpellClaimed.Count; i++)
        {
            if (!syncedSpellClaimed[i]) allClaimed = false;
        }
        
        if (allClaimed) TriggerSinkEffectClientRpc();
    }
    
    [ClientRpc]
    private void TriggerSinkEffectClientRpc()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        StartCoroutine(SinkRoutine());
    }

    private IEnumerator SinkRoutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (Vector3.down * sinkDepth);
        float elapsedTime = 0f;

        while (elapsedTime < sinkDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / sinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        
        if (IsServer) GetComponent<NetworkObject>().Despawn(true);
    }
}
