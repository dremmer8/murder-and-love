using UnityEngine;

/// <summary>
/// Shared layer names/masks for look+E interactables, dialogue volumes, and in-minigame click zones.
/// Keep those colliders on the matching layer so the raycast systems never hit each other.
/// </summary>
public static class GameLayers
{
    public const string Interactable = "Interactable";
    public const string MinigameZone = "MinigameZone";
    public const string DialogueZone = "DialogueZone";

    public static int InteractableIndex => LayerMask.NameToLayer(Interactable);
    public static int MinigameZoneIndex => LayerMask.NameToLayer(MinigameZone);
    public static int DialogueZoneIndex => LayerMask.NameToLayer(DialogueZone);

    public static LayerMask InteractableMask => MaskOrZero(InteractableIndex);
    public static LayerMask MinigameZoneMask => MaskOrZero(MinigameZoneIndex);
    public static LayerMask DialogueZoneMask => MaskOrZero(DialogueZoneIndex);

    static LayerMask MaskOrZero(int layerIndex) =>
        layerIndex >= 0 ? (LayerMask)(1 << layerIndex) : 0;
}
