using UnityEngine;

public class InkStoryCommands : MonoBehaviour
{
    public void ChangeCamera(string cameraId)
    {
        if (CameraManager.Instance == null)
        {
            Debug.LogWarning($"ChangeCamera: No CameraManager in scene. Requested '{cameraId}'.");
            return;
        }

        if (!CameraManager.Instance.TransitionToCamera(cameraId))
            Debug.LogWarning($"ChangeCamera: Failed to transition to '{cameraId}'.");
    }

    public void TriggerAnimation(string targetId, string animationName)
    {
        Debug.Log($"TriggerAnimation: {targetId} / {animationName}");
    }

    public void PlayAudioClip(string soundKey)
    {
        Debug.Log($"PlayAudioClip: {soundKey}");
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
