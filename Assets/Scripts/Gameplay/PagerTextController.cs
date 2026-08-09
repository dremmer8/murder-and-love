using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using Ink.Runtime;

/// <summary>
/// Pager inbox for Jason conversations. Tab opens/closes (locks movement while open; look stays free).
/// A/D scroll the visible window. Pressing D again at the end of a message advances forward.
/// Conversation stays until a new one replaces it. "no messages" when fully read.
/// Prop screen shows "new message" until the player finishes reading the thread.
/// Respond-support mode: after the inbound message, D at end of scroll shows "start typing";
/// any key types a canned reply; finishing plays the completion ending and completes the knot.
/// First inbound thread runs a one-shot tutorial: long messages require A then D before
/// advancing works, and Tab cannot put the pager down until that step is done.
/// </summary>
public class PagerTextController : MonoBehaviour
{
    public static PagerTextController Instance { get; private set; }

    enum RespondPhase
    {
        None,
        ReadingInbound,
        StartTypingPrompt,
        TypingReply,
        Finished
    }

    /// <summary>First-open tutorial step used by control hints.</summary>
    public enum TutorialHintStep
    {
        None,
        Scroll,
        Advance
    }

    [Header("Hardware")]
    public Animator animator;
    public GameObject truePager;
    public List<GameObject> propPagers = new();

    [Header("Screen")]
    [SerializeField] private TextMeshPro screenText;
    [Tooltip("Screen width measured in Latin character widths. Full-width (CJK) glyphs take two.")]
    [SerializeField] private int visibleCharacterCount = 16;
    [SerializeField] private string emptyInboxText = "no messages";

    [Header("Prop Screen")]
    [Tooltip("World/prop pager displays (visible while the true pager is closed).")]
    [SerializeField] private List<TextMeshPro> propScreenTexts = new();
    [SerializeField] private string unreadPropText = "new message";
    [SerializeField] private string blankPropText = "";

    [Header("Respond Support")]
    [SerializeField] private string startTypingText = "start typing";
    [SerializeField] private string respondSupportReply =
        "I'm done. The wash cycles should be finished in a few minutes.";

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode scrollLeftKey = KeyCode.A;
    [SerializeField] private KeyCode scrollRightKey = KeyCode.D;

    readonly List<string> _messages = new();
    int _messageIndex;
    int _scrollIndex;
    bool _isOpen;
    bool _hasConversation;
    bool _waitingForChoice;
    bool _completionFired;
    bool _hasUnreadMessage;

    bool _respondSupportMode;
    RespondPhase _respondPhase = RespondPhase.None;
    int _typedCharCount;

    // One-shot tutorial for the first pager conversation only.
    bool _hasCompletedFirstPagerTutorial;
    bool _tutorialActive;
    bool _tutorialScrolledLeft;
    bool _tutorialScrolledRight;

    Story _story;
    string _knotName;
    Action<string> _onConversationComplete;
    EventInstance _newMessageInstance;

    const string ActParam = "act";
    const string CloseStateName = "close";
    const string LayDownStateName = "Lay down";
    const int Act1Progression = 14;
    const int Act2Progression = 21;
    int _lastSeenActProgression = int.MinValue;
    float _desiredAct;
    Coroutine _putDownVisibilityRoutine;

    public bool IsOpen => _isOpen;
    public bool HasConversation => _hasConversation;
    public bool HasUnreadMessage => _hasUnreadMessage;
    public bool IsWaitingForChoice => _waitingForChoice && !_respondSupportMode;
    public bool IsRespondSupportMode => _respondSupportMode;

    /// <summary>
    /// True pager + prop screen TMP labels (world-space). Used so localization can leave their custom font alone.
    /// </summary>
    public void CollectScreenLabels(List<TMP_Text> into)
    {
        if (into == null)
            return;

        if (screenText != null)
            into.Add(screenText);

        if (propScreenTexts == null)
            return;

        for (int i = 0; i < propScreenTexts.Count; i++)
        {
            TextMeshPro prop = propScreenTexts[i];
            if (prop != null)
                into.Add(prop);
        }
    }

    /// <summary>Respond-support: reading the inbound thread (A/D scroll; D at end continues / starts the reply).</summary>
    public bool IsRespondReadingInbound => _respondSupportMode && _respondPhase == RespondPhase.ReadingInbound;

    /// <summary>Respond-support: the "start typing" / typing phase (any key types the reply).</summary>
    public bool IsRespondTyping => _respondSupportMode
        && (_respondPhase == RespondPhase.StartTypingPrompt || _respondPhase == RespondPhase.TypingReply);

    /// <summary>True while the first-message tutorial is still gating input.</summary>
    public bool IsFirstOpenTutorialActive => _tutorialActive;

    /// <summary>Advance (D at end of scroll) must wait until A and D have both been used on a long first message.</summary>
    public bool TutorialBlocksAdvance => _tutorialActive && RequiresTutorialScroll();

    /// <summary>Tab cannot put the pager down until the first-open scroll tutorial is finished.</summary>
    public bool TutorialBlocksLeave => _tutorialActive;

    /// <summary>Hint step for the first-open tutorial (None once completed / not active).</summary>
    public TutorialHintStep CurrentTutorialHintStep
    {
        get
        {
            if (!_tutorialActive)
                return TutorialHintStep.None;
            if (RequiresTutorialScroll())
                return TutorialHintStep.Scroll;
            return TutorialHintStep.Advance;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PagerTextController: more than one instance.", this);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        LocalizationService.LanguageChanged += OnLanguageChanged;
    }

    void OnDisable()
    {
        LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    void OnDestroy()
    {
        LocalizationService.LanguageChanged -= OnLanguageChanged;
        StopNewMessageSound();

        if (Instance == this)
            Instance = null;
    }

    void OnLanguageChanged()
    {
        RefreshDisplay();
        RefreshPropDisplay();
    }

    string LocEmptyInbox() =>
        LocalizationService.Get(LocalizationKeys.PagerEmpty, emptyInboxText);

    string LocUnreadProp() =>
        LocalizationService.Get(LocalizationKeys.PagerNewMessage, unreadPropText);

    string LocStartTyping() =>
        LocalizationService.Get(LocalizationKeys.PagerStartTyping, startTypingText);

    string LocRespondReply() =>
        LocalizationService.Get(LocalizationKeys.PagerRespondReply, respondSupportReply);

    void Start()
    {
        if (truePager != null)
            truePager.SetActive(false);

        _hasUnreadMessage = false;
        RefreshDisplay();
        RefreshPropDisplay();
        SyncActFromProgression(force: true);
    }

    void OnValidate()
    {
        visibleCharacterCount = Mathf.Max(1, visibleCharacterCount);
        _scrollIndex = Mathf.Clamp(_scrollIndex, 0, GetMaxScrollIndex());
        RefreshDisplay();
        RefreshPropDisplay();
    }

    void Update()
    {
        SyncActFromProgression();

        // During respond-support, Tab is the only way out of the open pager.
        if (Input.GetKeyDown(toggleKey))
        {
            // Opening/closing the pager mid-minigame steals GameState and can abort
            // thoughts tied to the minigame sequence (softlock).
            if (MinigameActivator.IsAnyActive)
                return;

            if (_isOpen && TutorialBlocksLeave)
                return;

            TogglePager();
        }

        if (!_isOpen)
            return;

        if (_respondSupportMode && HandleRespondSupportInput())
            return;

        if (Input.GetKeyDown(scrollLeftKey))
            ScrollLeft();

        if (Input.GetKeyDown(scrollRightKey))
            ScrollRight();
    }

    /// <summary>
    /// Starts (or replaces) a Jason conversation from an Ink story already at the target knot.
    /// </summary>
    public bool BeginConversation(Story story, string knotName, Action<string> onComplete)
    {
        return BeginConversation(story, knotName, onComplete, respondSupportMode: false);
    }

    public bool BeginConversation(Story story, string knotName, Action<string> onComplete, bool respondSupportMode)
    {
        if (story == null)
            return false;

        // New conversation overwrites the previous inbox.
        ForceCloseWithoutUnlock();
        ClearConversationState();

        _story = story;
        _knotName = knotName ?? "";
        _onConversationComplete = onComplete;
        _hasConversation = true;
        _completionFired = false;
        _messageIndex = 0;
        _scrollIndex = 0;
        _respondSupportMode = respondSupportMode;
        _respondPhase = RespondPhase.None;
        _typedCharCount = 0;

        CollectLinesUntilPause();
        BeginFirstOpenTutorialIfNeeded();

        if (_respondSupportMode)
        {
            // Choice buttons are replaced by the typing reply flow.
            _waitingForChoice = false;
            _respondPhase = RespondPhase.ReadingInbound;
            _hasUnreadMessage = _messages.Count > 0;
            RefreshDisplay();
            RefreshPropDisplay();
            if (_hasUnreadMessage && !IsEndingCutscenePlaying())
                StartNewMessageSound();
            PokePager();
            return true;
        }

        _hasUnreadMessage = _messages.Count > 0 || _waitingForChoice;
        RefreshDisplay();
        RefreshPropDisplay();
        if (_hasUnreadMessage && !IsEndingCutscenePlaying())
            StartNewMessageSound();
        PokePager();

        // Story progression / knot completion happens when the thread arrives.
        // The inbox remains readable (and re-openable) until Jason replaces it.
        if (!_waitingForChoice)
            CompleteConversation();

        return true;
    }

    public void SetMessage(string text)
    {
        _messages.Clear();
        _hasConversation = !string.IsNullOrEmpty(text);
        _messageIndex = 0;
        _scrollIndex = 0;
        _waitingForChoice = false;
        _respondSupportMode = false;
        _respondPhase = RespondPhase.None;
        _typedCharCount = 0;
        _hasUnreadMessage = _hasConversation;

        if (_hasConversation)
            _messages.Add(text);

        BeginFirstOpenTutorialIfNeeded();
        RefreshDisplay();
        RefreshPropDisplay();
    }

    /// <summary>
    /// Ending cutscenes: stop the new-message loop and clear any inbox that chained in
    /// from Mandy smoking (Jason pager) so audio/UI do not play over the finale.
    /// </summary>
    public void SuppressForEndingCutscene()
    {
        StopNewMessageSound();

        if (_isOpen)
            ForceCloseWithoutUnlock();

        ClearConversationState();
        _hasUnreadMessage = false;
        RefreshDisplay();
        RefreshPropDisplay();
    }

    [ContextMenu("Toggle Pager")]
    public void TogglePager()
    {
        if (_isOpen)
        {
            if (TutorialBlocksLeave)
                return;

            ClosePager();
        }
        else
            OpenPager();
    }

    public void OpenPager()
    {
        if (_isOpen)
            return;

        // Don't fight other locking dialogue UIs.
        if (GameStateManager.CurrentState == GameState.Dialogue)
            return;

        if (MinigameActivator.IsAnyActive)
            return;

        StopNewMessageSound();
        CancelPutDownVisibilitySwap();

        _isOpen = true;
        ApplyPagerVisuals(true);
        // Animator lives on the true pager, which is inactive while closed — re-push act now.
        SyncActFromProgression(force: true);
        GameStateManager.ChangeState(GameState.Pager);

        if (animator != null)
            animator.SetTrigger("toggle");

        // After a finished read, reopening lets the player review the thread
        // until Jason sends a new conversation.
        if (!_respondSupportMode
            && _hasConversation
            && _messages.Count > 0
            && _messageIndex >= _messages.Count)
            _messageIndex = 0;

        _scrollIndex = 0;
        RefreshDisplay();
    }

    public void ClosePager()
    {
        if (!_isOpen)
            return;

        // First-open tutorial keeps the player in the pager until scroll / advance practice is done.
        if (TutorialBlocksLeave)
            return;

        _isOpen = false;

        if (animator != null)
        {
            animator.SetTrigger("toggle");
            // Keep the true pager visible through the reverse put-down clip;
            // swap to prop pagers once "close" finishes into "Lay down".
            CancelPutDownVisibilitySwap();
            _putDownVisibilityRoutine = StartCoroutine(WaitForPutDownThenSwapVisuals());
        }
        else
        {
            ApplyPagerVisuals(false);
        }

        if (GameStateManager.CurrentState == GameState.Pager)
            GameStateManager.ChangeState(GameState.Gameplay);
    }

    public void PokePager()
    {
        if (animator != null)
            animator.SetTrigger("poke");
    }

    /// <summary>
    /// Drives the pager blend-tree <c>act</c> float from <see cref="GlobalVariableOperator.GameProgression"/>:
    /// 1 at progression &gt;= 14, 2 at &gt;= 21, otherwise 0.
    /// Caches the desired value even while the animator GO is inactive (true pager is off when closed),
    /// and only calls SetFloat when the animator is active so the parameter actually sticks.
    /// </summary>
    void SyncActFromProgression(bool force = false)
    {
        int progression = GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;

        if (force || progression != _lastSeenActProgression)
        {
            _lastSeenActProgression = progression;
            _desiredAct = 0f;
            if (progression >= Act2Progression)
                _desiredAct = 2f;
            else if (progression >= Act1Progression)
                _desiredAct = 1f;
        }

        ApplyDesiredAct();
    }

    void ApplyDesiredAct()
    {
        if (animator == null || !animator.isActiveAndEnabled)
            return;

        animator.SetFloat(ActParam, _desiredAct);
    }

    [ContextMenu("Scroll Left")]
    public void ScrollLeft()
    {
        PokePager();
        _scrollIndex = GetPreviousScrollIndex();
        if (_tutorialActive)
            _tutorialScrolledLeft = true;
        RefreshDisplay();
        TryFinishTutorialAfterScrollPractice();
    }

    [ContextMenu("Scroll Right")]
    public void ScrollRight()
    {
        int maxScroll = GetMaxScrollIndex();

        // Already at the end of this message — D advances to the next one.
        if (_scrollIndex >= maxScroll)
        {
            if (TutorialBlocksAdvance)
                return;

            TryAdvanceFromScrollEnd();
            return;
        }

        PokePager();
        int pageLength = Mathf.Max(1, CountCharsThatFit(GetCurrentDisplaySource(), _scrollIndex));
        _scrollIndex = Mathf.Min(maxScroll, _scrollIndex + pageLength);
        if (_tutorialActive)
            _tutorialScrolledRight = true;
        RefreshDisplay();
        TryFinishTutorialAfterScrollPractice();
    }

    /// <summary>
    /// Called when D is pressed while already scrolled to the end of the current message.
    /// </summary>
    void TryAdvanceFromScrollEnd()
    {
        PokePager();

        if (_respondSupportMode && _respondPhase == RespondPhase.ReadingInbound)
        {
            if (_messageIndex < _messages.Count - 1)
            {
                _messageIndex++;
                _scrollIndex = 0;
                RefreshDisplay();
            }
            else
            {
                EnterStartTypingPrompt();
            }

            CompleteFirstOpenTutorial();
            return;
        }

        if (_waitingForChoice)
            return;

        AdvanceMessage();
    }

    /// <summary>Called by DialogueManager when the player picks a pager choice.</summary>
    public void NotifyChoiceMade(int choiceIndex)
    {
        if (_respondSupportMode)
            return;

        if (!_waitingForChoice || _story == null)
            return;

        if (choiceIndex < 0 || choiceIndex >= _story.currentChoices.Count)
            return;

        if (GlobalVariableOperator.Instance != null)
            GlobalVariableOperator.Instance.RecordChoice(_story.currentChoices[choiceIndex].text);

        _story.ChooseChoiceIndex(choiceIndex);
        _waitingForChoice = false;

        int previousCount = _messages.Count;
        CollectLinesUntilPause();

        if (_messages.Count > previousCount)
            _messageIndex = previousCount;
        else if (_messages.Count > 0 && _messageIndex >= _messages.Count)
            _messageIndex = _messages.Count - 1;

        _scrollIndex = 0;
        RefreshDisplay();

        if (!_waitingForChoice && _story != null && !_story.canContinue && _story.currentChoices.Count == 0)
            CompleteConversation();
    }

    public IReadOnlyList<Choice> GetPendingChoices()
    {
        if (_respondSupportMode || !_waitingForChoice || _story == null)
            return Array.Empty<Choice>();

        return _story.currentChoices;
    }

    /// <summary>
    /// Returns true when respond-support consumed this frame's input (skip normal scroll).
    /// </summary>
    bool HandleRespondSupportInput()
    {
        switch (_respondPhase)
        {
            case RespondPhase.ReadingInbound:
                if (Input.GetKeyDown(scrollLeftKey))
                {
                    ScrollLeft();
                    return true;
                }

                if (Input.GetKeyDown(scrollRightKey))
                {
                    ScrollRight();
                    return true;
                }

                return true;

            case RespondPhase.StartTypingPrompt:
            case RespondPhase.TypingReply:
                // Tab is handled above for leaving. Every other key types the canned reply.
                if (Input.GetKeyDown(toggleKey))
                    return true;

                if (TryConsumeTypingKey())
                    return true;

                return true;

            case RespondPhase.Finished:
                // Stay on the typed reply until Tab closes; no further input advances.
                return true;

            default:
                return false;
        }
    }

    bool TryConsumeTypingKey()
    {
        if (!AnyNonToggleKeyDown())
            return false;

        if (_respondPhase == RespondPhase.StartTypingPrompt)
        {
            _respondPhase = RespondPhase.TypingReply;
            _typedCharCount = 0;
        }

        string reply = LocRespondReply() ?? "";
        if (_typedCharCount >= reply.Length)
            return true;

        _typedCharCount++;
        // The display source is the typed prefix, so its last page keeps the caret on screen.
        _scrollIndex = GetMaxScrollIndex();
        RefreshDisplay();
        PokePager();

        if (_typedCharCount >= reply.Length)
            FinishRespondSupport();

        return true;
    }

    static readonly KeyCode[] ModifierKeys =
    {
        KeyCode.LeftShift, KeyCode.RightShift,
        KeyCode.LeftControl, KeyCode.RightControl,
        KeyCode.LeftAlt, KeyCode.RightAlt,
        KeyCode.LeftCommand, KeyCode.RightCommand,
        KeyCode.CapsLock
    };

    static readonly KeyCode[] KeyboardKeys = BuildKeyboardKeys();

    static KeyCode[] BuildKeyboardKeys()
    {
        var keys = new List<KeyCode>();
        foreach (KeyCode key in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
        {
            if (key == KeyCode.None || key == KeyCode.Tab)
                continue;
            if ((int)key >= (int)KeyCode.Mouse0)
                continue;
            if (IsModifierKey(key))
                continue;
            keys.Add(key);
        }

        return keys.ToArray();
    }

    static bool AnyNonToggleKeyDown()
    {
        // Tab alone must not type — it only toggles the pager.
        if (Input.GetKeyDown(KeyCode.Tab))
            return false;

        for (int i = 0; i < KeyboardKeys.Length; i++)
        {
            if (Input.GetKeyDown(KeyboardKeys[i]))
                return true;
        }

        return false;
    }

    static bool IsModifierKey(KeyCode key)
    {
        for (int i = 0; i < ModifierKeys.Length; i++)
        {
            if (ModifierKeys[i] == key)
                return true;
        }

        return false;
    }

    void EnterStartTypingPrompt()
    {
        _respondPhase = RespondPhase.StartTypingPrompt;
        _typedCharCount = 0;
        _scrollIndex = 0;
        RefreshDisplay();
        PokePager();
    }

    void FinishRespondSupport()
    {
        if (_respondPhase == RespondPhase.Finished)
            return;

        _respondPhase = RespondPhase.Finished;
        MarkConversationRead();
        RefreshDisplay();

        // Leave the pager UI so the next dialogue / cutscene are not fighting Pager state.
        CompleteFirstOpenTutorial();
        ClosePager();

        if (GameManager.Instance != null)
            GameManager.Instance.PlayEndingCutscene(GameManager.CompletionEndingCinematicIndex);

        CompleteConversation();
    }

    void AdvanceMessage()
    {
        if (!_hasConversation)
        {
            RefreshDisplay();
            return;
        }

        if (_messages.Count == 0)
        {
            RefreshDisplay();
            return;
        }

        // Forward only through the thread.
        if (_messageIndex < _messages.Count - 1)
        {
            _messageIndex++;
            _scrollIndex = 0;
            RefreshDisplay();
            CompleteFirstOpenTutorial();
            return;
        }

        // On last message.
        if (_waitingForChoice)
            return;

        if (_story != null && (_story.canContinue || _story.currentChoices.Count > 0))
        {
            int previousCount = _messages.Count;
            CollectLinesUntilPause();

            if (_messages.Count > previousCount)
            {
                _messageIndex++;
                _scrollIndex = 0;
                RefreshDisplay();
                CompleteFirstOpenTutorial();
                return;
            }
        }

        if (!_completionFired)
            CompleteConversation();

        // Terminal view until the pager is closed / reopened for review.
        _messageIndex = _messages.Count;
        MarkConversationRead();
        SoundManager.PlayOneShot("pagerNoMessages");
        RefreshDisplay();
        CompleteFirstOpenTutorial();
    }

    void BeginFirstOpenTutorialIfNeeded()
    {
        if (_hasCompletedFirstPagerTutorial || _messages.Count == 0)
        {
            _tutorialActive = false;
            return;
        }

        _tutorialActive = true;
        _tutorialScrolledLeft = false;
        _tutorialScrolledRight = false;
    }

    bool CurrentMessageNeedsScroll()
    {
        return GetMaxScrollIndex() > 0;
    }

    bool RequiresTutorialScroll()
    {
        if (!_tutorialActive || !CurrentMessageNeedsScroll())
            return false;

        return !_tutorialScrolledLeft || !_tutorialScrolledRight;
    }

    /// <summary>
    /// Long first message: once A and D have both been used, unlock advance (D at end) and Tab.
    /// Short first message: keep Tab locked until the player advances once with D.
    /// </summary>
    void TryFinishTutorialAfterScrollPractice()
    {
        if (!_tutorialActive || !CurrentMessageNeedsScroll())
            return;

        if (_tutorialScrolledLeft && _tutorialScrolledRight)
            CompleteFirstOpenTutorial();
    }

    void CompleteFirstOpenTutorial()
    {
        if (!_tutorialActive)
            return;

        _tutorialActive = false;
        _hasCompletedFirstPagerTutorial = true;
    }

    void CollectLinesUntilPause()
    {
        if (_story == null)
            return;

        while (_story.canContinue)
        {
            string text = _story.Continue();
            while (string.IsNullOrWhiteSpace(text) && _story.canContinue)
                text = _story.Continue();

            if (!string.IsNullOrWhiteSpace(text))
                _messages.Add(text.Trim());
        }

        _waitingForChoice = _story.currentChoices.Count > 0;

        if (_messages.Count > 0 && _messageIndex >= _messages.Count)
            _messageIndex = _messages.Count - 1;
    }

    void CompleteConversation()
    {
        if (_completionFired)
            return;

        _completionFired = true;
        _waitingForChoice = false;

        if (GlobalVariableOperator.Instance != null && _story != null)
        {
            GlobalVariableOperator.Instance.SyncFromStory(_story);
            GlobalVariableOperator.Instance.UnbindStory();
        }

        string knot = _knotName;
        Action<string> callback = _onConversationComplete;
        _onConversationComplete = null;
        _story = null;

        callback?.Invoke(knot);
    }

    void ClearConversationState()
    {
        if (!_completionFired && _onConversationComplete != null && _story != null)
        {
            // Replaced before the player finished — still sync & complete so progression does not soft-lock.
            CompleteConversation();
        }

        _messages.Clear();
        _messageIndex = 0;
        _scrollIndex = 0;
        _hasConversation = false;
        _waitingForChoice = false;
        _completionFired = false;
        _respondSupportMode = false;
        _respondPhase = RespondPhase.None;
        _typedCharCount = 0;
        _story = null;
        _knotName = "";
        _onConversationComplete = null;
    }

    void MarkConversationRead()
    {
        if (!_hasUnreadMessage)
            return;

        _hasUnreadMessage = false;
        RefreshPropDisplay();
    }

    void StartNewMessageSound()
    {
        StopNewMessageSound();

        if (SoundManager.Instance == null)
            return;

        SoundManager.Instance.TryStartInstance("pagerNewMessage", out _newMessageInstance);
    }

    void StopNewMessageSound()
    {
        if (!_newMessageInstance.isValid())
            return;

        _newMessageInstance.stop(STOP_MODE.ALLOWFADEOUT);
        _newMessageInstance.release();
        _newMessageInstance.clearHandle();
    }

    static bool IsEndingCutscenePlaying()
    {
        if (GameManager.Instance == null)
            return false;

        // HasReachedEnding covers credits after the cutscene coroutine has already cleared.
        return GameManager.Instance.IsCutscenePlaying || GameManager.Instance.HasReachedEnding;
    }

    void RefreshPropDisplay()
    {
        if (propScreenTexts == null || propScreenTexts.Count == 0)
            return;

        string text = _hasUnreadMessage ? LocUnreadProp() : blankPropText;
        for (int i = 0; i < propScreenTexts.Count; i++)
        {
            if (propScreenTexts[i] != null)
                propScreenTexts[i].text = text;
        }
    }

    void ForceCloseWithoutUnlock()
    {
        if (!_isOpen && _putDownVisibilityRoutine == null)
            return;

        _isOpen = false;
        CancelPutDownVisibilitySwap();
        ApplyPagerVisuals(false);
    }

    void CancelPutDownVisibilitySwap()
    {
        if (_putDownVisibilityRoutine == null)
            return;

        StopCoroutine(_putDownVisibilityRoutine);
        _putDownVisibilityRoutine = null;
    }

    IEnumerator WaitForPutDownThenSwapVisuals()
    {
        // Let the toggle trigger take effect.
        yield return null;

        if (animator == null)
        {
            ApplyPagerVisuals(false);
            _putDownVisibilityRoutine = null;
            yield break;
        }

        // Wait until the reverse put-down state starts (or we already reached lay-down).
        float timeout = 1.5f;
        float elapsed = 0f;
        bool enteredClose = false;
        while (elapsed < timeout)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(CloseStateName))
            {
                enteredClose = true;
                break;
            }

            if (info.IsName(LayDownStateName))
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hold true-pager visibility until put-down leaves the close state.
        if (enteredClose)
        {
            while (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName(CloseStateName))
                yield return null;
        }

        _putDownVisibilityRoutine = null;

        if (!_isOpen)
            ApplyPagerVisuals(false);
    }

    void ApplyPagerVisuals(bool open)
    {
        if (truePager != null)
            truePager.SetActive(open);

        foreach (GameObject pager in propPagers)
        {
            if (pager != null)
                pager.SetActive(!open);
        }
    }

    string GetCurrentDisplaySource()
    {
        if (_respondSupportMode)
        {
            switch (_respondPhase)
            {
                case RespondPhase.StartTypingPrompt:
                    return LocStartTyping();

                case RespondPhase.TypingReply:
                case RespondPhase.Finished:
                    string reply = LocRespondReply() ?? "";
                    return reply.Substring(0, Mathf.Clamp(_typedCharCount, 0, reply.Length));

                case RespondPhase.ReadingInbound:
                    break;
            }
        }

        if (!_hasConversation || _messages.Count == 0)
            return LocEmptyInbox();

        if (_messageIndex >= _messages.Count)
            return LocEmptyInbox();

        return _messages[_messageIndex];
    }

    /// <summary>
    /// Screen columns taken by one character. Chinese/Japanese glyphs render at roughly double the
    /// width of a Latin one, so a raw character count would overflow the truncating pager screen.
    /// Punctuation shared with the Latin script (— ’ …) keeps its Latin width because the pager
    /// screen never swaps to the locale font.
    /// </summary>
    static int CharacterColumns(char c)
    {
        bool fullWidth =
            (c >= '\u1100' && c <= '\u115F')
            || (c >= '\u2E80' && c <= '\u303E')
            || (c >= '\u3041' && c <= '\u33FF')
            || (c >= '\u3400' && c <= '\u4DBF')
            || (c >= '\u4E00' && c <= '\u9FFF')
            || (c >= '\uA000' && c <= '\uA4CF')
            || (c >= '\uAC00' && c <= '\uD7A3')
            || (c >= '\uF900' && c <= '\uFAFF')
            || (c >= '\uFE10' && c <= '\uFE19')
            || (c >= '\uFE30' && c <= '\uFE6F')
            || (c >= '\uFF00' && c <= '\uFF60')
            || (c >= '\uFFE0' && c <= '\uFFE6');

        return fullWidth ? 2 : 1;
    }

    /// <summary>Characters from <paramref name="startIndex"/> that fit on one screen.</summary>
    int CountCharsThatFit(string message, int startIndex)
    {
        if (string.IsNullOrEmpty(message) || startIndex < 0 || startIndex >= message.Length)
            return 0;

        int columns = 0;
        int count = 0;

        for (int i = startIndex; i < message.Length; i++)
        {
            int nextColumns = columns + CharacterColumns(message[i]);

            // Always show at least one character, even if it alone exceeds the budget.
            if (nextColumns > visibleCharacterCount && count > 0)
                break;

            columns = nextColumns;
            count++;
        }

        return count;
    }

    /// <summary>Index of the last page, walking forward so pages line up in both directions.</summary>
    int GetMaxScrollIndex()
    {
        return GetPageStartBefore(GetCurrentDisplaySource(), int.MaxValue);
    }

    int GetPreviousScrollIndex()
    {
        return GetPageStartBefore(GetCurrentDisplaySource(), _scrollIndex);
    }

    /// <summary>
    /// Last page start strictly before <paramref name="limit"/>, or the final page when the whole
    /// message is walked. Pages are measured from index 0 so scrolling back lands on the same
    /// boundaries scrolling forward produced.
    /// </summary>
    int GetPageStartBefore(string message, int limit)
    {
        if (string.IsNullOrEmpty(message) || limit <= 0)
            return 0;

        int start = 0;
        while (true)
        {
            int pageLength = CountCharsThatFit(message, start);
            if (pageLength <= 0)
                return start;

            int next = start + pageLength;
            if (next >= message.Length || next >= limit)
                return start;

            start = next;
        }
    }

    void RefreshDisplay()
    {
        if (screenText == null)
            return;

        string message = GetCurrentDisplaySource();
        if (string.IsNullOrEmpty(message))
        {
            // Empty typed buffer still shows blank during typing start; otherwise inbox empty.
            if (_respondSupportMode && _respondPhase == RespondPhase.TypingReply)
            {
                screenText.text = "";
                return;
            }

            screenText.text = LocEmptyInbox();
            return;
        }

        _scrollIndex = Mathf.Clamp(_scrollIndex, 0, GetMaxScrollIndex());
        int length = CountCharsThatFit(message, _scrollIndex);
        if (length <= 0)
        {
            screenText.text = LocEmptyInbox();
            return;
        }

        screenText.text = message.Substring(_scrollIndex, length);
    }
}
