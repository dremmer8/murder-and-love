using UnityEngine;

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
