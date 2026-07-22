using System.Collections;
using FMOD.Studio;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int IntroCinematicIndex = 0;
    public const int EscapeEndingCinematicIndex = 1;
    public const int ConfessionEndingCinematicIndex = 2;
    public const int CompletionEndingCinematicIndex = 3;

    public static GameManager Instance { get; private set; }

    [SerializeField] private DialogueTrigger introTrigger;
    [SerializeField] private bool delayOneFrame = true;

    [Header("Cinematics")]
    [Tooltip("0 = intro, 1 = escapeEnding, 2 = confessionEnding, 3 = CompletionEnding")]
    [SerializeField] private GameObject[] cinematics;

    [Tooltip("Seconds to keep a cutscene active (intro after exit, and ending cutscenes from Ink).")]
    [SerializeField] private float cinematicDuration = 60f;

    [Tooltip("Shown after any ending cutscene (1–3) finishes.")]
    [SerializeField] private GameObject creditsObject;

    [Header("Audio")]
    [Tooltip("SoundLibrary key for the looping ambience started when this scene begins.")]
    [SerializeField] private string soundscapeKey = "soundscape";

    bool _waitingForIntroExit;
    Coroutine _cutsceneRoutine;
    EventInstance _soundscapeInstance;

    /// <summary>
    /// True while intro cinematic (0) or any ending cutscene (1–3) is running.
    /// </summary>
    public bool IsCutscenePlaying => _cutsceneRoutine != null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager: more than one instance in scene.");
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // BakedLightingController owns pause/resume during blackout; skip if already dark.
        if (BakedLightingController.Instance == null || !BakedLightingController.Instance.IsBlackout)
            StartSoundscape();

        if (delayOneFrame)
            StartCoroutine(StartIntroNextFrame());
        else
            StartIntroSequence();
    }

    private void OnDestroy()
    {
        UnsubscribeIntroExit();
        StopSoundscape();
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
    /// Ink EXTERNAL: show ending cutscene by cinematic index for <see cref="cinematicDuration"/> seconds.
    /// 1 = escapeEnding, 2 = confessionEnding, 3 = CompletionEnding.
    /// </summary>
    public void PlayEndingCutscene(int cinematicIndex) => PlayCutscene(cinematicIndex);

    /// <summary>
    /// Show a cinematic for <see cref="cinematicDuration"/> seconds, then disable it.
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
        for (int i = 0; i < (cinematics != null ? cinematics.Length : 0); i++)
        {
            if (i != cinematicIndex)
                DeactivateCinematic(i);
        }

        if (cinematicIndex != IntroCinematicIndex)
            TryPlayMusicOutro();

        ActivateCinematic(cinematicIndex);
        yield return new WaitForSeconds(cinematicDuration);
        DeactivateCinematic(cinematicIndex);
        _cutsceneRoutine = null;

        if (cinematicIndex != IntroCinematicIndex)
            ShowCredits();
    }

    void ShowCredits()
    {
        if (creditsObject == null)
            return;

        creditsObject.SetActive(true);
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

        if (SoundManager.Instance == null)
            return;

        if (SoundManager.Instance.TryStartInstance("musicOutro", out _))
            return;

        SoundManager.PlayOneShot("musicOutro");
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
