using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drop on a UI <see cref="Button"/> to play the SoundLibrary <c>click</c> event on press.
/// Auto-wires <see cref="Button.onClick"/>; you can also call <see cref="PlayClick"/> from OnClick.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour
{
    [SerializeField]
    [Tooltip("SoundLibrary key. Defaults to FMOD event click.")]
    string m_SoundKey = "click";

    [SerializeField]
    [Tooltip("If true, listens to Button.onClick automatically.")]
    bool m_HookButtonOnClick = true;

    Button m_Button;

    void Awake()
    {
        m_Button = GetComponent<Button>();
    }

    void OnEnable()
    {
        if (m_HookButtonOnClick && m_Button != null)
            m_Button.onClick.AddListener(PlayClick);
    }

    void OnDisable()
    {
        if (m_Button != null)
            m_Button.onClick.RemoveListener(PlayClick);
    }

    public void PlayClick()
    {
        if (string.IsNullOrWhiteSpace(m_SoundKey))
            return;

        SoundManager.PlayOneShot(m_SoundKey.Trim());
    }
}
