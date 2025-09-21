using UnityEngine;
using Unity.Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    private float _shakeTimer;
    private float _totalShakeDuration;
    private float _startingIntensity;

    // Awake is called when the script instance is being loaded
    // This is where we initialize the CinemachineBasicMultiChannelPerlin component
    void Awake()
    {
        cinemachineBasicMultiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
     public void ShakeCamera(float intensity, float duration)
    {
        cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;
        _startingIntensity = intensity;
        _totalShakeDuration = duration;
        _shakeTimer = duration;
    }
    // Update is called once per frame
     private void Update()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;
            if (_shakeTimer <= 0)
            {
                cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0f;
            }
        }
    }
}
