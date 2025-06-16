using UnityEngine;

/// <summary>
/// Stabilna orbita z ustaloną wysokością.
/// </summary>
public class OrbitAroundTarget : MonoBehaviour
{
    public Transform target;
    public float speed = 10f;
    public float distance = 10f;
    public float height = 5f;  // << wysokość kamery ponad celem
    private float angle = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        angle += speed * Time.deltaTime;
        if (angle > 360f) angle -= 360f;

        float radians = angle * Mathf.Deg2Rad;
        Vector3 offset = target.forward * Mathf.Sin(radians) +
                         target.right * Mathf.Cos(radians);
        offset *= distance;

        // Dodaj wysokość w osi Y
        Vector3 orbitPosition = target.position + offset + new Vector3(0, height, 0);
        transform.position = orbitPosition;

        transform.LookAt(target.position);
    }
}
