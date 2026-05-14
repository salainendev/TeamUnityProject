using UnityEngine;

public class GizmoTest : MonoBehaviour
{
    [Header("Debug Distances")]
    public float forwardDistance = 1.2f;
    public float sideDistance = 0.8f;
    public float sphereRadius = 0.25f;

    private void Update()
    {
        // Рисуем линии только в Play режиме
        if (!Application.isPlaying) return;

        // 🔴 ВПЕРЁД
        Debug.DrawLine(
            transform.position,
            transform.position + transform.forward * forwardDistance,
            Color.red,
            0.1f
        );

        // 🟡 ВПРАВО
        Debug.DrawLine(
            transform.position,
            transform.position + transform.right * sideDistance,
            Color.yellow,
            0.1f
        );

        // 🟡 ВЛЕВО
        Debug.DrawLine(
            transform.position,
            transform.position - transform.right * sideDistance,
            Color.yellow,
            0.1f
        );
    }

    private void OnDrawGizmos()
    {
        // 🔴 Центр бабочки
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);

        // 🟣 Радиус "тела" (пример коллизии)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);

        // 🔵 Покажем forward прямо в редакторе
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * forwardDistance);
    }
}