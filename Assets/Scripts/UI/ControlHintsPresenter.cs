using System.Collections;
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
///   3. First ring prompt  — optional centred prompt faded in for a few seconds the first
///      time the pager rings, teaching the Tab control.
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

    [Header("First Pager Ring Prompt")]
    [Tooltip("Optional centred prompt faded in the first time the pager rings. Leave empty to skip it.")]
    [SerializeField] private CanvasGroup firstRingPromptGroup;
    [SerializeField] private TMP_Text firstRingPromptLabel;

    [TextArea] [SerializeField] private string firstRingPromptText =
        "Your pager is buzzing\nPress [TAB] to read it";

    [Tooltip("Seconds the prompt stays fully visible, excluding the fades.")]
    [SerializeField] private float firstRingPromptHoldDuration = 4f;
    [SerializeField] private float firstRingPromptFadeDuration = 0.4f;

    [Header("References (optional — auto-resolved at runtime)")]
    [SerializeField] private InteractionSystem interactionSystem;
    [SerializeField] private IntroSequencePresenter introPresenter;

    // Cached last-applied values so we only touch TMP / SetActive when something changes.
    private bool _interactShown;
    private string _interactText;
    private bool _topRightShown;
    private string _topRightText;
    private Coroutine _firstRingRoutine;

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += OnLanguageChanged;
        PagerTextController.FirstRing += OnPagerFirstRing;
        // Start hidden; the first Update fills in the correct context.
        SetInteractHint(false, null);
        SetTopRightHint(false, null);
        HideFirstRingPrompt();
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= OnLanguageChanged;
        PagerTextController.FirstRing -= OnPagerFirstRing;
        StopFirstRingPrompt();
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

    // ------------------------------------------------------------------ First ring prompt

    private void OnPagerFirstRing()
    {
        if (firstRingPromptGroup == null || !isActiveAndEnabled)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsCutscenePlaying)
            return;

        StopFirstRingPrompt();
        _firstRingRoutine = StartCoroutine(FirstRingPromptRoutine());
    }

    private IEnumerator FirstRingPromptRoutine()
    {
        if (firstRingPromptLabel != null)
            firstRingPromptLabel.text = Loc(LocalizationKeys.HintPagerFirstRing, firstRingPromptText);

        firstRingPromptGroup.blocksRaycasts = false;
        firstRingPromptGroup.interactable = false;
        firstRingPromptGroup.alpha = 0f;
        firstRingPromptGroup.gameObject.SetActive(true);

        yield return FadeFirstRingPrompt(1f, firstRingPromptFadeDuration);

        float held = 0f;
        while (held < firstRingPromptHoldDuration && !ShouldDismissFirstRingPrompt())
        {
            held += Time.deltaTime;
            yield return null;
        }

        yield return FadeFirstRingPrompt(0f, firstRingPromptFadeDuration);

        HideFirstRingPrompt();
        _firstRingRoutine = null;
    }

    /// <summary>The prompt has done its job once the player opens the pager, and must not sit over a cutscene.</summary>
    private bool ShouldDismissFirstRingPrompt()
    {
        PagerTextController pager = PagerTextController.Instance;
        if (pager != null && pager.IsOpen)
            return true;

        return GameManager.Instance != null && GameManager.Instance.IsCutscenePlaying;
    }

    private IEnumerator FadeFirstRingPrompt(float targetAlpha, float duration)
    {
        float startAlpha = firstRingPromptGroup.alpha;
        if (duration <= 0f)
        {
            firstRingPromptGroup.alpha = targetAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            firstRingPromptGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        firstRingPromptGroup.alpha = targetAlpha;
    }

    private void StopFirstRingPrompt()
    {
        if (_firstRingRoutine == null)
            return;

        StopCoroutine(_firstRingRoutine);
        _firstRingRoutine = null;
        HideFirstRingPrompt();
    }

    private void HideFirstRingPrompt()
    {
        if (firstRingPromptGroup == null)
            return;

        firstRingPromptGroup.alpha = 0f;
        firstRingPromptGroup.gameObject.SetActive(false);
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
