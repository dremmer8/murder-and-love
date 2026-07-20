/// <summary>
/// Optional provider implemented by minigame operators so
/// <see cref="MinigameStepHintPresenter"/> can show a world-space hint for the
/// current step. Step ids must match entries configured on the sibling
/// <see cref="MinigameActivator"/>.
/// </summary>
public interface IMinigameStepHintSource
{
    /// <summary>
    /// Returns true when a step hint should be shown. <paramref name="stepId"/> is a
    /// stable key matching a <see cref="MinigameStepHintEntry.stepId"/> on the active
    /// <see cref="MinigameActivator"/>.
    /// </summary>
    bool TryGetCurrentStepHintId(out string stepId);
}
