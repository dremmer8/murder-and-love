using UnityEngine;

/// <summary>
/// Marks a subtree whose TMP labels keep the font, material and size authored in the scene, so
/// <see cref="LocalizedFontApplier"/> leaves them alone. Meant for text that is never localized and
/// needs glyphs the locale font lacks — the credits roles, where the locale font carries no Ö.
/// </summary>
[DisallowMultipleComponent]
public class KeepAuthoredFont : MonoBehaviour
{
}
