using UnityEngine;

public class AppearAndFall : MonoBehaviour
{
    public float delay = 3f; // czas w sekundach po którym kafelek się pojawi i spadnie

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // na starcie wyłącz obiekt i zablokuj fizykę
        gameObject.SetActive(false);
        Invoke(nameof(ActivateAndFall), delay);
    }

    void ActivateAndFall()
    {
        gameObject.SetActive(true);
        rb = GetComponent<Rigidbody>(); // trzeba pobrać jeszcze raz po aktywacji
        rb.isKinematic = false;
    }
}
