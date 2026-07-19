using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

/// <summary>
/// One intro row: a text bit on the left and optional option buttons on the right.
/// Row 0 and the final row are usually text-only; middle rows use up to three option buttons.
/// </summary>
[Serializable]
public class IntroBitRow
{
    [Tooltip("Root of this row. Hidden until the bit is revealed.")]
    public GameObject rowRoot;

    public TextMeshProUGUI textBit;

    [Tooltip("Up to three option slots (leave empty for text-only bits).")]
    public Button[] optionButtons = new Button[3];
}

/// <summary>
/// Static intro layout: opening text bit, text+option rows, then a closing text-only bit.
/// After a pick, hide all option buttons and leave the text bit.
/// </summary>
public class IntroSequencePresenter : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private GameObject rootPanel;

    [Tooltip("Bit 0 = opening text only. Bits 1–3 = text + options. Bit 4 = closing text only.")]
    [SerializeField] private IntroBitRow[] bitRows = new IntroBitRow[5];

    [Header("Input")]
    [SerializeField] private float inputDelay = 0.2f;

    [Tooltip("If true, Space advances when a revealed bit has no pending choices.")]
    [SerializeField] private bool advanceTextWithSpace = true;

    [Header("Typewriter")]
    [SerializeField] private DialogueTypewriter typewriter;

    private Story _story;
    private Action _onComplete;
    private bool _active;
    private bool _isChoosing;
    private float _nextInputTime;
    private int _activeRowIndex = -1;

    public bool IsActive => _active;

    /// <returns>False if the static layout is not wired; caller should fall back.</returns>
    public bool Begin(Story story, Action onComplete)
    {
        ResetAllRows();

        if (!HasAnyWiredRow())
        {
            Debug.LogError(
                $"{name}: No IntroBitRow slots wired (rowRoot / textBit / optionButtons). " +
                "Assign 5 rows: opening text + 3 text/option rows + closing text.",
                this);
            return false;
        }

        _story = story;
        _onComplete = onComplete;
        _active = true;
        _isChoosing = false;
        _activeRowIndex = -1;
        _nextInputTime = Time.time + inputDelay;

        if (rootPanel != null)
            rootPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RevealNextBit();
        return true;
    }

    private bool HasAnyWiredRow()
    {
        if (bitRows == null || bitRows.Length == 0)
            return false;

        for (int i = 0; i < bitRows.Length; i++)
        {
            IntroBitRow row = bitRows[i];
            if (row != null && (row.rowRoot != null || row.textBit != null))
                return true;
        }

        return false;
    }

    public void Abort()
    {
        if (!_active)
            return;

        ResolveTypewriter()?.Stop(clearText: false);
        Finish(invokeCallback: false);
    }

    private void Update()
    {
        if (!_active || _isChoosing)
            return;

        if (!advanceTextWithSpace)
            return;

        if (Time.time < _nextInputTime)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DialogueTypewriter writer = ResolveTypewriter();
            if (writer != null && writer.IsTyping && writer.Skip())
                return;

            RevealNextBit();
        }
    }

    private void RevealNextBit()
    {
        if (_story == null)
            return;

        if (_isChoosing)
            return;

        DialogueTypewriter writer = ResolveTypewriter();
        if (writer != null && writer.IsTyping)
            return;

        string prompt = PullNextText();

        if (string.IsNullOrEmpty(prompt) && !_story.canContinue && _story.currentChoices.Count == 0)
        {
            Finish(invokeCallback: true);
            return;
        }

        // Choices waiting with no new text — show them on the current / next row.
        if (string.IsNullOrEmpty(prompt) && _story.currentChoices.Count > 0)
        {
            if (_activeRowIndex < 0)
                ActivateNextRow("");
            ShowOptionsOnActiveRow();
            return;
        }

        ShowTextBit(prompt);
    }

    /// <summary>
    /// Shows <paramref name="prompt"/> on the next row. If the rest of the story has no
    /// choices (closing beat), also pulls remaining paragraphs into the same field.
    /// </summary>
    private void ShowTextBit(string prompt)
    {
        prompt = CollectClosingParagraphs(prompt);

        if (!ActivateNextRow(prompt))
        {
            // More Ink lines than wired rows — fold remaining text into the last bit.
            AppendToActiveText(prompt);
            while (_story.canContinue)
            {
                string extra = PullNextText();
                if (!string.IsNullOrEmpty(extra))
                    AppendToActiveText(extra);
            }

            if (_story.currentChoices.Count > 0)
            {
                ShowOptionsOnActiveRow();
                return;
            }

            _nextInputTime = Time.time + inputDelay;
            return;
        }

        _nextInputTime = Time.time + inputDelay;

        if (_story.currentChoices.Count > 0)
        {
            ShowOptionsOnActiveRow();
            return;
        }

        // Text-only bit (e.g. final closing lines). Space will advance / finish.
    }

    /// <summary>
    /// When no further choices remain, merge following Ink lines into one closing bit.
    /// </summary>
    private string CollectClosingParagraphs(string firstLine)
    {
        if (_story == null || string.IsNullOrEmpty(firstLine))
            return firstLine;

        if (_story.currentChoices.Count > 0 || PeekRemainingHasChoices())
            return firstLine;

        while (_story.canContinue)
        {
            string extra = PullNextText();
            if (string.IsNullOrEmpty(extra))
                break;

            firstLine = $"{firstLine}\n{extra}";
        }

        return firstLine;
    }

    private bool PeekRemainingHasChoices()
    {
        if (_story == null)
            return false;

        string savedState = _story.state.ToJson();
        try
        {
            while (_story.canContinue)
            {
                _story.Continue();
                if (_story.currentChoices.Count > 0)
                    return true;
            }

            return _story.currentChoices.Count > 0;
        }
        finally
        {
            _story.state.LoadJson(savedState);
        }
    }

    private void AppendToActiveText(string extra)
    {
        if (string.IsNullOrEmpty(extra) || _activeRowIndex < 0 || bitRows == null)
            return;

        if (_activeRowIndex >= bitRows.Length)
            return;

        IntroBitRow row = bitRows[_activeRowIndex];
        if (row?.textBit == null)
            return;

        DialogueTypewriter writer = ResolveTypewriter();
        string coloredExtra = writer != null ? writer.ApplySpeakerColor(extra) : extra;

        if (string.IsNullOrEmpty(row.textBit.text))
            row.textBit.text = coloredExtra;
        else
            row.textBit.text = $"{row.textBit.text}\n{coloredExtra}";

        row.textBit.maxVisibleCharacters = int.MaxValue;
    }

    private string PullNextText()
    {
        if (_story == null)
            return string.Empty;

        if (!_story.canContinue)
            return string.Empty;

        string text = _story.Continue();
        while (string.IsNullOrWhiteSpace(text) && _story.canContinue)
            text = _story.Continue();

        return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
    }

    private bool ActivateNextRow(string prompt)
    {
        int next = _activeRowIndex + 1;
        if (bitRows == null || next < 0 || next >= bitRows.Length)
            return false;

        IntroBitRow row = bitRows[next];
        if (row == null)
            return false;

        _activeRowIndex = next;

        if (row.rowRoot != null)
            row.rowRoot.SetActive(true);

        if (row.textBit != null)
        {
            row.textBit.gameObject.SetActive(true);
            DialogueTypewriter writer = ResolveTypewriter();
            if (writer != null)
                writer.PlayIntro(prompt ?? "", row.textBit);
            else
                row.textBit.text = prompt ?? "";
        }

        HideAllOptions(row);
        return true;
    }

    DialogueTypewriter ResolveTypewriter()
    {
        if (typewriter == null)
            typewriter = DialogueTypewriter.Instance;
        return typewriter;
    }

    private void ShowOptionsOnActiveRow()
    {
        if (_story == null || _activeRowIndex < 0 || _activeRowIndex >= bitRows.Length)
            return;

        IntroBitRow row = bitRows[_activeRowIndex];
        if (row == null || row.optionButtons == null)
        {
            Debug.LogWarning($"{name}: Active intro row has no option buttons.", this);
            return;
        }

        List<Choice> currentChoices = _story.currentChoices;
        if (currentChoices == null || currentChoices.Count == 0)
            return;

        _isChoosing = true;
        HideAllOptions(row);

        int slots = Mathf.Min(row.optionButtons.Length, currentChoices.Count);
        for (int i = 0; i < slots; i++)
        {
            Button button = row.optionButtons[i];
            if (button == null)
                continue;

            int choiceIndex = i;
            button.gameObject.SetActive(true);
            button.interactable = true;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                label.text = currentChoices[i].text;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnOptionChosen(choiceIndex));
        }

        // Extra pre-placed slots stay hidden when Ink has fewer than 3 choices.
        for (int i = slots; i < row.optionButtons.Length; i++)
        {
            if (row.optionButtons[i] != null)
                row.optionButtons[i].gameObject.SetActive(false);
        }
    }

    private void OnOptionChosen(int choiceIndex)
    {
        if (!_isChoosing || _story == null)
            return;

        if (choiceIndex < 0 || choiceIndex >= _story.currentChoices.Count)
            return;

        IntroBitRow row = bitRows[_activeRowIndex];
        string chosenText = _story.currentChoices[choiceIndex].text;

        if (GlobalVariableOperator.Instance != null)
            GlobalVariableOperator.Instance.RecordChoice(chosenText);

        HideAllOptions(row);

        _isChoosing = false;
        _nextInputTime = Time.time + inputDelay;

        _story.ChooseChoiceIndex(choiceIndex);

        // Fold the chosen wording into the text bit (buttons are cleared), then any <> glue.
        if (row?.textBit != null && !string.IsNullOrEmpty(chosenText))
        {
            if (string.IsNullOrEmpty(row.textBit.text))
                row.textBit.text = chosenText;
            else
                row.textBit.text = $"{row.textBit.text} {chosenText}";
        }

        string following = PullNextText();
        if (!string.IsNullOrEmpty(following))
        {
            if (IsSentenceGlue(following) && row?.textBit != null)
            {
                row.textBit.text = $"{row.textBit.text} {following}";
                row.textBit.maxVisibleCharacters = int.MaxValue;
            }
            else
            {
                if (row?.textBit != null)
                    row.textBit.maxVisibleCharacters = int.MaxValue;

                ShowTextBit(following);
                return;
            }
        }

        if (row?.textBit != null)
            row.textBit.maxVisibleCharacters = int.MaxValue;

        if (_story.currentChoices.Count > 0)
        {
            ShowOptionsOnActiveRow();
            return;
        }

        // Auto-advance to the next bit after a choice so the layout fills like the mockup.
        if (_story.canContinue || _story.currentChoices.Count > 0)
        {
            RevealNextBit();
            return;
        }

        Finish(invokeCallback: true);
    }

    /// <summary>
    /// Ink <> glue after a choice is typically a lowercase continuation ("tried to take...").
    /// A new capitalised paragraph is the next intro bit, not glue.
    /// </summary>
    private static bool IsSentenceGlue(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        char c = text[0];
        return char.IsLower(c) || c == ',' || c == ';' || c == ')' || c == ']';
    }

    private void HideAllOptions(IntroBitRow row)
    {
        if (row?.optionButtons == null)
            return;

        for (int i = 0; i < row.optionButtons.Length; i++)
        {
            Button button = row.optionButtons[i];
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.interactable = false;
            button.gameObject.SetActive(false);
        }
    }

    private void ResetAllRows()
    {
        _isChoosing = false;
        _activeRowIndex = -1;

        if (bitRows == null)
            return;

        for (int r = 0; r < bitRows.Length; r++)
        {
            IntroBitRow row = bitRows[r];
            if (row == null)
                continue;

            if (row.rowRoot != null)
                row.rowRoot.SetActive(false);

            if (row.textBit != null)
            {
                row.textBit.text = "";
                row.textBit.gameObject.SetActive(false);
            }

            HideAllOptions(row);
        }
    }

    private void Finish(bool invokeCallback)
    {
        _active = false;
        _isChoosing = false;
        _story = null;
        _activeRowIndex = -1;

        // Keep final layout visible until panel is closed by caller / next Begin.
        if (rootPanel != null)
            rootPanel.SetActive(false);

        Action callback = _onComplete;
        _onComplete = null;

        if (invokeCallback)
            callback?.Invoke();
    }
}
