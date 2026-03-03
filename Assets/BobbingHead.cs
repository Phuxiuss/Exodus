using System.Collections.Generic;
using UnityEngine;

public class BobbingHead : MonoBehaviour
{
    [SerializeField] private bool enable = true;

    [Header("___________________Walk bobbing__________________")]
    [SerializeField] private float horizontalWalkAmplitude = 0.02f;
    [SerializeField] private float verticalWalkAmplitude = 0.02f;
    [SerializeField, Range(0, 30)] private float walkFrequncy = 10.0f;

    // Head bobbing
    [Header("___________________Run bobbing__________________")]

    [SerializeField] private float horizontalRunAmplitude = 0.02f;
    [SerializeField] private float verticalRunAmplitude = 0.02f;
    [SerializeField, Range(0, 30)] private float runFrequncy = 10.0f;
    [SerializeField] private float bobbingResetTime = 10f;
    [SerializeField] private float scopingHeadBobbingRatio = 0.3f;

    private float currentFrequency;
    private float currentHorizontalAmplitude;
    private float currentVerticalAmplitude;
    private float currentScopingHeadBobbingRatio = 1;


    [Header("___________________Falling camera shake effect__________________")]

    // Falling effect
    [SerializeField] private float fallingEffectResetTime = 10f;
    [SerializeField] private float initialFallingEffectTime = 10f;
    [SerializeField] Quaternion shakeRotation;

    private float currentInitialFallingTime =34;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }
    private void Update()
    {
        UpdateFallingEffect();
        UpdateCurrentInitialFallingEffectTime();
        if (enable)
        {
            UpdateHeadBobbing();
 
        }
        ResetHeadBobbing();
    }

    private Vector3 UpdateHeadBobbing()
    {
        Vector3 position = Vector3.zero;
        position.y += Mathf.Lerp(position.y, Mathf.Sin(Time.time * currentFrequency) * currentVerticalAmplitude, bobbingResetTime * Time.deltaTime);
        position.x += Mathf.Lerp(position.x, Mathf.Cos(Time.time * currentFrequency / 2f) * currentHorizontalAmplitude, bobbingResetTime * Time.deltaTime);
        transform.localPosition += position;

        return position;
    }

    private void ResetHeadBobbing()
    {
        if (transform.localPosition == startPosition) return;
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition, 1 * Time.deltaTime);
    }

    private void UpdateHeadBobbingStats(bool playerIsRunning)
    {
        if (playerIsRunning)
        {
            currentFrequency = runFrequncy;
            currentHorizontalAmplitude = horizontalRunAmplitude * currentScopingHeadBobbingRatio;
            currentVerticalAmplitude = verticalRunAmplitude * currentScopingHeadBobbingRatio;
        }
        else
        {
            currentFrequency = walkFrequncy;
            currentHorizontalAmplitude = horizontalWalkAmplitude * currentScopingHeadBobbingRatio;
            currentVerticalAmplitude = verticalWalkAmplitude * currentScopingHeadBobbingRatio;
        }
    }

    private void UpdateHeadBobbingStats()
    {
        currentFrequency = walkFrequncy;
        currentHorizontalAmplitude = horizontalWalkAmplitude * currentScopingHeadBobbingRatio;
        currentVerticalAmplitude = verticalWalkAmplitude * currentScopingHeadBobbingRatio;
    }

    private void UpdateFallingEffect()
    {
        if(transform.localRotation == Quaternion.identity)
        {
            transform.localRotation = Quaternion.identity;
            return;
        }
        transform.localRotation = Quaternion.Lerp(transform.localRotation, startRotation, 1/ fallingEffectResetTime * Time.deltaTime);
    }
   
    public void OnTriggerFallingEffect()
    {
        currentInitialFallingTime = 0;
    }

    private void UpdateInitialFallingEffect()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, shakeRotation, 1 / initialFallingEffectTime * Time.deltaTime);
    }
    
    private void UpdateCurrentInitialFallingEffectTime()
    {
        if (currentInitialFallingTime < initialFallingEffectTime)
        {
            currentInitialFallingTime += Time.deltaTime;
            UpdateInitialFallingEffect();
        }
        else
        {
           // ResetHeadBobbing();
        }
    }

    public void OnEnableHeadBobbing(bool enable, bool playerIsRunning)
    {
        this.enable = enable;
        UpdateHeadBobbingStats(playerIsRunning);
    }

    public void SetHeadBobbingToScoping(bool scoping, float zoomInDuration)
    {
        if (scoping)
        {
            currentScopingHeadBobbingRatio = scopingHeadBobbingRatio;
        }
        else
        {
            currentScopingHeadBobbingRatio = 1;
        }

        UpdateHeadBobbingStats();
    }
}
