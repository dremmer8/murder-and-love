using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConditionalKnot
{
    public string knotName;
    public Collider triggerArea;
    public Transform targetObject;
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("Ink File Settings")] 
    [SerializeField] private TextAsset inkFile;
    [SerializeField] private string defaultKnotName;
    
    [Header("Conditional Knots")]
    [SerializeField] private List<ConditionalKnot> conditionalKnots;
    
    private bool playerInRange;

    private void Awake()
    {
        playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            if (Input.GetKey(KeyCode.E))
            {
                string knotToPlay = EvaluateKnotConditions();
                DialogueManager.GetInstance().EnterDialogue(inkFile, knotToPlay);
            }
        }
    }

    private string EvaluateKnotConditions()
    {
        foreach (ConditionalKnot condition in conditionalKnots)
        {
            if (condition.triggerArea != null && condition.targetObject != null)
            {
                if (condition.triggerArea.bounds.Contains(condition.targetObject.position))
                {
                    return condition.knotName;
                }
            }
        }
        
        return defaultKnotName;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}