using UnityEngine;

public class HeroActionHandler : MonoBehaviour
{
    public HeroMovementManager movementManager;
    public SymbolPanelUI symbolPanelUI;

    public void HandleAction(string symbolId, GameObject symbolButton)
    {

        movementManager.CancelHeroMovement();

        switch (symbolId.ToLower())
        {
            case "move":
                Debug.Log("BOHATER WYKONUJE RUCH");

                movementManager.OnMoveCompleted = () =>
                {
                    Destroy(symbolButton); // 💥 symbol znika z UI
                    symbolPanelUI.ClearSelectedSymbol();
                    movementManager.OnMoveCompleted = null;
                };

                movementManager.PrepareHeroMovement();
                break;

            case "attack":
                Debug.Log("BOHATER WYKONUJE ATAK");
                break;

            case "heroic":
                Debug.Log("BOHATER WYKONUJE CZYN HEROICZNY");
                break;

            case "wild":
                Debug.Log("BOHATER WYKONUJE DZIKĄ AKCJĘ");
                break;

            default:
                Debug.LogWarning($"Nieznany symbol: {symbolId}");
                break;
        }
    }
}
