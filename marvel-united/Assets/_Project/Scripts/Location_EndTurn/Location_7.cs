using System.Collections;
using UnityEngine;

public class Location_7 : MonoBehaviour, ILocationEndTurnAbility
{
    public IEnumerator ExecuteEndTurn(HeroController hero)
    {
        if (hero == null)
            yield break;

        var moveMgr = UnityEngine.Object.FindAnyObjectByType<HeroMovementManager>();
        if (moveMgr == null)
            yield break;

        bool moveDone = false;
        moveMgr.OnMoveCompleted = () => { moveDone = true; };
        moveMgr.PrepareHeroTeleport();
        yield return new WaitUntil(() => moveDone);
        moveMgr.OnMoveCompleted = null;
    }
}