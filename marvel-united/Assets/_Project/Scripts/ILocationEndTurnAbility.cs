using System.Collections;

public interface ILocationEndTurnAbility
{
    /// <summary>
    /// Execute this location's end-of-turn effect for the given hero.
    /// </summary>
    IEnumerator ExecuteEndTurn(HeroController hero);
}