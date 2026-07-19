using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    [Header("Standard Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Presenters")]
    [SerializeField] private IntroSequencePresenter introPresenter;
    [SerializeField] private InternalMonologuePresenter internalPresenter;
    [SerializeField] private PagerTextController pagerController;
    [SerializeField] private DialogueTypewriter typewriter;

    [Header("Standard Input")]
    [SerializeField] private float inputDelay = 0.2f;

    private Story currentStory;
    private static DialogueManager instance;

    private bool isChoosing;
    private float nextInputTime;
    private string activeKnotName = "";
    private DialoguePresentationMode activeMode = DialoguePresentationMode.Standard;

    public bool dialogueIsPlaying { get; private set; }
    public DialoguePresentationMode ActiveMode => activeMode;

    /// <summary>True while choice buttons are shown and awaiting a click.</summary>
    public bool IsChoosing => isChoosing;

    /// <summary>Fired when a presentation finishes. Argument is the knot that was entered.</summary>
    public event Action<string> OnDialogueEnded;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogWarning("DialogueManager instances are missing or more than one in scene");
    }

    public static DialogueManager GetInstance() => instance;

    private void Start()
    {
        dialogueIsPlaying = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (choices == null)
            choices = Array.Empty<GameObject>();

        choicesText = new TextMeshProUGUI[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            choicesText[i] = choices[i].GetComponentInChildren<TextMeshProUGUI>();
            WireChoiceButton(choices[i], i);
        }
    }

    private void WireChoiceButton(GameObject choiceObject, int choiceIndex)
    {
        Button button = choiceObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"Choice UI '{choiceObject.name}' has no Button component.", choiceObject);
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => MakeChoice(choiceIndex));
    }

    private void Update()
    {
        if (activeMode == DialoguePresentationMode.Pager && pagerController != null)
        {
            UpdatePagerChoiceUi();
            return;
        }

        if (activeMode != DialoguePresentationMode.Standard)
            return;

        if (GameStateManager.CurrentState != GameState.Dialogue)
            return;

        if (Time.time < nextInputTime)
            return;

        if (!isChoosing && Input.GetKeyDown(KeyCode.Space))
        {
            DialogueTypewriter writer = ResolveTypewriter();
            if (writer != null && writer.IsTyping && writer.Skip())
                return;

            ContinueStandardStory();
        }
    }

    DialogueTypewriter ResolveTypewriter()
    {
        if (typewriter == null)
            typewriter = DialogueTypewriter.Instance;
        return typewriter;
    }

    public void EnterDialogue(TextAsset inkFile, string knotName = "")
    {
        EnterDialogue(inkFile, knotName, DialoguePresentationMode.Standard);
    }

    public void EnterDialogue(TextAsset inkFile, string knotName, DialoguePresentationMode mode)
    {
        EnterDialogue(inkFile, knotName, mode, respondSupportPager: false);
    }

    public void EnterDialogue(TextAsset inkFile, string knotName, DialoguePresentationMode mode, bool respondSupportPager)
    {
        if (inkFile == null)
        {
            Debug.LogWarning($"{name}: EnterDialogue called with null ink file.", this);
            return;
        }

        if (dialogueIsPlaying && mode != DialoguePresentationMode.Pager)
            return;

        // Pager can replace an existing Jason thread while the player is free.
        if (mode == DialoguePresentationMode.Pager && dialogueIsPlaying)
            return;

        currentStory = new Story(inkFile.text);
        activeKnotName = knotName ?? "";
        activeMode = mode;

        if (GlobalVariableOperator.Instance != null)
        {
            GlobalVariableOperator.Instance.ApplyVariablesToStory(currentStory);
            GlobalVariableOperator.Instance.BindStory(currentStory);
        }

        BindInkExternals(currentStory);

        if (!string.IsNullOrEmpty(knotName))
            currentStory.ChoosePathString(knotName);

        switch (mode)
        {
            case DialoguePresentationMode.Standard:
                BeginStandard();
                break;
            case DialoguePresentationMode.IntroSequence:
                BeginIntro();
                break;
            case DialoguePresentationMode.InternalMonologue:
                BeginInternal();
                break;
            case DialoguePresentationMode.Pager:
                BeginPager(respondSupportPager);
                break;
        }
    }

    void BindInkExternals(Story story)
    {
        if (story == null)
            return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
            gameManager.BindInkExternals(story);
        else
            Debug.LogWarning($"{name}: No GameManager found — PlayEndingCutscene will not be bound.", this);

        DialogueItemUnhide itemUnhide = DialogueItemUnhide.Instance;
        if (itemUnhide == null)
            itemUnhide = FindFirstObjectByType<DialogueItemUnhide>();

        if (itemUnhide != null)
            story.BindExternalFunction("UnhideItem", (string itemId) => itemUnhide.UnhideItem(itemId));
        else
            Debug.LogWarning($"{name}: No DialogueItemUnhide found — UnhideItem will not be bound.", this);

        DialogueItemGiveAway itemGiveAway = DialogueItemGiveAway.Instance;
        if (itemGiveAway == null)
            itemGiveAway = FindFirstObjectByType<DialogueItemGiveAway>();

        if (itemGiveAway != null)
            story.BindExternalFunction("GiveAwayItem", (string itemId) => itemGiveAway.GiveAwayItem(itemId));
        else
            Debug.LogWarning($"{name}: No DialogueItemGiveAway found — GiveAwayItem will not be bound.", this);
    }

    private void BeginStandard()
    {
        dialogueIsPlaying = true;
        GameStateManager.ChangeState(GameState.Dialogue);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ContinueStandardStory();
    }

    private void BeginIntro()
    {
        if (introPresenter == null)
            introPresenter = FindFirstObjectByType<IntroSequencePresenter>();

        if (introPresenter == null)
        {
            Debug.LogWarning(
                $"{name}: IntroSequencePresenter missing in scene — falling back to Standard dialogue UI. " +
                "Add IntroSequencePresenter and assign it on DialogueManager.",
                this);
            activeMode = DialoguePresentationMode.Standard;
            BeginStandard();
            return;
        }

        if (!introPresenter.Begin(currentStory, ExitAfterPresenter))
        {
            activeMode = DialoguePresentationMode.Standard;
            BeginStandard();
            return;
        }

        dialogueIsPlaying = true;
        if (GameStateManager.CurrentState != GameState.Dialogue)
            GameStateManager.ChangeState(GameState.Dialogue);
    }

    private void BeginInternal()
    {
        if (internalPresenter == null)
        {
            Debug.LogWarning($"{name}: InternalMonologuePresenter not assigned — falling back to Standard.", this);
            activeMode = DialoguePresentationMode.Standard;
            BeginStandard();
            return;
        }

        // Stay in Gameplay so the player can keep moving.
        dialogueIsPlaying = true;
        internalPresenter.Begin(currentStory, ExitAfterPresenter);
    }

    /// <summary>
    /// Stops the current presentation without treating it as a completed knot.
    /// Used when another dialogue must force-start.
    /// </summary>
    public void AbortActivePresentation()
    {
        if (internalPresenter != null && internalPresenter.IsActive)
            internalPresenter.Abort();

        if (introPresenter != null && introPresenter.IsActive)
            introPresenter.Abort();

        if (GlobalVariableOperator.Instance != null)
        {
            GlobalVariableOperator.Instance.SyncFromStory(currentStory);
            GlobalVariableOperator.Instance.UnbindStory();
        }

        activeKnotName = "";
        currentStory = null;
        dialogueIsPlaying = false;
        isChoosing = false;
        activeMode = DialoguePresentationMode.Standard;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ResolveTypewriter()?.Stop(clearText: true);
        if (dialogueText != null)
            dialogueText.text = "";

        HideChoiceButtons();

        if (GameStateManager.CurrentState == GameState.Dialogue)
            GameStateManager.ChangeState(GameState.Gameplay);
    }

    private void BeginPager(bool respondSupportPager = false)
    {
        PagerTextController pager = pagerController != null ? pagerController : PagerTextController.Instance;
        if (pager == null)
        {
            Debug.LogWarning($"{name}: PagerTextController not assigned — falling back to Standard.", this);
            activeMode = DialoguePresentationMode.Standard;
            BeginStandard();
            return;
        }

        pagerController = pager;
        // Player stays free until they press Tab. Does not set dialogueIsPlaying,
        // so NPC talk remains possible while a thread sits in the inbox.
        dialogueIsPlaying = false;
        activeMode = DialoguePresentationMode.Pager;

        pager.BeginConversation(currentStory, activeKnotName, HandlePagerCompleted, respondSupportPager);
    }

    private void HandlePagerCompleted(string knotName)
    {
        string completed = string.IsNullOrEmpty(knotName) ? activeKnotName : knotName;
        activeKnotName = "";
        currentStory = null;
        activeMode = DialoguePresentationMode.Standard;
        HideChoiceButtons();

        // Pager does not own the cursor — leave lock/visibility as-is.
        if (GameStateManager.CurrentState == GameState.Dialogue)
            GameStateManager.ChangeState(GameState.Gameplay);

        OnDialogueEnded?.Invoke(completed);
    }

    private void UpdatePagerChoiceUi()
    {
        if (pagerController == null || !pagerController.IsOpen)
        {
            if (isChoosing)
                HideChoiceButtons();
            return;
        }

        if (!pagerController.IsWaitingForChoice)
        {
            if (isChoosing)
                HideChoiceButtons();
            return;
        }

        IReadOnlyList<Choice> pending = pagerController.GetPendingChoices();
        if (pending == null || pending.Count == 0)
            return;

        if (!isChoosing)
            DisplayChoicesFromList(pending);
    }

    private void ExitAfterPresenter()
    {
        DialoguePresentationMode endingMode = activeMode;

        if (GlobalVariableOperator.Instance != null)
        {
            GlobalVariableOperator.Instance.SyncFromStory(currentStory);
            GlobalVariableOperator.Instance.UnbindStory();
        }

        string completedKnot = activeKnotName;
        activeKnotName = "";
        currentStory = null;
        dialogueIsPlaying = false;
        isChoosing = false;
        activeMode = DialoguePresentationMode.Standard;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ResolveTypewriter()?.Stop(clearText: true);
        if (dialogueText != null)
            dialogueText.text = "";

        HideChoiceButtons();

        // Internal monologue never took the cursor; don't steal it back
        // (e.g. minigame / pager UI that needs it visible).
        if (endingMode != DialoguePresentationMode.InternalMonologue
            && endingMode != DialoguePresentationMode.Pager)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (GameStateManager.CurrentState == GameState.Dialogue)
            GameStateManager.ChangeState(GameState.Gameplay);

        OnDialogueEnded?.Invoke(completedKnot);
    }

    private void ExitStandardDialogue()
    {
        if (GlobalVariableOperator.Instance != null)
        {
            GlobalVariableOperator.Instance.SyncFromStory(currentStory);
            GlobalVariableOperator.Instance.UnbindStory();
        }

        string completedKnot = activeKnotName;
        activeKnotName = "";
        currentStory = null;

        dialogueIsPlaying = false;
        isChoosing = false;
        activeMode = DialoguePresentationMode.Standard;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ResolveTypewriter()?.Stop(clearText: true);
        if (dialogueText != null)
            dialogueText.text = "";

        HideChoiceButtons();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameStateManager.ChangeState(GameState.Gameplay);
        OnDialogueEnded?.Invoke(completedKnot);
    }

    private void ContinueStandardStory()
    {
        if (currentStory == null)
            return;

        DialogueTypewriter writer = ResolveTypewriter();
        if (writer != null && writer.IsTyping)
            return;

        if (!currentStory.canContinue)
        {
            if (currentStory.currentChoices.Count > 0)
            {
                DisplayChoices();
                nextInputTime = Time.time + inputDelay;
                return;
            }

            ExitStandardDialogue();
            return;
        }

        string text = currentStory.Continue();
        while (string.IsNullOrWhiteSpace(text) && currentStory.canContinue)
            text = currentStory.Continue();

        nextInputTime = Time.time + inputDelay;

        if (!string.IsNullOrWhiteSpace(text))
        {
            string trimmed = text.Trim();
            if (writer != null)
            {
                // Keep choices hidden until typing finishes. Showing them early let a click
                // call ChooseChoiceIndex, then ContinueStandardStory early-out on IsTyping,
                // leaving isChoosing false with buttons still up (clicks dead; Space advances).
                HideChoiceButtons();
                writer.Play(DialogueTextChannel.Standard, trimmed, dialogueText, OnStandardLineTyped);
                return;
            }

            if (dialogueText != null)
                dialogueText.text = trimmed;
        }

        DisplayChoices();

        if (string.IsNullOrWhiteSpace(text) && !currentStory.canContinue && currentStory.currentChoices.Count == 0)
            ExitStandardDialogue();
    }

    private void OnStandardLineTyped()
    {
        if (!dialogueIsPlaying || activeMode != DialoguePresentationMode.Standard || currentStory == null)
            return;

        DisplayChoices();

        if (!currentStory.canContinue && currentStory.currentChoices.Count == 0)
            ExitStandardDialogue();
    }

    private void DisplayChoices()
    {
        if (currentStory == null)
            return;

        DisplayChoicesFromList(currentStory.currentChoices);
    }

    private void DisplayChoicesFromList(IReadOnlyList<Choice> currentChoices)
    {
        if (choices == null || choices.Length == 0)
            return;

        if (currentChoices.Count > choices.Length)
            Debug.LogWarning("invalid number of choices");

        if (currentChoices.Count > 0)
        {
            isChoosing = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            int index = 0;
            foreach (Choice choice in currentChoices)
            {
                choices[index].SetActive(true);
                if (choicesText[index] != null)
                    choicesText[index].text = choice.text;
                index++;
            }

            for (int i = index; i < choices.Length; i++)
                choices[i].SetActive(false);
        }
        else
        {
            HideChoiceButtons();
        }
    }

    private void HideChoiceButtons()
    {
        isChoosing = false;
        if (choices == null)
            return;

        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i] != null)
                choices[i].SetActive(false);
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        if (!isChoosing)
            return;

        if (activeMode == DialoguePresentationMode.Pager && pagerController != null)
        {
            isChoosing = false;
            HideChoiceButtons();
            pagerController.NotifyChoiceMade(choiceIndex);
            nextInputTime = Time.time + inputDelay;
            return;
        }

        if (activeMode != DialoguePresentationMode.Standard || currentStory == null)
            return;

        if (choiceIndex < 0 || choiceIndex >= currentStory.currentChoices.Count)
            return;

        isChoosing = false;
        HideChoiceButtons();
        nextInputTime = Time.time + inputDelay;

        DialogueTypewriter writer = ResolveTypewriter();
        if (writer != null && writer.IsTyping)
            writer.Stop(clearText: false);

        if (GlobalVariableOperator.Instance != null)
            GlobalVariableOperator.Instance.RecordChoice(currentStory.currentChoices[choiceIndex].text);

        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStandardStory();
    }

    /// <summary>True when a locking or overlapping presentation should block new starts.</summary>
    public bool IsBusy
    {
        get
        {
            if (dialogueIsPlaying)
                return true;

            if (introPresenter != null && introPresenter.IsActive)
                return true;

            if (internalPresenter != null && internalPresenter.IsActive)
                return true;

            return false;
        }
    }
}
