using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonolithController : NetworkBehaviour
{
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
    public List<SpellData> allSpellData = new List<SpellData>();

    [Header("Configuración de Hundimiento")]
    public float sinkDepth = 5f;
    public float sinkDuration = 2f;

    void Awake()
    {
        data = new MonolithData(id, level, runeChallenge);
    }

    public void ServerInitialize()
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
        if (!IsServer) return; // Solo el host gestiona los hechizos de la red

        if (spells.Contains(spellName))
        {
            spells.Remove(spellName);
        }

        // Si ya no quedan hechizos en el monolito, iniciamos el final
        if (spells.Count == 0)
        {
            TriggerSinkEffectClientRpc();
        }
    }
    Elements GetMappedElement(Elements original)
    {

        switch (original)
        {
            case Elements.Wind:
                return Elements.Thunder;

            case Elements.Water:
                return Elements.Water;

            case Elements.Earth:
                return Elements.Fire;

            case Elements.Nature:
                return Elements.Thunder;

            default:
                return original;
        }

    }

    public void PopulateSpells()
    {
        // 1. Limpiamos la lista actual para empezar frescos
        spells.Clear();

        // 2. Elegimos el Elemento Principal al azar
        // Obtenemos todos los elementos posibles de tu enum 'Elements'
        System.Array allElements = System.Enum.GetValues(typeof(Elements));
        int randomElementIndex = Random.Range(1, allElements.Length); // Empieza en 1 si el 0 es "Ninguno"
        Elements targetElement = (Elements)allElements.GetValue(randomElementIndex);

        // Si sigues usando tu función de mapeo (ej. Wind -> Thunder), la aplicamos aquí:
        targetElement = GetMappedElement(targetElement);

        Debug.Log($"[Monolito] Elemento Principal Elegido: {targetElement}");

        // 3. Iteramos automáticamente por TODOS los Tiers que existan en tu enum 'SpellTiers'
        // ¡Esto lo hace 100% escalable! Si agregas un Tier 4 mañana, esto lo detecta solo.
        System.Array allTiers = System.Enum.GetValues(typeof(SpellTiers));

        foreach (SpellTiers currentTier in allTiers)
        {
            // Creamos la "cubeta" para este Tier en específico
            List<SpellData> spellsInThisTier = new List<SpellData>();

            // Filtramos nuestra base de datos buscando hechizos que cumplan las dos condiciones
            foreach (SpellData sData in allSpellData)
            {
                if (sData == null) continue; // Seguro de vida contra espacios vacíos en el Inspector

                if (sData.elementType == targetElement && sData.spellTier == currentTier)
                {
                    spellsInThisTier.Add(sData);
                }
            }

            // 4. Si la cubeta tiene hechizos, sacamos UNO al azar
            if (spellsInThisTier.Count > 0)
            {
                int randomSpellIndex = Random.Range(0, spellsInThisTier.Count);
                SpellData selectedSpellData = spellsInThisTier[randomSpellIndex];
                
                // NOTA: Tu lista 'spells' espera un objeto tipo 'Spell'. 
                // Aquí buscamos el 'Spell' correspondiente en 'allSpells' que coincida con este 'SpellData'
                Spell matchingSpell = allSpells.Find(s => s.elementType == selectedSpellData.elementType /* Agrega aquí más condiciones si necesitas vincularlos exacto */);
                
                if (matchingSpell != null)
                {
                    spells.Add(matchingSpell);
                    Debug.Log($"[Monolito] Agregado - Tier: {currentTier} | Hechizo: {selectedSpellData.runeString}");
                }
            }
            else
            {
                // Si aún no han creado hechizos para este Tier y Elemento, no crashea, solo avisa.
                Debug.LogWarning($"[Monolito] No hay hechizos de {targetElement} en el Tier {currentTier}. Saltando...");
            }
        }
    }
    
    [ClientRpc]
    private void TriggerSinkEffectClientRpc()
    {
        // 1. Quitamos interacciones
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Iniciamos la animación local
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

        // 3. El Host destruye el objeto de la red al terminar
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
