using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PagerTextController : MonoBehaviour
{


    public Animator animator;
    [SerializeField] TextMeshPro screenText;
    [SerializeField] string message;
    [SerializeField] int visibleCharacterCount = 16;
    public GameObject truePager;

    public List<GameObject> propPagers = new List<GameObject>();
    bool isVisible=false;

    int _scrollIndex;

    void Start()
    {
        RefreshDisplay();
    }

    void OnValidate()
    {
        visibleCharacterCount = Mathf.Max(1, visibleCharacterCount);
        _scrollIndex = Mathf.Clamp(_scrollIndex, 0, GetMaxScrollIndex());
        RefreshDisplay();
    }

    public void SetMessage(string text)
    {
        message = text ?? "";
        _scrollIndex = 0;
        RefreshDisplay();
    }

    [ContextMenu("Toggle Pager")]
    public void TogglePager()
    {
        isVisible =!isVisible;
    if (isVisible)
    {
        foreach (var pager in propPagers)
        {
            pager.SetActive(false);
        }
    }
    else
    {
        foreach (var pager in propPagers)
        {
            pager.SetActive(true);
        }
    }
        if (animator != null  )
            animator.SetTrigger("toggle");
    }



   
    public void PokePager()
    {
        if (animator != null)
            animator.SetTrigger("poke");
    }

    [ContextMenu("Scroll Left")]
    public void ScrollLeft()
    {PokePager();
        _scrollIndex = Mathf.Max(0, _scrollIndex - visibleCharacterCount);
        RefreshDisplay();
    }

    [ContextMenu("Scroll Right")]
    public void ScrollRight()
    {PokePager();
        _scrollIndex = Mathf.Min(GetMaxScrollIndex(), _scrollIndex + visibleCharacterCount);
        RefreshDisplay();
    }



    int GetMaxScrollIndex()
    {
        if (string.IsNullOrEmpty(message))
            return 0;
        return Mathf.Max(0, message.Length - visibleCharacterCount);
    }

    void RefreshDisplay()
    {
        if (screenText == null)
            return;

        if (string.IsNullOrEmpty(message))
        {
            screenText.text = "";
            return;
        }

        int length = Mathf.Min(visibleCharacterCount, message.Length - _scrollIndex);
        screenText.text = message.Substring(_scrollIndex, length);
    }
}
