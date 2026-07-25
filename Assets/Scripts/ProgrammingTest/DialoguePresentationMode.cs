/// <summary>
/// How dialogue content is shown after a <see cref="DialogueTrigger"/> starts.
/// Separate from <see cref="DialogueActivationMode"/> (how it is started).
/// </summary>
public enum DialoguePresentationMode
{
    /// <summary>Classic panel: Space to advance, choices as buttons, locks player.</summary>
    Standard = 0,

    /// <summary>
    /// Intro-style: one row at a time, fading in from transparent. After a row is done,
    /// wait then hide it and fade in the next. Locks player.
    /// </summary>
    IntroSequence = 1,

    /// <summary>
    /// Internal thought: timed by character count, Space does not skip,
    /// player keeps moving (no GameState lock).
    /// </summary>
    InternalMonologue = 2,

    /// <summary>
    /// Jason pager inbox: messages live on the pager. Tab opens (locks movement,
    /// look stays free), A/D scroll, D at end advances. Re-openable
    /// until a new Jason conversation replaces it.
    /// </summary>
    Pager = 3
}
