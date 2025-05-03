using System.Collections;
using System.Linq;
using UnityEngine;

public class VillainController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer visualRenderer;
    public VillainVisualDatabase visualDatabase;

    [Header("Movement Settings")]
    [Tooltip("Time (in seconds) for one movement step")]  
    public float stepDuration = 0.3f;

    private Transform[] _villainSlots;
    private int _currentIndex;

    void Awake()
    {
        // Pobierz wszystkie sloty (Villain_Slot) z LocationManager
        var locMan = Object.FindFirstObjectByType<LocationManager>();
        _villainSlots = locMan.VillainSlots.ToArray();
    }

    // ============================
    // 1️⃣ Initialization
    // ============================
    /// <summary>
    /// Ustawia grafikę i pozycję zbira na określonym slocie.
    /// </summary>
    /// <param name="villainID">ID zbira (klucz do bazy sprite'ów)</param>
    /// <param name="startIndex">Indeks slota startowego (0-based)</param>
    public void Initialize(string villainID, int startIndex = 0)
    {
        var locMan = FindAnyObjectByType<LocationManager>();
        _villainSlots = locMan.VillainSlots.ToArray();
        // Ustaw sprite
        var sprite = visualDatabase.GetVillainSprite(villainID);
        if (sprite != null)
            visualRenderer.sprite = sprite;

        // Ustawienie pozycji
        _currentIndex = startIndex;
        transform.SetParent(_villainSlots[_currentIndex], false);
        transform.localPosition = Vector3.zero;
    }

    // ============================
    // 2️⃣ Movement
    // ============================
    /// <summary>
    /// Korutyna: przesuń się o podaną liczbę pól zgodnie z ruchem wskazówek zegara.
    /// </summary>
    public IEnumerator MoveVillain(int steps)
    {
        int count = _villainSlots.Length;
        for (int i = 0; i < steps; i++)
        {
            _currentIndex = (_currentIndex + 1) % count;
            var target = _villainSlots[_currentIndex];
            yield return StartCoroutine(AnimateMoveTo(target.position));
            // Przypnij w hierarchii, by zbira „trzymał” slot
            transform.SetParent(target, true);
        }
    }

    private IEnumerator AnimateMoveTo(Vector3 targetPos)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < stepDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, targetPos, t / stepDuration);
            yield return null;
        }
        transform.position = targetPos;
    }

    // ============================
    // 3️⃣ BAM Attack
    // ============================
    public IEnumerator ExecuteAttack(VillainCard card)
    {
        if (card.BAM_effect)
        {
            Debug.Log("💥 BAM effect!");
            // TODO: dodaj animacje/efekty obrażeń
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ============================
    // 4️⃣ Spawn Tokens
    // ============================
    public IEnumerator ExecuteSpawn(VillainCard card)
    {
        bool hasSpawn = !string.IsNullOrEmpty(card.Location_left)
                     || !string.IsNullOrEmpty(card.Location_middle)
                     || !string.IsNullOrEmpty(card.Location_right);
        if (hasSpawn)
        {
            Debug.Log("🎯 Spawn tokens");
            // TODO: wywołaj logikę SpawnTokens z LocationManager
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ============================
    // 5️⃣ Special Ability
    // ============================
    public IEnumerator ExecuteAbility(VillainCard card)
    {
        if (card.special)
        {
            Debug.Log($"🌟 Special: {card.special_name}");
            // TODO: zaimplementuj szczegółowość zdolności
            yield return new WaitForSeconds(0.5f);
        }
    }
}
