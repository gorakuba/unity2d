using UnityEngine;

public class ThreatCardInstance : MonoBehaviour
{
    public ThreatCard data;               // Dane z JSON-a
    public GameObject assignedLocation;   // Obiekt lokacji, do której karta została przypisana
    public int currentMinionHealth = 0;

}
