using System;
using TMPro;
using UnityEngine;
using Ink.Runtime;

/// <summary>
/// Non-locking internal thoughts. Lines auto-advance by character count;
/// Space cannot skip. Player movement stays in Gameplay.
/// </summary>
public class InternalMonologuePresenter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI monologueText;

    [Header("Timing")]
    [SerializeField] private float secondsPerCharacter = 0.045f;
    [SerializeField] private float minDisplaySeconds = 1.4f;
    [SerializeField] private float maxDisplaySeconds = 7.5f;
    [SerializeField] private float gapBetweenLines = 0.2f;

    private Story _story;
    private Action _onComplete;
    private bool _active;
    private float _lineExpireTime;
    private bool _holdingLine;

    public bool IsActive => _active;

    public void Begin(Story story, Action onComplete)
    {
        _story = story;
        _onComplete = onComplete;
        _active = true;
        _holdingLine = false;

        if (panel != null)
            panel.SetActive(true);

        ShowNextLine();
    }

    public void Abort()
    {
        if (!_active)
            return;

        Finish(invokeCallback: false);
    }

    private void Update()
    {
        if (!_active || !_holdingLine)
            return;

        // Intentionally ignore Space — timed only.
        if (Time.time < _lineExpireTime)
            return;

        _holdingLine = false;

        if (_story != null && _story.currentChoices.Count > 0)
        {
            // Internal monologues should not branch; auto-pick first if Ink has choices.
            Debug.LogWarning($"{name}: Internal monologue hit choices; auto-selecting index 0.", this);
            _story.ChooseChoiceIndex(0);
        }

        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (_story == null)
        {
            Finish(invokeCallback: true);
            return;
        }

        if (!_story.canContinue)
        {
            if (_story.currentChoices.Count > 0)
            {
                _story.ChooseChoiceIndex(0);
                ShowNextLine();
                return;
            }

            Finish(invokeCallback: true);
            return;
        }

        string text = _story.Continue();
        while (string.IsNullOrWhiteSpace(text) && _story.canContinue)
            text = _story.Continue();

        if (string.IsNullOrWhiteSpace(text))
        {
            if (!_story.canContinue && _story.currentChoices.Count == 0)
            {
                Finish(invokeCallback: true);
                return;
            }

            ShowNextLine();
            return;
        }

        string trimmed = text.Trim();
        if (monologueText != null)
            monologueText.text = trimmed;

        float duration = Mathf.Clamp(
            trimmed.Length * secondsPerCharacter,
            minDisplaySeconds,
            maxDisplaySeconds);

        _lineExpireTime = Time.time + duration + gapBetweenLines;
        _holdingLine = true;
    }

    private void Finish(bool invokeCallback)
    {
        _active = false;
        _holdingLine = false;
        _story = null;

        if (monologueText != null)
            monologueText.text = "";

        if (panel != null)
            panel.SetActive(false);

        Action callback = _onComplete;
        _onComplete = null;

        if (invokeCallback)
            callback?.Invoke();
    }
}
