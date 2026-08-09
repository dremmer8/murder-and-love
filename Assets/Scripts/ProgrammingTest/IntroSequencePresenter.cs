using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    [Tooltip("Optional fade target for this row. If empty, CanvasGroups are resolved on textBit / option buttons.")]
    public CanvasGroup canvasGroup;

    [Tooltip("Up to three option slots (leave empty for text-only bits).")]
    public Button[] optionButtons = new Button[3];
}

/// <summary>
/// Intro layout: one row visible at a time. Each row fades in from transparent;
/// after the row is done, wait, hide it, then fade in the next.
/// </summary>
public class IntroSequencePresenter : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private GameObject rootPanel;

    [Tooltip("Bit 0 = opening text only. Bits 1–3 = text + options. Bit 4 = closing text only.")]
    [SerializeField] private IntroBitRow[] bitRows = new IntroBitRow[5];

    [Header("Input")]
    [SerializeField] private float inputDelay = 0.2f;

    [Tooltip("If true, Space or left click advances when a revealed bit has no pending choices.")]
    [SerializeField] private bool advanceTextWithSpace = true;

    [Header("Transitions")]
    [Tooltip("Seconds to wait after a row is done before hiding it and showing the next.")]
    [SerializeField] private float rowTransitionDelay = 1f;

    [Tooltip("Fade-in / fade-out duration for each row.")]
    [SerializeField] private float rowFadeDuration = 0.6f;

    [Header("Typewriter")]
    [SerializeField] private DialogueTypewriter typewriter;

    private Story _story;
    private Action _onComplete;
    private bool _active;
    private bool _isChoosing;
    private bool _transitioning;
    private float _nextInputTime;
    private int _activeRowIndex = -1;
    private Coroutine _showRoutine;
    private readonly List<Tween> _activeFades = new List<Tween>();

    public bool IsActive => _active;

    /// <summary>True while option buttons are shown and awaiting a click.</summary>
    public bool IsChoosing => _isChoosing;

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
        _transitioning = false;
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

        StopTransition();
        ResolveTypewriter()?.Stop(clearText: false);
        Finish(invokeCallback: false);
    }

    private void Update()
    {
        if (!_active || _isChoosing || _transitioning)
            return;

        if (!advanceTextWithSpace)
            return;

        if (Time.time < _nextInputTime)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            DialogueTypewriter writer = ResolveTypewriter();
            if (writer != null && writer.IsTyping && writer.Skip())
                return;

            RevealNextBit();
        }
    }

    private void RevealNextBit()
    {
        if (_story == null || _transitioning)
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
            {
                if (_showRoutine != null)
                    StopCoroutine(_showRoutine);
                _showRoutine = StartCoroutine(ShowTextBitRoutine("", showChoicesAfterType: true));
            }
            else
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
        bool showChoicesAfterType = _story != null && _story.currentChoices.Count > 0;

        if (_showRoutine != null)
            StopCoroutine(_showRoutine);

        _showRoutine = StartCoroutine(ShowTextBitRoutine(prompt, showChoicesAfterType));
    }

    private IEnumerator ShowTextBitRoutine(string prompt, bool showChoicesAfterType)
    {
        // Leave the finished row: wait, fade out, hide — then reveal the next object.
        if (_activeRowIndex >= 0)
        {
            _transitioning = true;
            if (rowTransitionDelay > 0f)
                yield return new WaitForSeconds(rowTransitionDelay);

            yield return FadeRow(_activeRowIndex, 0f);
            HideRowVisuals(_activeRowIndex);
            _transitioning = false;
        }

        if (!ActivateNextRow(prompt, showChoicesAfterType ? OnIntroLineTyped : null))
        {
            // More Ink lines than wired rows — re-show last bit and fold remaining text in.
            if (_activeRowIndex >= 0)
            {
                ShowRowVisuals(_activeRowIndex, fadeIn: true);
                AppendToActiveText(prompt);
                while (_story.canContinue)
                {
                    string extra = PullNextText();
                    if (!string.IsNullOrEmpty(extra))
                        AppendToActiveText(extra);
                }

                if (_story.currentChoices.Count > 0)
                    ShowOptionsOnActiveRow();
            }

            _nextInputTime = Time.time + inputDelay;
            _showRoutine = null;
            yield break;
        }

        _nextInputTime = Time.time + inputDelay;
        _showRoutine = null;
    }

    private void OnIntroLineTyped()
    {
        if (!_active || _story == null || _transitioning)
            return;

        if (_story.currentChoices.Count > 0)
            ShowOptionsOnActiveRow();
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

    private bool ActivateNextRow(string prompt, Action onTyped = null)
    {
        int next = _activeRowIndex + 1;
        if (bitRows == null || next < 0 || next >= bitRows.Length)
            return false;

        IntroBitRow row = bitRows[next];
        if (row == null)
            return false;

        _activeRowIndex = next;

        ShowRowVisuals(next, fadeIn: true);
        HideAllOptions(row);

        if (row.textBit != null)
        {
            DialogueTypewriter writer = ResolveTypewriter();
            if (writer != null)
            {
                writer.PlayIntro(prompt ?? "", row.textBit, onTyped);
            }
            else
            {
                row.textBit.text = prompt ?? "";
                onTyped?.Invoke();
            }
        }
        else
        {
            onTyped?.Invoke();
        }

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

        // Ensure option containers are visible for this row.
        if (row.rowRoot != null)
            row.rowRoot.SetActive(true);

        int slots = Mathf.Min(row.optionButtons.Length, currentChoices.Count);
        for (int i = 0; i < slots; i++)
        {
            Button button = row.optionButtons[i];
            if (button == null)
                continue;

            int choiceIndex = i;
            button.gameObject.SetActive(true);
            button.interactable = true;

            CanvasGroup buttonGroup = GetOrAddCanvasGroup(button.gameObject);
            buttonGroup.alpha = 1f;
            buttonGroup.blocksRaycasts = true;
            buttonGroup.interactable = true;

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
        if (!_isChoosing || _story == null || _transitioning)
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

        // Ink already prints non-bracketed choice text on Continue() (e.g. "* gun." → "gun.").
        // Do not prepend chosenText here or it doubles in the intro bit.
        string following = PullNextText();
        if (!string.IsNullOrEmpty(following))
        {
            if ((IsChoiceEcho(following, chosenText) || IsSentenceGlue(following)) && row?.textBit != null)
            {
                string spacer = string.IsNullOrEmpty(row.textBit.text) || following[0] == '.'
                    ? ""
                    : " ";
                row.textBit.text = $"{row.textBit.text}{spacer}{following}";
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
    /// Ink echoes the text of a non-bracketed choice as the next line, and that echo always
    /// completes the sentence the options were attached to. Checked before
    /// <see cref="IsSentenceGlue"/> because scripts without letter case (e.g. Chinese) cannot
    /// be classified by capitalisation.
    /// </summary>
    private static bool IsChoiceEcho(string following, string chosenText)
    {
        if (string.IsNullOrWhiteSpace(chosenText))
            return false;

        return following.StartsWith(chosenText.Trim(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Ink &lt;&gt; glue after a choice is typically a lowercase continuation ("tried to take...")
    /// or a trailing sentence closer (e.g. "." after "shattered by a loan shark").
    /// A new capitalised paragraph is the next intro bit, not glue.
    /// </summary>
    private static bool IsSentenceGlue(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        char c = text[0];
        return char.IsLower(c)
            || c == ','
            || c == ';'
            || c == '.'
            || c == '!'
            || c == '?'
            || c == ')'
            || c == ']';
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

    private void ShowRowVisuals(int rowIndex, bool fadeIn)
    {
        if (bitRows == null || rowIndex < 0 || rowIndex >= bitRows.Length)
            return;

        IntroBitRow row = bitRows[rowIndex];
        if (row == null)
            return;

        if (row.rowRoot != null && (IsRowRootExclusive(rowIndex) || HasAnyOptionButton(row)))
            row.rowRoot.SetActive(true);

        if (row.textBit != null)
            row.textBit.gameObject.SetActive(true);

        List<CanvasGroup> groups = CollectFadeGroups(row);
        for (int i = 0; i < groups.Count; i++)
        {
            CanvasGroup group = groups[i];
            KillFadesOn(group);
            group.alpha = fadeIn && rowFadeDuration > 0f ? 0f : 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        if (fadeIn && rowFadeDuration > 0f)
            StartFade(groups, 1f);
    }

    private void HideRowVisuals(int rowIndex)
    {
        if (bitRows == null || rowIndex < 0 || rowIndex >= bitRows.Length)
            return;

        IntroBitRow row = bitRows[rowIndex];
        if (row == null)
            return;

        HideAllOptions(row);

        if (row.textBit != null)
        {
            row.textBit.text = "";
            row.textBit.gameObject.SetActive(false);
        }

        if (row.rowRoot != null)
            row.rowRoot.SetActive(false);

        List<CanvasGroup> groups = CollectFadeGroups(row);
        for (int i = 0; i < groups.Count; i++)
        {
            CanvasGroup group = groups[i];
            KillFadesOn(group);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    private IEnumerator FadeRow(int rowIndex, float targetAlpha)
    {
        if (bitRows == null || rowIndex < 0 || rowIndex >= bitRows.Length)
            yield break;

        IntroBitRow row = bitRows[rowIndex];
        if (row == null)
            yield break;

        List<CanvasGroup> groups = CollectFadeGroups(row);
        if (groups.Count == 0 || rowFadeDuration <= 0f)
        {
            for (int i = 0; i < groups.Count; i++)
                groups[i].alpha = targetAlpha;
            yield break;
        }

        bool done = false;
        Tween lead = StartFade(groups, targetAlpha, () => done = true);
        if (lead == null)
        {
            for (int i = 0; i < groups.Count; i++)
                groups[i].alpha = targetAlpha;
            yield break;
        }

        while (!done)
            yield return null;
    }

    private Tween StartFade(List<CanvasGroup> groups, float targetAlpha, Action onComplete = null)
    {
        if (groups == null || groups.Count == 0)
        {
            onComplete?.Invoke();
            return null;
        }

        Tween lead = null;
        for (int i = 0; i < groups.Count; i++)
        {
            CanvasGroup group = groups[i];
            KillFadesOn(group);
            Tween tween = group.DOFade(targetAlpha, rowFadeDuration)
                .SetUpdate(true)
                .SetTarget(group);
            _activeFades.Add(tween);
            if (lead == null)
                lead = tween;
        }

        if (lead != null && onComplete != null)
            lead.OnComplete(() => onComplete());

        return lead;
    }

    private void KillFadesOn(CanvasGroup group)
    {
        if (group == null)
            return;

        DOTween.Kill(group);
        for (int i = _activeFades.Count - 1; i >= 0; i--)
        {
            Tween tween = _activeFades[i];
            if (tween == null || !tween.IsActive() || tween.target == (object)group)
                _activeFades.RemoveAt(i);
        }
    }

    private void StopTransition()
    {
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        _transitioning = false;

        for (int i = 0; i < _activeFades.Count; i++)
        {
            Tween tween = _activeFades[i];
            if (tween != null && tween.IsActive())
                tween.Kill();
        }

        _activeFades.Clear();
    }

    private List<CanvasGroup> CollectFadeGroups(IntroBitRow row)
    {
        var groups = new List<CanvasGroup>();
        if (row == null)
            return groups;

        if (row.canvasGroup != null)
        {
            groups.Add(row.canvasGroup);
            return groups;
        }

        if (row.textBit != null)
            groups.Add(GetOrAddCanvasGroup(row.textBit.gameObject));

        if (row.optionButtons != null)
        {
            for (int i = 0; i < row.optionButtons.Length; i++)
            {
                Button button = row.optionButtons[i];
                if (button != null)
                    groups.Add(GetOrAddCanvasGroup(button.gameObject));
            }
        }

        return groups;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup group = go.GetComponent<CanvasGroup>();
        if (group == null)
            group = go.AddComponent<CanvasGroup>();
        return group;
    }

    private bool IsRowRootExclusive(int rowIndex)
    {
        if (bitRows == null || rowIndex < 0 || rowIndex >= bitRows.Length)
            return false;

        GameObject root = bitRows[rowIndex]?.rowRoot;
        if (root == null)
            return false;

        for (int i = 0; i < bitRows.Length; i++)
        {
            if (i == rowIndex || bitRows[i] == null)
                continue;
            if (bitRows[i].rowRoot == root)
                return false;
        }

        return true;
    }

    private static bool HasAnyOptionButton(IntroBitRow row)
    {
        if (row?.optionButtons == null)
            return false;

        for (int i = 0; i < row.optionButtons.Length; i++)
        {
            if (row.optionButtons[i] != null)
                return true;
        }

        return false;
    }

    private void ResetAllRows()
    {
        StopTransition();
        _isChoosing = false;
        _activeRowIndex = -1;

        if (bitRows == null)
            return;

        for (int r = 0; r < bitRows.Length; r++)
        {
            IntroBitRow row = bitRows[r];
            if (row == null)
                continue;

            HideAllOptions(row);

            if (row.textBit != null)
            {
                row.textBit.text = "";
                row.textBit.gameObject.SetActive(false);
            }

            if (row.rowRoot != null)
                row.rowRoot.SetActive(false);

            List<CanvasGroup> groups = CollectFadeGroups(row);
            for (int i = 0; i < groups.Count; i++)
            {
                CanvasGroup group = groups[i];
                KillFadesOn(group);
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }
        }
    }

    private void Finish(bool invokeCallback)
    {
        StopTransition();

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

    private void OnDestroy()
    {
        StopTransition();
    }
}
