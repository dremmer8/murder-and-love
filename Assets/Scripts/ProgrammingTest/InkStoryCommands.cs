using UnityEngine;

public class InkStoryCommands : MonoBehaviour
{
    public void ChangeCamera(string cameraId)
    {
        if (CutsceneDialogueCameraManager.Instance != null)
        {
            CutsceneDialogueCameraManager.Instance.ChangeCamera(cameraId);
            return;
        }

        if (CameraManager.Instance == null)
        {
            Debug.LogWarning($"ChangeCamera: No CutsceneDialogueCameraManager / CameraManager in scene. Requested '{cameraId}'.");
            return;
        }

        if (!CameraManager.Instance.TransitionToCamera(cameraId))
            Debug.LogWarning($"ChangeCamera: Failed to transition to '{cameraId}'.");
    }

    public void TriggerAnimation(string targetId, string animationName)
    {
        DialogueAnimationTargets targets = DialogueAnimationTargets.Instance;
        if (targets == null)
            targets = FindFirstObjectByType<DialogueAnimationTargets>();

        if (targets == null)
        {
            Debug.LogWarning(
                $"TriggerAnimation: No DialogueAnimationTargets in scene. Requested '{targetId}' / '{animationName}'.",
                this);
            return;
        }

        targets.Trigger(targetId, animationName);
    }

    public void PlayAudioClip(string soundKey)
    {
        if (string.IsNullOrWhiteSpace(soundKey))
        {
            Debug.LogWarning("PlayAudioClip: Empty sound key.", this);
            return;
        }

        if (!SoundManager.PlayOneShot(soundKey.Trim()))
            Debug.LogWarning($"PlayAudioClip: Failed to play '{soundKey}'.", this);
    }

    public void SetMandyAffection(int value)
    {
        Debug.Log($"SetMandyAffection: {value}");
    }

    public void SetStoryAct(int act)
    {
        Debug.Log($"SetStoryAct: {act}");
    }

    public void SetActProgression(int progress)
    {
        Debug.Log($"SetActProgression: {progress}");
    }
}
