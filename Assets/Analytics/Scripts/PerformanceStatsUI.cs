using UnityEngine;
using TMPro;
using UnityEngine.Profiling;
using System;
using System.Collections;

public class PerformanceStatsUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI statsText;

    private float deltaTime;
    private int frameCount;
    private float updateRate = 4.0f; // updates per second

    private bool showStats = false;

    void Start()
    {
        if (statsText == null)
        {
            Debug.LogWarning("⚠️ StatsText is not assigned.");
            enabled = false;
            return;
        }

        InvokeRepeating(nameof(UpdateStats), 0f, 1f / updateRate);
    }

    void Update()
    {
        DetectThreeFingerTap();
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        frameCount++;
    }

    void UpdateStats()
    {
        if (!showStats) {
            statsText.text = "";
            return;
        }

        float fps = 1.0f / deltaTime;
        float memory = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);


        string text =
            $"<b>📊 Performance Stats</b>\n" +
            $"🕹️ FPS: <b>{fps:0.}</b>\n" +
            $"🧠 RAM: <b>{memory:0.0} MB</b>\n" +
            $"🎮 GPU: {SystemInfo.graphicsDeviceName}\n" +
            $"🧬 CPU: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)\n" +
            $"📱 Screen: {Screen.width}x{Screen.height} @ {Screen.dpi} DPI\n" +
            $"🔋 Battery: {(SystemInfo.batteryLevel * 100f):0}% | {SystemInfo.batteryStatus}\n" +
            $"📶 Network: {Application.internetReachability}\n" +
            $"💾 Data Path: {Application.persistentDataPath}";

        statsText.text = text;
    }

    void DetectThreeFingerTap()
    {
        if (Input.touchCount == 3)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            Touch touch2 = Input.GetTouch(2);

            if (touch0.phase == TouchPhase.Began ||
                touch1.phase == TouchPhase.Began ||
                touch2.phase == TouchPhase.Began)
            {
                showStats = !showStats;
            }
        }
    }
}
