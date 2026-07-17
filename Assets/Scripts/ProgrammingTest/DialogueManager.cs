using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using System;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")] 
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choices UI")] 
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;
    
    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }
    private static DialogueManager instance;

    private bool isChoosing = false;
    
    private float inputDelay = 0.2f;
    private float nextInputTime = 0f;

    private string activeKnotName = "";

    /// <summary>Fired when dialogue exits. Argument is the knot that was entered (may be empty).</summary>
    public event Action<string> OnDialogueEnded;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("DialogueManager instances are missing or more than one in scene");
        }
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        
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

        // Inspector OnClick targets can go missing (Game.unity had null refs). Bind in code.
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => MakeChoice(choiceIndex));
    }

    private void Update()
    {
        if (GameStateManager.CurrentState != GameState.Dialogue)
        {
            return;
        }

        if (Time.time < nextInputTime)
        {
            return;
        }

        if (!isChoosing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ContinueStory();
            }
        }
    }

    public void EnterDialogue(TextAsset inkFile, string knotName = "")
    {
        GameStateManager.ChangeState(GameState.Dialogue);
        
        currentStory = new Story(inkFile.text);
        activeKnotName = knotName ?? "";

        if (GlobalVariableOperator.Instance != null)
        {
            GlobalVariableOperator.Instance.ApplyVariablesToStory(currentStory);
            GlobalVariableOperator.Instance.BindStory(currentStory);
        }
        
        if (!string.IsNullOrEmpty(knotName))
        {
            currentStory.ChoosePathString(knotName);
        }

        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ContinueStory();
    }

    private void ExitDialogue()
    {
        if (GlobalVariableOperator.Instance != null)
        {
            GlobalVariableOperator.Instance.SyncFromStory(currentStory);
            GlobalVariableOperator.Instance.UnbindStory();
        }

        string completedKnot = activeKnotName;
        activeKnotName = "";

        dialogueIsPlaying = false;
        isChoosing = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        GameStateManager.ChangeState(GameState.Gameplay);

        OnDialogueEnded?.Invoke(completedKnot);
    }

    private void ContinueStory()
    {
        if (!currentStory.canContinue)
        {
            // Choices can appear when the story cannot continue further without picking one.
            if (currentStory.currentChoices.Count > 0)
            {
                DisplayChoices();
                nextInputTime = Time.time + inputDelay;
                return;
            }

            ExitDialogue();
            return;
        }

        // Ink often yields empty/whitespace lines at gathers ("-") and after choice diverts.
        // Skip those so the player never sees a blank "" beat.
        string text = currentStory.Continue();
        while (string.IsNullOrWhiteSpace(text) && currentStory.canContinue)
        {
            text = currentStory.Continue();
        }

        if (!string.IsNullOrWhiteSpace(text))
            dialogueText.text = text.Trim();

        DisplayChoices();
        nextInputTime = Time.time + inputDelay;

        // After skipping blanks we may land on choices with no new line, or on END.
        if (string.IsNullOrWhiteSpace(text) && !currentStory.canContinue && currentStory.currentChoices.Count == 0)
            ExitDialogue();
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogWarning("invalid number of choices");
        }

        if (currentChoices.Count > 0)
        {
            isChoosing = true;
            
            int index = 0;
            
            foreach (Choice choice in currentChoices)
            {
                choices[index].SetActive(true);
                choicesText[index].text = choice.text;
                index++;
            }
            
            for (int i = index; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
            }
        }
        else
        {
            isChoosing = false;
            for (int i = 0; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
            }
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        if (!isChoosing) return;

        isChoosing = false;
        nextInputTime = Time.time + inputDelay;

        if (GlobalVariableOperator.Instance != null
            && choiceIndex >= 0
            && choiceIndex < currentStory.currentChoices.Count)
        {
            GlobalVariableOperator.Instance.RecordChoice(currentStory.currentChoices[choiceIndex].text);
        }

        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }
}
