using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class RedskullThreat04 : MonoBehaviour, IThreatAbility
{
    private ThreatCardInstance _threat;
    private GameObject _choicePanel;

    public void Init(ThreatCardInstance threatInstance, GameObject choicePanel)
    {
        _threat      = threatInstance;
        _choicePanel = choicePanel;
        _choicePanel.SetActive(false);
    }

    public void RegisterTrigger(string trigger, ThreatCardInstance inst)
    {
        if (trigger == "OnBAM")
            VillainController.Instance.OnBAMEffect += OnBam;
    }

    public void OnTurnStart(ThreatCardInstance threat, HeroController hero) { }

    private IEnumerator OnBam()
    {
        var heroes = UnityEngine.Object
            .FindObjectsByType<HeroController>(FindObjectsSortMode.None)
            .Where(h => h.CurrentLocation?.gameObject == _threat.assignedLocation && !h.IsStunned && !h.IsInvulnerable);

        foreach (var hero in heroes)
            yield return HandleChoice(hero);
    }

private IEnumerator HandleChoice(HeroController hero)
{
    if (hero.IsStunned || hero.IsInvulnerable) yield break;
    _choicePanel.SetActive(true);
    var ctrl = _choicePanel.GetComponent<ThreatChoicePanelController>();

    // dynamiczne napisy
    string displayName = GameManager.Instance.GetHeroName(hero.HeroId);
    string dmgLabel    = "Take 1 Damage";
    string tokenLabel  = "Take 1 Threat Token";

    bool? takeDamage = null;
    ctrl.Init(
        displayName,
        dmgLabel, 
        tokenLabel,
        onDamage:  () => {
            takeDamage = true;
            _choicePanel.SetActive(false);
        },
        onTokens:  () => {
            takeDamage = false;
            _choicePanel.SetActive(false);
        }
    );

    yield return new WaitUntil(() => takeDamage.HasValue);

    if (takeDamage.Value)
    {
        var dmg = hero.GetComponent<HeroDamageHandler>();
        yield return dmg.TakeDamageCoroutine(false);  // jeden discard
    }
    else
    {
        CrisisTokenManager.Instance.GiveCrisisToken(hero);
        Debug.Log($"[RedskullThreat04] {hero.HeroId} otrzymuje 1 Threat Token");
    }
}

    private void OnDestroy()
    {
        if (VillainController.Instance != null)
            VillainController.Instance.OnBAMEffect -= OnBam;
    }
}
