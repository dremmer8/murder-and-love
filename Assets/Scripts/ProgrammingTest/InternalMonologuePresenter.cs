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
    [SerializeField] private DialogueTypewriter typewriter;

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
    private bool _waitingForTypewriter;

    public bool IsActive => _active;

    public void Begin(Story story, Action onComplete)
    {
        _story = story;
        _onComplete = onComplete;
        _active = true;
        _holdingLine = false;
        _waitingForTypewriter = false;

        if (panel != null)
            panel.SetActive(true);

        ShowNextLine();
    }

    public void Abort()
    {
        if (!_active)
            return;

        ResolveTypewriter()?.Stop(clearText: true);
        Finish(invokeCallback: false);
    }

    private void Update()
    {
        if (!_active || !_holdingLine || _waitingForTypewriter)
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
        float lineStart = Time.time;
        DialogueTypewriter writer = ResolveTypewriter();

        if (writer != null)
        {
            _waitingForTypewriter = true;
            _holdingLine = false;
            writer.Play(DialogueTextChannel.Internal, trimmed, monologueText, () => BeginHoldAfterTyping(trimmed, lineStart));
            return;
        }

        if (monologueText != null)
        {
            monologueText.text = trimmed;
            ThoughtLineHover.ApplyForLine(monologueText, trimmed);
        }

        BeginHoldAfterTyping(trimmed, lineStart);
    }

    void BeginHoldAfterTyping(string trimmed, float lineStart)
    {
        if (!_active)
            return;

        _waitingForTypewriter = false;

        float targetDuration = Mathf.Clamp(
            trimmed.Length * secondsPerCharacter,
            minDisplaySeconds,
            maxDisplaySeconds);
        float remaining = Mathf.Max(0f, targetDuration - (Time.time - lineStart));

        _lineExpireTime = Time.time + remaining + gapBetweenLines;
        _holdingLine = true;
    }

    DialogueTypewriter ResolveTypewriter()
    {
        if (typewriter == null)
            typewriter = DialogueTypewriter.Instance;
        return typewriter;
    }

    private void Finish(bool invokeCallback)
    {
        _active = false;
        _holdingLine = false;
        _waitingForTypewriter = false;
        _story = null;

        ResolveTypewriter()?.Clear(DialogueTextChannel.Internal);
        if (monologueText != null)
        {
            ThoughtLineHover.StopFor(monologueText);
            monologueText.text = "";
        }

        if (panel != null)
            panel.SetActive(false);

        Action callback = _onComplete;
        _onComplete = null;

        if (invokeCallback)
            callback?.Invoke();
    }
}
