using TMPro;
using UnityEngine;

/// <summary>
/// Context-sensitive control hints HUD. Watches the current game context every frame and
/// drives two configurable hint widgets (set up your own TMP GUI and drag them in):
///
///   1. Interaction hint  — shown in normal gameplay when the crosshair is on a usable
///      (unblocked) Interactable / DialogueTrigger. "[E] Interact" / "[E] Talk".
///   2. Control hint      — exclusive top-right panel for the current mode:
///        • normal gameplay .......... "[TAB] Check pager"
///        • minigame active .......... per-minigame ControlHints (fallback below)
///        • pager open ............... A/D scroll, D at end next, Tab put down
///        • pager first-open tutorial  scroll A/D, then D at end (Tab locked until done)
///        • pager respond (reading) .. A/D scroll, D at end continue, Tab put down
///        • pager respond (typing) ... any key types the reply, Tab put down
///        • dialogue / intro ......... Space continue, or mouse to choose options
///
/// Each widget is a root GameObject (toggled on/off) plus a TMP label whose text this
/// component sets. Position/style them however you like in your Canvas.
/// </summary>
public class ControlHintsPresenter : MonoBehaviour
{
    [Header("Interaction Hint (aim at interactable / dialogue)")]
    [Tooltip("Root toggled on when the crosshair is on a usable interactable / dialogue trigger.")]
    [SerializeField] private GameObject interactHintRoot;
    [SerializeField] private TMP_Text interactHintLabel;
    [SerializeField] private string interactableHintText = "[E] Interact";
    [SerializeField] private string dialogueHintText = "[E] Talk";

    [Header("Control Hint Panel")]
    [Tooltip("Root toggled on for pager / minigame / dialogue / check-pager contexts. Place it top-right.")]
    [SerializeField] private GameObject topRightHintRoot;
    [SerializeField] private TMP_Text topRightHintLabel;

    [Tooltip("Shown in normal gameplay to tell the player they can open the pager.")]
    [TextArea] [SerializeField] private string checkPagerHint = "[TAB] Check pager";

    [Tooltip("If true, only show the check-pager hint once a conversation is waiting in the inbox.")]
    [SerializeField] private bool onlyShowCheckPagerWhenConversation = false;

    [Tooltip("Fallback minigame how-to-play text when the active MinigameActivator has no ControlHints.")]
    [TextArea] [SerializeField] private string minigameFallbackHint =
        "A / D — turn dial\nMouse — grab & place\n[ESC] Leave";

    [TextArea] [SerializeField] private string pagerOpenHint =
        "A / D — scroll\n[D] at end — next message\n[TAB] Put down pager";

    [TextArea] [SerializeField] private string pagerTutorialScrollHint =
        "[A] / [D] Scroll left & right";

    [TextArea] [SerializeField] private string pagerTutorialAdvanceHint =
        "[D] at end — next message";

    [TextArea] [SerializeField] private string pagerRespondReadingHint =
        "A / D — scroll\n[D] at end — continue / reply\n[TAB] Put down pager";

    [TextArea] [SerializeField] private string pagerRespondTypingHint =
        "Type on any key to reply\n[TAB] Put down pager";

    [TextArea] [SerializeField] private string pagerRespondTutorialScrollHint =
        "[A] / [D] Scroll left & right";

    [TextArea] [SerializeField] private string pagerRespondTutorialAdvanceHint =
        "[D] at end — continue / reply";

    [TextArea] [SerializeField] private string dialogueProgressHint =
        "[SPACE] Continue\nMouse — choose options";

    [TextArea] [SerializeField] private string dialogueChoiceHint =
        "Mouse — choose options";

    [Header("References (optional — auto-resolved at runtime)")]
    [SerializeField] private InteractionSystem interactionSystem;
    [SerializeField] private IntroSequencePresenter introPresenter;

    // Cached last-applied values so we only touch TMP / SetActive when something changes.
    private bool _interactShown;
    private string _interactText;
    private bool _topRightShown;
    private string _topRightText;

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += OnLanguageChanged;
        // Start hidden; the first Update fills in the correct context.
        SetInteractHint(false, null);
        SetTopRightHint(false, null);
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    void OnLanguageChanged()
    {
        // Force re-apply on next Update even if context text is unchanged in English.
        _interactText = null;
        _topRightText = null;
    }

    string Loc(string key, string fallback) => LocalizationService.Get(key, fallback);

    private void Update()
    {
        // Intro cinematic + ending cutscenes (including dialogue-over-cutscene endings).
        if (GameManager.Instance != null && GameManager.Instance.IsCutscenePlaying)
        {
            SetInteractHint(false, null);
            SetTopRightHint(false, null);
            return;
        }

        UpdateInteractHint();
        UpdateTopRightHint();
    }

    // ------------------------------------------------------------------ Interaction

    private void UpdateInteractHint()
    {
        // Only during free-roam gameplay, never mid-minigame / dialogue / pager.
        if (GameStateManager.CurrentState != GameState.Gameplay || MinigameActivator.IsAnyActive)
        {
            SetInteractHint(false, null);
            return;
        }

        InteractionSystem interaction = ResolveInteractionSystem();
        if (interaction == null)
        {
            SetInteractHint(false, null);
            return;
        }

        // Dialogue wins ties, matching InteractionSystem's own tie-break.
        if (interaction.TryGetAimedDialogueTrigger(out DialogueTrigger dialogue)
            && dialogue != null && dialogue.CanStartFromLook())
        {
            SetInteractHint(true, Loc(LocalizationKeys.HintTalk, dialogueHintText));
            return;
        }

        if (interaction.TryGetAimedInteractable(out Interactable interactable) && interactable != null)
        {
            SetInteractHint(true, Loc(LocalizationKeys.HintInteract, interactableHintText));
            return;
        }

        SetInteractHint(false, null);
    }

    // ------------------------------------------------------------------ Control hint panel

    private void UpdateTopRightHint()
    {
        PagerTextController pager = PagerTextController.Instance;

        // 1. Pager open — scrolling / reading / responding.
        if (pager != null && pager.IsOpen)
        {
            if (pager.IsRespondTyping)
            {
                SetTopRightHint(true, Loc(LocalizationKeys.HintPagerRespondTyping, pagerRespondTypingHint));
                return;
            }

            switch (pager.CurrentTutorialHintStep)
            {
                case PagerTextController.TutorialHintStep.Scroll:
                    SetTopRightHint(true,
                        pager.IsRespondReadingInbound
                            ? Loc(LocalizationKeys.HintPagerRespondTutorialScroll, pagerRespondTutorialScrollHint)
                            : Loc(LocalizationKeys.HintPagerTutorialScroll, pagerTutorialScrollHint));
                    return;

                case PagerTextController.TutorialHintStep.Advance:
                    SetTopRightHint(true,
                        pager.IsRespondReadingInbound
                            ? Loc(LocalizationKeys.HintPagerRespondTutorialAdvance, pagerRespondTutorialAdvanceHint)
                            : Loc(LocalizationKeys.HintPagerTutorialAdvance, pagerTutorialAdvanceHint));
                    return;
            }

            if (pager.IsRespondReadingInbound)
                SetTopRightHint(true, Loc(LocalizationKeys.HintPagerRespondReading, pagerRespondReadingHint));
            else
                SetTopRightHint(true, Loc(LocalizationKeys.HintPagerOpen, pagerOpenHint));
            return;
        }

        // 2. Dialogue / intro — replace usual gameplay hints while locked in conversation.
        if (TryGetDialogueControlHint(out string dialogueHint))
        {
            SetTopRightHint(true, dialogueHint);
            return;
        }

        // Below here only applies to free-roam gameplay.
        if (GameStateManager.CurrentState != GameState.Gameplay)
        {
            SetTopRightHint(false, null);
            return;
        }

        // 3. Minigame active — per-minigame how-to-play.
        if (MinigameActivator.IsAnyActive)
        {
            MinigameActivator active = MinigameActivator.ActiveInstance;
            string hint = active != null && !string.IsNullOrEmpty(active.ControlHints)
                ? active.ControlHints
                : Loc(LocalizationKeys.HintMinigameFallback, minigameFallbackHint);
            SetTopRightHint(true, hint);
            return;
        }

        // 4. Normal gameplay — offer the pager.
        bool showCheckPager = pager != null
            && (!onlyShowCheckPagerWhenConversation || pager.HasConversation);
        SetTopRightHint(showCheckPager,
            showCheckPager ? Loc(LocalizationKeys.HintCheckPager, checkPagerHint) : null);
    }

    private bool TryGetDialogueControlHint(out string hint)
    {
        hint = null;

        DialogueManager dialogue = DialogueManager.GetInstance();
        if (dialogue != null
            && dialogue.dialogueIsPlaying
            && dialogue.ActiveMode == DialoguePresentationMode.Standard
            && GameStateManager.CurrentState == GameState.Dialogue)
        {
            hint = dialogue.IsChoosing
                ? Loc(LocalizationKeys.HintDialogueChoice, dialogueChoiceHint)
                : Loc(LocalizationKeys.HintDialogueProgress, dialogueProgressHint);
            return true;
        }

        IntroSequencePresenter intro = ResolveIntroPresenter();
        if (intro != null && intro.IsActive)
        {
            hint = intro.IsChoosing
                ? Loc(LocalizationKeys.HintDialogueChoice, dialogueChoiceHint)
                : Loc(LocalizationKeys.HintDialogueProgress, dialogueProgressHint);
            return true;
        }

        return false;
    }

    // ------------------------------------------------------------------ Widget helpers

    private void SetInteractHint(bool show, string text)
    {
        if (show == _interactShown && text == _interactText)
            return;

        _interactShown = show;
        _interactText = text;

        if (interactHintLabel != null && show)
            interactHintLabel.text = text;

        if (interactHintRoot != null)
            interactHintRoot.SetActive(show);
    }

    private void SetTopRightHint(bool show, string text)
    {
        if (show == _topRightShown && text == _topRightText)
            return;

        _topRightShown = show;
        _topRightText = text;

        if (topRightHintLabel != null && show)
            topRightHintLabel.text = text;

        if (topRightHintRoot != null)
            topRightHintRoot.SetActive(show);
    }

    private InteractionSystem ResolveInteractionSystem()
    {
        if (interactionSystem == null)
            interactionSystem = InteractionSystem.Instance;
        return interactionSystem;
    }

    private IntroSequencePresenter ResolveIntroPresenter()
    {
        if (introPresenter == null)
            introPresenter = FindFirstObjectByType<IntroSequencePresenter>();
        return introPresenter;
    }
}
