using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBobSystem : MonoBehaviour {

    [Header("General Settings")]
    public bool useDynamicSpeed = true;
    [Range(10f, 100f)]
    public float smooth = 10.0f;
    [Range(1f, 20f)]
    public float resetSpeed = 8.0f;

    [Header("Vertical Bobbing")]
    public float verticalAmount = 0.005f;
    public float verticalFrequency = 10.0f;

    [Header("Horizontal Bobbing")]
    public float horizontalAmount = 0.003f;
    public float horizontalFrequency = 5.0f;

    private Vector3 startPos;
    private float timer;

    private void Start() {
        startPos = transform.localPosition;
    }

    private void Update() {
        CheckHeadBob();
    }

    private void CheckHeadBob() {
        Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        float inputMagnitude = Mathf.Clamp01(inputVector.magnitude);

        if (inputMagnitude > 0.01f) {
            CalculateHeadBob(inputMagnitude);
        } else {
            ResetHeadBob();
        }
    }

    private void CalculateHeadBob(float magnitude) {
        float currentSpeed = useDynamicSpeed ? magnitude : 1f;
        timer += Time.deltaTime * currentSpeed;

        Vector3 targetPosition = startPos;
        targetPosition.y += Mathf.Sin(timer * verticalFrequency) * verticalAmount;
        targetPosition.x += Mathf.Cos(timer * horizontalFrequency) * horizontalAmount;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smooth * Time.deltaTime);
    }

    private void ResetHeadBob() {
        timer = 0f;
        if (transform.localPosition != startPos) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetSpeed * Time.deltaTime);
        }
    }
}