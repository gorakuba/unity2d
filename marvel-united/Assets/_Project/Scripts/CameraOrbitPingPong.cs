using UnityEngine;
using System.Collections;

public class CameraOrbitPingPong : MonoBehaviour
{
    public float speed = 20f;        // Szybkość obrotu
    public float pauseDuration = 1f; // Ile sekund zatrzymania
    private Vector3 offset;
    private bool rotatingForward = true;
    private float angle = 0f;        // Kąt całkowity obrotu

    void Start()
    {
        offset = transform.position - Vector3.zero;
        StartCoroutine(RotatePingPong());
    }

    IEnumerator RotatePingPong()
    {
        while (true)
        {
            // Reset kąta
            angle = 0f;

            // Rotacja w jedną stronę (do 180 stopni)
            while (angle < 180f)
            {
                float deltaAngle = speed * Time.deltaTime;
                angle += deltaAngle;

                Quaternion rotation = Quaternion.Euler(0, rotatingForward ? -deltaAngle : deltaAngle, 0);
                offset = rotation * offset;
                transform.position = Vector3.zero + offset;
                transform.LookAt(Vector3.zero);

                yield return null;
            }

            // Pauza
            yield return new WaitForSeconds(pauseDuration);

            // Zmieniamy kierunek
            rotatingForward = !rotatingForward;
        }
    }
}