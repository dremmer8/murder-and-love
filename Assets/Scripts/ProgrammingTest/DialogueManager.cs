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
    private int currentChoiceIndex = 0;
    
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
        if (!dialogueIsPlaying)
        {
            return;
        }

        if (Time.time < nextInputTime)
        {
            return;
        }

        if (isChoosing)
        {
            HandleChoiceNavigation();
        }
        else
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ContinueStory();
            }
        }
    }

    private void HandleChoiceNavigation()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentChoiceIndex++;
            if (currentChoiceIndex >= currentStory.currentChoices.Count)
            {
                currentChoiceIndex = 0;
            }
            EventSystem.current.SetSelectedGameObject(choices[currentChoiceIndex]);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentChoiceIndex--;
            if (currentChoiceIndex < 0)
            {
                currentChoiceIndex = currentStory.currentChoices.Count - 1;
            }
            EventSystem.current.SetSelectedGameObject(choices[currentChoiceIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            MakeChoice(currentChoiceIndex);
        }
    }

    public void EnterDialogue(TextAsset inkFile)
    {
        currentStory = new Story(inkFile.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    private void ExitDialogue()
    {
        dialogueIsPlaying = false;
        isChoosing = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
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
            currentChoiceIndex = 0;
            
            int index = 0;
            //enable and initialize choices of current story
            foreach (Choice choice in currentChoices)
            {
                choices[index].SetActive(true);
                choicesText[index].text = choice.text;
                index++;
            }
            //close choices game objects that are not necessary
            for (int i = index; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
            }

            StartCoroutine(SelectFirstChoice());
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

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0]);
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