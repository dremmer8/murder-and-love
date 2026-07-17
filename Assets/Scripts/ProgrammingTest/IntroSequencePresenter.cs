using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

/// <summary>
/// One intro row: a text bit on the left and optional option buttons on the right.
/// Row 0 is usually text-only; later rows use up to three option buttons.
/// </summary>
[Serializable]
public class IntroBitRow
{
    [Tooltip("Root of this row. Hidden until the bit is revealed.")]
    public GameObject rowRoot;

    public TextMeshProUGUI textBit;

    [Tooltip("Up to three option slots (leave empty / unused for the opening text-only bit).")]
    public Button[] optionButtons = new Button[3];
}

/// <summary>
/// Static intro layout: opening text bit, then text+option rows.
/// After a pick, hide unchosen options and leave text + chosen option.
/// </summary>
public class IntroSequencePresenter : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private GameObject rootPanel;

    [Tooltip("Bit 0 = opening text only. Bits 1–3 = text + up to 3 options each.")]
    [SerializeField] private IntroBitRow[] bitRows = new IntroBitRow[4];

    [Header("Input")]
    [SerializeField] private float inputDelay = 0.2f;

    [Tooltip("If true, Space advances when a revealed bit has no pending choices.")]
    [SerializeField] private bool advanceTextWithSpace = true;

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
                "Assign 4 rows: opening text + 3 text/option rows.",
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
            RevealNextBit();
    }

    private void RevealNextBit()
    {
        if (_story == null)
            return;

        if (_isChoosing)
            return;

        // Pull next non-empty line (and any immediate glue after prior choice).
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

        // Text-only bit (e.g. final closing lines). Space will advance.
        if (!_story.canContinue && _story.currentChoices.Count == 0)
        {
            // Stay on last text; next Space finishes.
            return;
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

        if (string.IsNullOrEmpty(row.textBit.text))
            row.textBit.text = extra;
        else
            row.textBit.text = $"{row.textBit.text}\n{extra}";
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
            row.textBit.text = prompt ?? "";
        }

        HideAllOptions(row);
        return true;
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

        // Hide unchosen options; leave the chosen one visible but no longer clickable.
        if (row != null && row.optionButtons != null)
        {
            for (int i = 0; i < row.optionButtons.Length; i++)
            {
                Button button = row.optionButtons[i];
                if (button == null)
                    continue;

                if (i == choiceIndex)
                {
                    button.gameObject.SetActive(true);
                    button.interactable = false;
                    button.onClick.RemoveAllListeners();
                }
                else
                {
                    button.gameObject.SetActive(false);
                    button.onClick.RemoveAllListeners();
                }
            }
        }

        _isChoosing = false;
        _nextInputTime = Time.time + inputDelay;

        _story.ChooseChoiceIndex(choiceIndex);

        // Ink often glues the chosen word back into the sentence — append onto this text bit.
        string glue = PullNextText();
        if (!string.IsNullOrEmpty(glue) && row != null && row.textBit != null)
        {
            if (string.IsNullOrEmpty(row.textBit.text))
                row.textBit.text = glue;
            else
                row.textBit.text = $"{row.textBit.text} {glue}";
        }

        // If more choices appeared immediately (unusual), show on same row leftover slots — otherwise next bit.
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
