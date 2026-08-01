/// <summary>
/// Stable CSV keys for built-in UI strings (control hints, pager, menus).
/// </summary>
public static class LocalizationKeys
{
    public const string HintInteract = "hint.interact";
    public const string HintTalk = "hint.talk";
    public const string HintCheckPager = "hint.check_pager";
    public const string HintMinigameFallback = "hint.minigame_fallback";
    public const string HintPagerOpen = "hint.pager_open";
    public const string HintPagerTutorialScroll = "hint.pager_tutorial_scroll";
    public const string HintPagerTutorialAdvance = "hint.pager_tutorial_advance";
    public const string HintPagerRespondReading = "hint.pager_respond_reading";
    public const string HintPagerRespondTyping = "hint.pager_respond_typing";
    public const string HintPagerRespondTutorialScroll = "hint.pager_respond_tutorial_scroll";
    public const string HintPagerRespondTutorialAdvance = "hint.pager_respond_tutorial_advance";
    public const string HintDialogueProgress = "hint.dialogue_progress";
    public const string HintDialogueChoice = "hint.dialogue_choice";

    public const string PagerEmpty = "pager.empty";
    public const string PagerNewMessage = "pager.new_message";
    public const string PagerStartTyping = "pager.start_typing";
    public const string PagerRespondReply = "pager.respond_reply";

    public const string MenuPlay = "menu.play";
    public const string MenuResume = "menu.resume";
    public const string MenuOptions = "menu.options";
    public const string MenuSound = "menu.sound";
    public const string MenuMusic = "menu.music";
    public const string MenuLanguage = "menu.language";
    public const string MenuBack = "menu.back";
    public const string MenuExit = "menu.exit";
    public const string MenuExitToMenu = "menu.exit_to_menu";

    public static string Task(int index) => $"task.{index}";

    public static string MinigameControls(string objectName) =>
        $"minigame.{Sanitize(objectName)}.controls";

    public static string MinigameStep(string objectName, string stepId) =>
        $"minigame.{Sanitize(objectName)}.step.{Sanitize(stepId)}";

    static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "unnamed";

        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!(char.IsLetterOrDigit(c) || c == '_' || c == '-'))
                chars[i] = '_';
        }

        return new string(chars);
    }
}
