using UnityEngine;

public class Location : MonoBehaviour
{
    public ThreatCardInstance currentThreat;

    public void AssignThreatCard(ThreatCardInstance card)
    {
        currentThreat = card;
        card.assignedLocation = gameObject;
    }

    public bool IsBlockedByThreat()
    {
        return currentThreat != null;
    }
}
