using UnityEngine;

public class ItemDestination : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, 0.04f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.12f);
    }
}
