using UnityEngine;

public class FPS_Limiter : MonoBehaviour
{
    [SerializeField]
    private int targetFPS = 60;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        QualitySettings.vSyncCount = 0; // Disable VSync
        Application.targetFrameRate = targetFPS;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
