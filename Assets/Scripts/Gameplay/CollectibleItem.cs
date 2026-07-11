using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public string slotKey;
    public Transform animatedTransform;

    public Transform Animated => animatedTransform != null ? animatedTransform : transform;
}
