using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThreatCardSpawner : MonoBehaviour
{
    [Header("Prefaby i dane JSON")]
    public GameObject threatCardPrefab;
    public GameObject tokenHealthPrefab;
    public ThreatCardTextureDatabase textureDatabase;
    public TextAsset villainJson;

    private LocationManager _locMan;

    private void Awake()
    {
        _locMan = UnityEngine.Object.FindFirstObjectByType<LocationManager>();
        if (_locMan == null)
            Debug.LogError("ThreatCardSpawner: nie znalazłem LocationManager!");
        else
            _locMan.OnLocationsAndTokensReady += () => StartCoroutine(DelayedThreatSpawn());
    }

    private void OnDestroy()
    {
        if (_locMan != null)
            _locMan.OnLocationsAndTokensReady -= () => StartCoroutine(DelayedThreatSpawn());
    }

    private IEnumerator DelayedThreatSpawn()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnAllThreatCards();
    }

    private void SpawnAllThreatCards()
    {
        Debug.Log("▶ ThreatCardSpawner: Start spawnów");

        if (GameManager.Instance == null)
        {
            Debug.LogError("ThreatCardSpawner: GameManager.Instance jest null");
            return;
        }

        string villainId = GameManager.Instance.selectedVillain;
        if (string.IsNullOrEmpty(villainId))
        {
            Debug.LogError("ThreatCardSpawner: selectedVillain jest pusty");
            return;
        }

        // 1) Wczytujemy i losujemy 6 threatów z JSONa
        var rootData = JsonUtility.FromJson<VillainsRoot>(villainJson.text);
        var villain  = rootData.villains.FirstOrDefault(v => v.id == villainId);
        if (villain == null)
        {
            Debug.LogError($"ThreatCardSpawner: nie ma villain ID={villainId}");
            return;
        }

        var threats = villain.threats
                             .OrderBy(_ => UnityEngine.Random.value)
                             .Take(6)
                             .ToList();

        // 2) Czyścimy każdy ThreatCardSlot
        var roots = _locMan.LocationRoots;
        foreach (var root in roots)
        {
            var slot = FindDeepChild(root, "ThreatCardSlot");
            if (slot == null) continue;
            for (int i = slot.childCount - 1; i >= 0; i--)
                Destroy(slot.GetChild(i).gameObject);
        }

        // 3) Instancjonujemy i przypinamy
        for (int i = 0; i < threats.Count && i < roots.Count; i++)
        {
            var threat = threats[i];
            var root   = roots[i];
            var slot   = FindDeepChild(root, "ThreatCardSlot");
            if (slot == null) continue;

            // a) Instantiate prefab karty
            var go = Instantiate(threatCardPrefab, slot, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            // b) Configurujemy ThreatCardInstance
            var inst = go.GetComponent<ThreatCardInstance>()
                       ?? go.AddComponent<ThreatCardInstance>();
            inst.data             = threat;
            inst.data.BuildDictionaries();
            inst.assignedLocation = root.gameObject;

            // c) Przypinamy do LocationController
            var locCtrl = root.GetComponentInChildren<LocationController>();
            if (locCtrl != null)
                locCtrl.AssignThreatCard(inst);
            else
                Debug.LogWarning($"ThreatCardSpawner: brakuje LocationController w {root.name}");

            // d) Przypinamy też do prostego Location (jeśli używasz tej klasy)
            var locComp = root.GetComponentInChildren<Location>();
            if (locComp != null)
                locComp.AssignThreatCard(inst);
            else
                Debug.LogWarning($"ThreatCardSpawner: brakuje Location w {root.name}");

            // e) Ustawiamy tekstury front/back
            var disp = go.GetComponent<CardDisplay>();
            if (disp != null)
            {
                disp.frontTexture = textureDatabase.GetTexture(villainId, threat.id);
                disp.backTexture  = textureDatabase.GetBackTexture(villainId);
            }

            // f) Spawn tokenów minionów (jeśli są)
            if (threat.minion
                && int.TryParse(threat.minion_health, out int hp)
                && hp > 0)
            {
                StartCoroutine(SpawnMinionTokens(go.transform, hp));
            }
        }
    }

    private IEnumerator SpawnMinionTokens(Transform card, int health)
    {
        yield return new WaitForSeconds(0.5f);
        var slot = card.Find("Slot_Health");
        if (slot == null)
        {
            Debug.LogWarning("ThreatCardSpawner: brak Slot_Health na karcie!");
            yield break;
        }
        for (int i = 0; i < health; i++)
        {
            var tok = Instantiate(tokenHealthPrefab, slot);
            tok.transform.localPosition = new Vector3(0, 0, i * 0.00004f);
            tok.transform.localRotation = Quaternion.identity;
            tok.transform.localScale    = Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform c in parent)
        {
            if (c.name == name) return c;
            var r = FindDeepChild(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
