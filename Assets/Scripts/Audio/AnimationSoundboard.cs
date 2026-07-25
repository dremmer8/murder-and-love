using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Drop on the same GameObject as an <see cref="Animator"/>. Wire Animation Events to the
/// parameterless methods below (or <see cref="Play"/> with a SoundLibrary key).
/// </summary>
[DisallowMultipleComponent]
public class AnimationSoundboard : MonoBehaviour
{
    [SerializeField]
    [Tooltip("If true, one-shots follow this transform. If false, they play at this position once.")]
    bool m_AttachToThis = true;

    [Header("Give item")]
    [Tooltip("Fired by ItemPlacedOnTable() Animation Event — wire to DialogueItemUnhide.RevealTableItem.")]
    public UnityEvent onItemPlacedOnTable;

    /// <summary>C# listeners (e.g. DialogueItemUnhide). Same moment as <see cref="onItemPlacedOnTable"/>.</summary>
    public event Action ItemPlacedOnTable;

    public void Play(string soundLibraryKey)
    {
        if (string.IsNullOrWhiteSpace(soundLibraryKey))
            return;

        var mgr = SoundManager.Instance;
        if (mgr == null)
        {
            Debug.LogWarning("[AnimationSoundboard] No SoundManager — cannot play sound.", this);
            return;
        }

        var key = soundLibraryKey.Trim();
        if (m_AttachToThis)
            mgr.TryPlayOneShotAttached(key, gameObject);
        else
            mgr.TryPlayOneShot(key, transform.position);
    }

    /// <summary>
    /// Animation Event target: call on the frame the item should appear on the table.
    /// Invokes <see cref="ItemPlacedOnTable"/>, <see cref="onItemPlacedOnTable"/>, and
    /// <see cref="DialogueItemUnhide.RevealTableItem"/> on the scene instance.
    /// </summary>
    public void NotifyItemPlacedOnTable()
    {
        ItemPlacedOnTable?.Invoke();
        onItemPlacedOnTable?.Invoke();

        DialogueItemUnhide unhide = DialogueItemUnhide.Instance;
        if (unhide != null)
            unhide.RevealTableItem();
    }

    // --- Animation Event targets (SoundLibrary keys) ---

    public void PlayAccident() => Play("accident");
    public void PlayAddCoin() => Play("addCoin");
    public void PlayAddDetergent() => Play("addDetergent");
    public void PlayCloseDoor() => Play("closeDoor");
    public void PlayCloseDoorWashingMachine() => Play("CloseDoorWashingMachine");
    public void PlayClothLands() => Play("clothLands");
    public void PlayCoinDrop() => Play("coinDrop");
    public void PlayDetergentFailSound() => Play("detergentFailSound");
    public void PlayGiveItem() => Play("giveItem");
    public void PlayMusicAccent3() => Play("musicAccent_3");
    public void PlayOpenBackDoor() => Play("openBackDoor");
    public void PlayOpenDoorWashingMachine() => Play("openDoorWashingMachine");
    public void PlayOpenLid() => Play("openLid");
    public void PlayOpenMainDoor() => Play("openMainDoor");
    public void PlayPagerNextMessage() => Play("pagerNextMessage");
    public void PlayPutBasket() => Play("putBasket");
    public void PlayPutCloth() => Play("putCloth");
    public void PlayPutMoneyFail() => Play("putMoneyFail");
    public void PlayPutMoneyWin() => Play("putMoneyWin");
    public void PlayStartMachine() => Play("startMachine");
    public void PlayStepSounds() => Play("stepSounds");
    public void PlaySwtichPressed() => Play("swtichPressed");
}
