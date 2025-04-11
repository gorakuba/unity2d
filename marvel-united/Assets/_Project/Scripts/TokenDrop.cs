using UnityEngine;

public class TokenDrop : MonoBehaviour
{
    public float dropHeight = 1.0f;
    public float dropTime = 0.4f;

    private Vector3 targetLocalPosition;
    private float elapsedTime = 0f;
    private bool isDropping = true;

    void Start()
    {
        // Zapamiętujemy miejsce docelowe
        targetLocalPosition = transform.localPosition;
        // Startujemy z wyższej pozycji
        transform.localPosition += new Vector3(0, dropHeight, 0);
    }

    void Update()
    {
        if (!isDropping) return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / dropTime);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, t);

        if (t >= 1f)
            isDropping = false;
    }
}
