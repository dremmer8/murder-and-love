using UnityEngine;

/// <summary>
/// Drop on the same GameObject as the coin-machine <see cref="Animator"/>.
/// Wire the same Animation Event on the fail clip to <see cref="OnFailBillMaterialChange"/>.
/// Each fail attempt advances to the next material on the bill skinned mesh.
/// </summary>
public class CoinMachineBillMaterialEvents : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer billMesh;

    [Tooltip("Material slot on the skinned mesh to replace (usually 0).")]
    [SerializeField] int materialSlotIndex;

    [Tooltip("One material per fail attempt (index 0 = first fail, 1 = second fail).")]
    [SerializeField] Material[] failMaterials;

    [Tooltip("Optional starting material restored by ResetBillMaterial / OnEnable.")]
    [SerializeField] Material initialMaterial;

    int failIndex;

    void OnEnable()
    {
        failIndex = 0;
        if (initialMaterial != null)
            Apply(initialMaterial);
    }

    /// <summary>
    /// Animation Event target — place once on the fail clip.
    /// First play → failMaterials[0], second play → failMaterials[1], etc.
    /// </summary>
    public void OnFailBillMaterialChange()
    {
        if (billMesh == null || failMaterials == null || failMaterials.Length == 0)
            return;

        if (failIndex < 0 || failIndex >= failMaterials.Length)
        {
            Debug.LogWarning(
                $"{name}: No fail material for attempt {failIndex} " +
                $"(array length {failMaterials.Length}).",
                this);
            return;
        }

        Material next = failMaterials[failIndex];
        failIndex++;

        if (next == null)
        {
            Debug.LogWarning($"{name}: failMaterials[{failIndex - 1}] is null.", this);
            return;
        }

        Apply(next);
    }

    /// <summary>Optional Animation Event / kickOff hook to restore the starting look.</summary>
    public void ResetBillMaterial()
    {
        failIndex = 0;
        if (initialMaterial != null)
            Apply(initialMaterial);
    }

    void Apply(Material material)
    {
        if (billMesh == null || material == null)
            return;

        Material[] mats = billMesh.materials;
        if (materialSlotIndex < 0 || materialSlotIndex >= mats.Length)
        {
            Debug.LogWarning(
                $"{name}: materialSlotIndex {materialSlotIndex} out of range " +
                $"(mesh has {mats.Length} materials).",
                this);
            return;
        }

        mats[materialSlotIndex] = material;
        billMesh.materials = mats;
    }
}
