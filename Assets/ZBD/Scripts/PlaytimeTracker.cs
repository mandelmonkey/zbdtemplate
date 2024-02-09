using UnityEngine;

public class PlaytimeTracker : MonoBehaviour
{
    private float lastActiveTime;
    private float totalPlayTime;
    private const float InactivityThreshold = 10f; // Set inactivity threshold (e.g., 10 seconds)
    public static PlaytimeTracker Instance;

    private void Awake()
    {
        Instance = this;
        totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0);

    }

    private void OnDestroy()
    {
        SavePlayTime();
        Instance = null;
    }

    void OnApplicationQuit()
    {
        SavePlayTime();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SavePlayTime();
        }
    }

    void Update()
    {
        // Check for user interaction
        if (Input.anyKeyDown || Input.touchCount > 0)
        {

            lastActiveTime = Time.unscaledTime;
        }

        // Update total playtime if within active threshold
        if (Time.unscaledTime - lastActiveTime < InactivityThreshold)
        {
            totalPlayTime += Time.unscaledDeltaTime;
        }
    }

    private void SavePlayTime()
    {
        // Save the updated total playtime
        PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime);
    }

    public float GetTotalPlayTime()
    {
        return totalPlayTime;
    }

    public long GetTotalPlayTimeMins()
    {
        return (long)(totalPlayTime / 60);
    }
}
