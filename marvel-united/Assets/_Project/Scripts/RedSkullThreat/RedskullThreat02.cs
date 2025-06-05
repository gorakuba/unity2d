using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class RedskullThreat02 : MonoBehaviour, IThreatAbility
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
        HUDMessageManager.Instance?.Enqueue("Karta Threat sie aktywuje");
        var heroes = UnityEngine.Object
            .FindObjectsByType<HeroController>(FindObjectsSortMode.None)
            .Where(h => h.CurrentLocation?.gameObject == _threat.assignedLocation);

        foreach (var hero in heroes)
            yield return HandleChoice(hero);
    }

private IEnumerator HandleChoice(HeroController hero)
{
    if (hero.IsStunned) yield break;
    _choicePanel.SetActive(true);
    var ctrl = _choicePanel.GetComponent<ThreatChoicePanelController>();

    // dynamiczne napisy
    string displayName = GameManager.Instance.GetHeroName(hero.HeroId);
    string dmgLabel    = "Take 2 Damage";
    string tokenLabel  = "Take 2 Threat Token";

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
            yield return dmg.TakeDamageCoroutine();
            yield return dmg.TakeDamageCoroutine();
    }
        else
        {
            CrisisTokenManager.Instance.GiveCrisisToken(hero);
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
