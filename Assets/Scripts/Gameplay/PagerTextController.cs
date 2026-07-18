using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;

/// <summary>
/// Pager inbox for Jason conversations. Tab opens/closes (locks movement while open; look stays free).
/// Space advances messages forward only. Arrows scroll the visible window.
/// Conversation stays until a new one replaces it. "no messages" when fully read.
/// Prop screen shows "new message" until the player finishes reading the thread.
/// Respond-support mode: after the inbound message, Space shows "start typing"; any key
/// types a canned reply; finishing plays the completion ending and completes the knot.
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

    [Header("Hardware")]
    public Animator animator;
    public GameObject truePager;
    public List<GameObject> propPagers = new();

    [Header("Screen")]
    [SerializeField] private TextMeshPro screenText;
    [SerializeField] private int visibleCharacterCount = 16;
    [SerializeField] private string emptyInboxText = "no messages";

    [Header("Prop Screen")]
    [Tooltip("World/prop pager display (visible while the true pager is closed).")]
    [SerializeField] private TextMeshPro propScreenText;
    [SerializeField] private string unreadPropText = "new message";
    [SerializeField] private string blankPropText = "";

    [Header("Respond Support")]
    [SerializeField] private string startTypingText = "start typing";
    [SerializeField] private string respondSupportReply =
        "I'm done. The wash cycles should be finished in a few minutes.";

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;

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

    Story _story;
    string _knotName;
    Action<string> _onConversationComplete;

    public bool IsOpen => _isOpen;
    public bool HasConversation => _hasConversation;
    public bool IsWaitingForChoice => _waitingForChoice && !_respondSupportMode;
    public bool IsRespondSupportMode => _respondSupportMode;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PagerTextController: more than one instance.", this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        if (truePager != null)
            truePager.SetActive(false);

        _hasUnreadMessage = false;
        RefreshDisplay();
        RefreshPropDisplay();
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
        // During respond-support, Tab is the only way out of the open pager.
        if (Input.GetKeyDown(toggleKey))
            TogglePager();

        if (!_isOpen)
            return;

        if (_respondSupportMode && HandleRespondSupportInput())
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            ScrollLeft();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ScrollRight();

        if (_waitingForChoice)
            return;

        if (Input.GetKeyDown(advanceKey))
            AdvanceMessage();
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

        if (_respondSupportMode)
        {
            // Choice buttons are replaced by the typing reply flow.
            _waitingForChoice = false;
            _respondPhase = RespondPhase.ReadingInbound;
            _hasUnreadMessage = _messages.Count > 0;
            RefreshDisplay();
            RefreshPropDisplay();
            PokePager();
            return true;
        }

        _hasUnreadMessage = _messages.Count > 0 || _waitingForChoice;
        RefreshDisplay();
        RefreshPropDisplay();
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

        RefreshDisplay();
        RefreshPropDisplay();
    }

    [ContextMenu("Toggle Pager")]
    public void TogglePager()
    {
        if (_isOpen)
            ClosePager();
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

        _isOpen = true;
        ApplyPagerVisuals(true);
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

        _isOpen = false;
        ApplyPagerVisuals(false);

        if (animator != null)
            animator.SetTrigger("toggle");

        if (GameStateManager.CurrentState == GameState.Pager)
            GameStateManager.ChangeState(GameState.Gameplay);
    }

    public void PokePager()
    {
        if (animator != null)
            animator.SetTrigger("poke");
    }

    [ContextMenu("Scroll Left")]
    public void ScrollLeft()
    {
        PokePager();
        _scrollIndex = Mathf.Max(0, _scrollIndex - visibleCharacterCount);
        RefreshDisplay();
    }

    [ContextMenu("Scroll Right")]
    public void ScrollRight()
    {
        PokePager();
        _scrollIndex = Mathf.Min(GetMaxScrollIndex(), _scrollIndex + visibleCharacterCount);
        RefreshDisplay();
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
    /// Returns true when respond-support consumed this frame's input (skip normal advance/scroll).
    /// </summary>
    bool HandleRespondSupportInput()
    {
        switch (_respondPhase)
        {
            case RespondPhase.ReadingInbound:
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    ScrollLeft();
                    return true;
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    ScrollRight();
                    return true;
                }

                if (Input.GetKeyDown(advanceKey))
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

        string reply = respondSupportReply ?? "";
        if (_typedCharCount >= reply.Length)
            return true;

        _typedCharCount++;
        _scrollIndex = Mathf.Max(0, _typedCharCount - visibleCharacterCount);
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
                return;
            }
        }

        if (!_completionFired)
            CompleteConversation();

        // Terminal view until the pager is closed / reopened for review.
        _messageIndex = _messages.Count;
        MarkConversationRead();
        RefreshDisplay();
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

    void RefreshPropDisplay()
    {
        if (propScreenText == null)
            return;

        propScreenText.text = _hasUnreadMessage ? unreadPropText : blankPropText;
    }

    void ForceCloseWithoutUnlock()
    {
        if (!_isOpen)
            return;

        _isOpen = false;
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
                    return startTypingText;

                case RespondPhase.TypingReply:
                case RespondPhase.Finished:
                    string reply = respondSupportReply ?? "";
                    return reply.Substring(0, Mathf.Clamp(_typedCharCount, 0, reply.Length));

                case RespondPhase.ReadingInbound:
                    break;
            }
        }

        if (!_hasConversation || _messages.Count == 0)
            return emptyInboxText;

        if (_messageIndex >= _messages.Count)
            return emptyInboxText;

        return _messages[_messageIndex];
    }

    int GetMaxScrollIndex()
    {
        string message = GetCurrentDisplaySource();
        if (string.IsNullOrEmpty(message))
            return 0;

        return Mathf.Max(0, message.Length - visibleCharacterCount);
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

            screenText.text = emptyInboxText;
            return;
        }

        _scrollIndex = Mathf.Clamp(_scrollIndex, 0, GetMaxScrollIndex());
        int length = Mathf.Min(visibleCharacterCount, message.Length - _scrollIndex);
        if (length <= 0)
        {
            screenText.text = emptyInboxText;
            return;
        }

        screenText.text = message.Substring(_scrollIndex, length);
    }
}
