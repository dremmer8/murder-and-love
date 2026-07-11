using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;

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
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
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
        dialogueIsPlaying = false;
        isChoosing = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        GameStateManager.ChangeState(GameState.Gameplay);
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            DisplayChoices();
            nextInputTime = Time.time + inputDelay;
        }
        else
        {
            ExitDialogue();
        }
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

        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }
}