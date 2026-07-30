using System;
using System.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public const int IntroCinematicIndex = 0;
    public const int EscapeEndingCinematicIndex = 1;
    public const int ConfessionEndingCinematicIndex = 2;
    public const int CompletionEndingCinematicIndex = 3;

    public static GameManager Instance { get; private set; }

    [SerializeField] private DialogueTrigger introTrigger;
    [Tooltip("If true, waits one frame before starting the intro (can flash the gameplay view). Leave off to show the intro immediately.")]
    [SerializeField] private bool delayOneFrame = false;

    [Header("Main Menu")]
    [Tooltip("Shown on load; deactivated when Start is pressed.")]
    [SerializeField] private GameObject mainMenuCanvas;

    [Tooltip("Kept inactive until Start; then enabled before the intro sequence.")]
    [SerializeField] private GameObject playerObject;

    [Header("Cinematics")]
    [Tooltip("0 = intro, 1 = escapeEnding, 2 = confessionEnding, 3 = CompletionEnding")]
    [SerializeField] private GameObject[] cinematics;

    [Tooltip("Fallback seconds if a cinematic has no PlayableDirector / Timeline. Cutscene length otherwise follows the Timeline.")]
    [SerializeField] private float cinematicDuration = 60f;

    [Tooltip("Shown after any ending cutscene (1–3) finishes.")]
    [SerializeField] private GameObject creditsObject;

    [Tooltip("Opened in the system browser after the credits delay.")]
    [SerializeField] private string surveyUrl = "https://forms.gle/UcCQp4xbSwmSaTvLA";

    [Tooltip("Seconds to wait after credits appear before opening the survey.")]
    [SerializeField] private float surveyOpenDelay = 10f;

    [Header("Completion Ending")]
    [Tooltip("Standard dialogue trigger for Boyfriend_ending_dialogue_final (story phase 31).")]
    [SerializeField] private DialogueTrigger completionEpilogueDialogueTrigger;

    [Tooltip("Seconds into the completion Timeline before phase-31 lines begin over the animation.")]
    [SerializeField] private float completionEpilogueStartDelay = 6f;

    [Tooltip("Seconds each line stays on screen during the completion epilogue.")]
    [SerializeField] private float completionEpilogueLineDuration = 5f;

    [Tooltip("Completion Timeline clip length when the asset reports infinite duration (post-extrapolation hold).")]
    [SerializeField] private float completionTimelineDuration = 57.583f;

    [Header("Audio")]
    [Tooltip("SoundLibrary key for the looping ambience started when this scene begins.")]
    [SerializeField] private string soundscapeKey = "soundscape";

    bool _waitingForIntroExit;
    bool _gameStarted;
    Coroutine _cutsceneRoutine;
    bool _endingCutsceneActive;
    bool _endingReached;
    EventInstance _soundscapeInstance;
    EventInstance _musicOutroInstance;

    /// <summary>
    /// True while intro cinematic (0) or any ending cutscene (1–3) is running.
    /// </summary>
    public bool IsCutscenePlaying => _cutsceneRoutine != null;

    /// <summary>True while escape / confession / completion ending cutscene (1–3) is running.</summary>
    public bool IsEndingCutscenePlaying => _cutsceneRoutine != null && _endingCutsceneActive;

    /// <summary>
    /// Sticky once any ending cutscene (1–3) begins. Stays true through credits so
    /// Mandy→Jason pager chains cannot fire after <see cref="IsCutscenePlaying"/> clears.
    /// </summary>
    public bool HasReachedEnding => _endingReached;

    /// <summary>True after the main-menu Start button has begun the game.</summary>
    public bool HasStartedFromMainMenu => _gameStarted;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager: more than one instance in scene.");
            Destroy(this);
            return;
        }

        Instance = this;
        PrepareMainMenu();
    }

    private void Start()
    {
        // Keep the menu in blackout even if BakedLightingController startState was left on LightsOn.
        if (!_gameStarted)
            ApplyMainMenuLighting();

        // BakedLightingController owns pause/resume during blackout; skip if already dark.
        if (BakedLightingController.Instance == null || !BakedLightingController.Instance.IsBlackout)
            StartSoundscape();

        // Intro waits for the main-menu Start button (see UI_StartGame).
    }

    /// <summary>
    /// Hook for Main Menu Start button: hide menu, enable player, run intro as usual.
    /// </summary>
    public void UI_StartGame()
    {
        if (_gameStarted)
            return;

        _gameStarted = true;
        GameAnalytics.StartSession();

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);

        if (playerObject != null)
            playerObject.SetActive(true);

        ApplyGameplayLighting();

        if (delayOneFrame)
            StartCoroutine(StartIntroNextFrame());
        else
            StartIntroSequence();
    }

    void PrepareMainMenu()
    {
        if (playerObject != null)
            playerObject.SetActive(false);

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Blackout baked lighting while the main menu is up.</summary>
    void ApplyMainMenuLighting()
    {
        BakedLightingController lighting = BakedLightingController.Instance;
        if (lighting == null)
            return;

        lighting.ApplyState(BakedLightingController.LightingState.Blackout, immediate: true);
    }

    /// <summary>Restore lights-on when the player leaves the main menu.</summary>
    void ApplyGameplayLighting()
    {
        BakedLightingController lighting = BakedLightingController.Instance;
        if (lighting == null)
            return;

        lighting.ApplyLightsOn();
    }

    private void OnDestroy()
    {
        UnsubscribeIntroExit();
        StopSoundscape();
        StopMusicOutro();
        if (Instance == this)
            Instance = null;
    }

    private IEnumerator StartIntroNextFrame()
    {
        yield return null;
        StartIntroSequence();
    }

    public void StartIntroSequence()
    {
        if (introTrigger == null
            || GlobalVariableOperator.Instance == null
            || GlobalVariableOperator.Instance.GameProgression != 0)
            return;

        SubscribeIntroExit();

        introTrigger.ActivationMode = DialogueActivationMode.ExternalEvent;
        introTrigger.PresentationMode = DialoguePresentationMode.IntroSequence;
        introTrigger.TryStartDialogue();
    }

    /// <summary>
    /// Ink EXTERNAL: show ending cutscene by cinematic index for the Timeline duration
    /// (falls back to <see cref="cinematicDuration"/> if no director/asset).
    /// 1 = escapeEnding, 2 = confessionEnding, 3 = CompletionEnding.
    /// </summary>
    public void PlayEndingCutscene(int cinematicIndex) => PlayCutscene(cinematicIndex);

    /// <summary>
    /// Show a cinematic for its Timeline duration, then disable it.
    /// </summary>
    public void PlayCutscene(int cinematicIndex)
    {
        if (_cutsceneRoutine != null)
            StopCoroutine(_cutsceneRoutine);

        _cutsceneRoutine = StartCoroutine(PlayCutsceneRoutine(cinematicIndex));
    }

    /// <summary>
    /// Enable cinematic by index: 0 intro, 1 escapeEnding, 2 confessionEnding, 3 CompletionEnding.
    /// </summary>
    public void ActivateCinematic(int index) => SetCinematicActive(index, true);

    /// <summary>
    /// Disable cinematic by index: 0 intro, 1 escapeEnding, 2 confessionEnding, 3 CompletionEnding.
    /// </summary>
    public void DeactivateCinematic(int index) => SetCinematicActive(index, false);

    public void BindInkExternals(Ink.Runtime.Story story)
    {
        if (story == null)
            return;

        story.BindExternalFunction("PlayEndingCutscene", (int cinematicIndex) => PlayEndingCutscene(cinematicIndex));
    }

    void SetCinematicActive(int index, bool active)
    {
        if (cinematics == null || index < 0 || index >= cinematics.Length)
            return;

        if (cinematics[index] == null)
            return;

        cinematics[index].SetActive(active);
    }

    IEnumerator PlayCutsceneRoutine(int cinematicIndex)
    {
        bool isEnding = cinematicIndex != IntroCinematicIndex;
        _endingCutsceneActive = isEnding;

        for (int i = 0; i < (cinematics != null ? cinematics.Length : 0); i++)
        {
            if (i != cinematicIndex)
                DeactivateCinematic(i);
        }

        if (isEnding)
        {
            _endingReached = true;
            GameAnalytics.RecordEndingReached(cinematicIndex);

            TryPlayMusicOutro();

            // Hide the player for ending cutscenes (escape / confession / completion).
            if (playerObject != null)
                playerObject.SetActive(false);

            // Mandy escape can still chain into Jason's pager on dialogue end — kill it.
            SuppressPagerDuringEnding();
        }

        // Jason completion ending: freeze every washer so the laundromat reads as still.
        if (cinematicIndex == CompletionEndingCinematicIndex)
            DoWorkTrigger.StopAllWork();

        ActivateCinematic(cinematicIndex);

        PlayableDirector director = GetCinematicDirector(cinematicIndex);
        if (director != null)
        {
            director.time = 0;
            director.Evaluate();
            director.Play();
        }

        float duration = ResolveCinematicDuration(director);

        if (!isEnding)
        {
            yield return new WaitForSeconds(duration);

            if (director != null && director.state == PlayState.Playing)
                director.Stop();

            DeactivateCinematic(cinematicIndex);
            _cutsceneRoutine = null;

            if (playerObject != null)
                playerObject.SetActive(true);
            yield break;
        }

        if (cinematicIndex == CompletionEndingCinematicIndex)
        {
            yield return PlayCompletionEndingRoutine(director, duration);
            yield break;
        }

        // Escape / confession: credits as soon as the cutscene dialogue ends.
        yield return WaitForEndingDialogueToFinish();

        if (director != null && director.state == PlayState.Playing)
            director.Stop();

        DeactivateCinematic(cinematicIndex);
        FinishEndingCutscene();
    }

    void FinishEndingCutscene()
    {
        _endingCutsceneActive = false;
        _cutsceneRoutine = null;

        DialogueManager dialogue = DialogueManager.GetInstance();
        if (dialogue != null && dialogue.dialogueIsPlaying)
            dialogue.AbortActivePresentation();

        VoiceOverOperator voice = VoiceOverOperator.Instance;
        if (voice != null)
            voice.StopPlayback();

        // Dialogue end schedules nextDialogueTrigger one frame later; by then IsCutscenePlaying
        // is already false. Kill any Mandy→Jason pager that slips through before that check.
        SuppressPagerDuringEnding();
        ShowCredits();
    }

    IEnumerator WaitForEndingDialogueToFinish()
    {
        while (IsStandardDialoguePlaying())
            yield return null;
    }

    static bool IsStandardDialoguePlaying()
    {
        DialogueManager dialogue = DialogueManager.GetInstance();
        return dialogue != null
            && dialogue.dialogueIsPlaying
            && dialogue.ActiveMode == DialoguePresentationMode.Standard;
    }

    /// <summary>
    /// Completion ending: animation plays fully; Jason lines overlay from 6s; credits when Timeline ends.
    /// </summary>
    IEnumerator PlayCompletionEndingRoutine(PlayableDirector director, float duration)
    {
        float timelineDuration = ResolveCompletionTimelineDuration(director, duration);
        float elapsed = 0f;
        bool epilogueStarted = false;
        bool epilogueScheduled = completionEpilogueDialogueTrigger != null;

        DialogueManager dialogue = DialogueManager.GetInstance();
        void OnDialogueEnded(string _) { }

        while (true)
        {
            elapsed += Time.deltaTime;
            float timelineTime = GetTimelineTime(director, elapsed);

            if (epilogueScheduled && !epilogueStarted && timelineTime >= completionEpilogueStartDelay)
            {
                epilogueStarted = TryBeginCompletionEpilogue(dialogue, OnDialogueEnded);
                if (!epilogueStarted)
                    epilogueScheduled = false;
            }

            if (timelineTime >= timelineDuration)
                break;

            yield return null;
        }

        StopCompletionEpilogue(dialogue, OnDialogueEnded, epilogueStarted);

        if (director != null && director.state == PlayState.Playing)
            director.Stop();

        DeactivateCinematic(CompletionEndingCinematicIndex);
        FinishEndingCutscene();
    }

    static float GetTimelineTime(PlayableDirector director, float fallbackElapsed)
    {
        if (director != null)
            return (float)director.time;

        return fallbackElapsed;
    }

    float ResolveCompletionTimelineDuration(PlayableDirector director, float resolvedFallback)
    {
        if (director?.playableAsset == null)
            return completionTimelineDuration > 0f ? completionTimelineDuration : resolvedFallback;

        double assetDuration = director.playableAsset.duration;
        if (assetDuration > 0d && !double.IsInfinity(assetDuration) && !double.IsNaN(assetDuration))
            return (float)assetDuration;

        if (completionTimelineDuration > 0f)
            return completionTimelineDuration;

        return resolvedFallback;
    }

    bool TryBeginCompletionEpilogue(DialogueManager dialogue, Action<string> onEnded)
    {
        if (completionEpilogueDialogueTrigger == null || dialogue == null)
            return false;

        dialogue.OnDialogueEnded += onEnded;
        dialogue.SetCompletionEpilogueMode(true, completionEpilogueLineDuration);

        if (completionEpilogueDialogueTrigger.TryStartDialogue())
            return true;

        dialogue.SetCompletionEpilogueMode(false, 0f);
        dialogue.OnDialogueEnded -= onEnded;
        Debug.LogWarning($"{name}: Failed to start completion epilogue dialogue.", this);
        return false;
    }

    void StopCompletionEpilogue(DialogueManager dialogue, Action<string> onEnded, bool epilogueStarted)
    {
        if (dialogue == null || !epilogueStarted)
            return;

        dialogue.OnDialogueEnded -= onEnded;
        dialogue.SetCompletionEpilogueMode(false, 0f);

        if (dialogue.dialogueIsPlaying)
            dialogue.AbortActivePresentation();
    }

    PlayableDirector GetCinematicDirector(int index)
    {
        if (cinematics == null || index < 0 || index >= cinematics.Length)
            return null;

        GameObject cinematic = cinematics[index];
        return cinematic != null ? cinematic.GetComponent<PlayableDirector>() : null;
    }

    /// <summary>
    /// Uses the Timeline / playable asset length when available; otherwise <see cref="cinematicDuration"/>.
    /// Prefers <see cref="PlayableAsset.duration"/> so infinite clip extrapolation cannot stall credits forever.
    /// </summary>
    float ResolveCinematicDuration(PlayableDirector director)
    {
        if (director == null)
            return Mathf.Max(0f, cinematicDuration);

        double duration = 0d;
        if (director.playableAsset != null)
            duration = director.playableAsset.duration;

        if (duration <= 0d || double.IsInfinity(duration) || double.IsNaN(duration))
            duration = director.duration;

        if (duration > 0d && !double.IsInfinity(duration) && !double.IsNaN(duration))
            return (float)duration;

        return Mathf.Max(0f, cinematicDuration);
    }

    void SuppressPagerDuringEnding()
    {
        PagerTextController pager = PagerTextController.Instance;
        if (pager != null)
            pager.SuppressForEndingCutscene();
    }

    void ShowCredits()
    {
        AnimationSoundboard.StopSiren();

        if (creditsObject == null)
            return;

        creditsObject.SetActive(true);
        StartCoroutine(OpenSurveyAfterDelay());
    }

    IEnumerator OpenSurveyAfterDelay()
    {
        yield return new WaitForSeconds(surveyOpenDelay);

        if (string.IsNullOrWhiteSpace(surveyUrl))
            yield break;

        Application.OpenURL(surveyUrl.Trim());
    }

    /// <summary>Start the looping soundscape if it is not already playing.</summary>
    public void StartSoundscape()
    {
        if (_soundscapeInstance.isValid())
            return;

        if (string.IsNullOrWhiteSpace(soundscapeKey) || SoundManager.Instance == null)
            return;

        if (!SoundManager.Instance.TryStartInstance(soundscapeKey.Trim(), out _soundscapeInstance))
            return;
    }

    /// <summary>Stop and release the looping soundscape.</summary>
    public void StopSoundscape()
    {
        if (!_soundscapeInstance.isValid())
            return;

        _soundscapeInstance.stop(STOP_MODE.ALLOWFADEOUT);
        _soundscapeInstance.release();
        _soundscapeInstance.clearHandle();
    }

    void TryPlayMusicOutro()
    {
        StopSoundscape();
        StopMusicOutro();

        if (SoundManager.Instance == null)
            return;

        if (SoundManager.Instance.TryStartInstance("musicOutro", out _musicOutroInstance))
            return;

        SoundManager.PlayOneShot("musicOutro");
    }

    void StopMusicOutro()
    {
        if (!_musicOutroInstance.isValid())
            return;

        _musicOutroInstance.stop(STOP_MODE.ALLOWFADEOUT);
        _musicOutroInstance.release();
        _musicOutroInstance.clearHandle();
    }

    void SubscribeIntroExit()
    {
        DialogueManager manager = DialogueManager.GetInstance();
        if (manager == null)
            return;

        manager.OnDialogueEnded -= OnIntroDialogueEnded;
        manager.OnDialogueEnded += OnIntroDialogueEnded;
        _waitingForIntroExit = true;
    }

    void UnsubscribeIntroExit()
    {
        DialogueManager manager = DialogueManager.GetInstance();
        if (manager == null)
            return;

        manager.OnDialogueEnded -= OnIntroDialogueEnded;
        _waitingForIntroExit = false;
    }

    void OnIntroDialogueEnded(string _)
    {
        if (!_waitingForIntroExit)
            return;

        UnsubscribeIntroExit();
        PlayCutscene(IntroCinematicIndex);
    }
}
