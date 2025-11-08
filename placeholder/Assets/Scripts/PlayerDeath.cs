using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Needed to reload scenes

public class PlayerDeath : MonoBehaviour
{
    [Header("Rewind Settings")]
    public float maxRewindTime = 5f; // Maximum rewind duration in seconds

    [Header("References")]
    public GameObject playerGameObject;
    public PlayerStartInfo playerInfoScript;
    public Timer timerScript; // Reference to your Timer script

    private TimeControlled[] timeObjects;
    private bool isRewinding = false;
    private float rewindTimer = 0f;

    // Track why rewind started
    private bool triggeredByTimer = false;

    private void Awake()
    {
        // Find all objects that are time-controlled
        timeObjects = FindObjectsOfType<TimeControlled>();
        
        // Debug: Check if references are set
        Debug.Log("PlayerDeath Awake - timerScript assigned: " + (timerScript != null));
        Debug.Log("PlayerDeath Awake - playerGameObject assigned: " + (playerGameObject != null));
    }

    private void Update()
    {
        // Timer-based rewind - check if timer just ran out
        if (timerScript != null)
        {
            // Debug every frame to see the timer value
            if (Time.frameCount % 50 == 0) // Log every 50 frames to avoid spam
            {
                Debug.Log($"Timer check: gameTime = {timerScript.gameTime}, isRewinding = {isRewinding}");
            }
            
            if (!isRewinding && timerScript.gameTime <= 0.01f)
            {
                Debug.Log("Timer expired! Starting rewind...");
                triggeredByTimer = true;
                StartRewind();
            }
        }
        else
        {
            if (Time.frameCount % 300 == 0) // Log occasionally
            {
                Debug.LogWarning("PlayerDeath: timerScript is NULL! Assign it in the Inspector.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hazard-based rewind
        if (!isRewinding && other.CompareTag("Hazard"))
        {
            triggeredByTimer = false;
            StartRewind();
        }
    }

    private void StartRewind()
    {
        if (isRewinding) return;

        Debug.Log("StartRewind called. Triggered by timer: " + triggeredByTimer);

        playerInfoScript = playerGameObject.GetComponent<PlayerStartInfo>();
        Vector3 startCoords = playerInfoScript.originalPosition;

        Debug.Log("Starting rewind to position: " + startCoords);

        StartCoroutine(RewindToStart(startCoords));
    }

    private IEnumerator RewindToStart(Vector3 startCoords)
    {
        isRewinding = true;

        // Force stop the timer while rewinding (in case it's still ticking)
        if (timerScript != null)
        {
            typeof(Timer)
                .GetField("stopTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(timerScript, true);
        }

        foreach (TimeControlled timeObject in timeObjects)
        {
            timeObject.StartRewind();
        }

        // Get the actual number of recorded frames from the player
        TimeControlled playerTimeControlled = playerGameObject.GetComponent<TimeControlled>();
        int actualFramesRecorded = 0;
        
        if (playerTimeControlled != null)
        {
            // Use reflection to get the actual positionHistory count
            var positionHistoryField = typeof(TimeControlled).GetField("positionHistory", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (positionHistoryField != null)
            {
                var historyList = positionHistoryField.GetValue(playerTimeControlled) as System.Collections.IList;
                if (historyList != null)
                {
                    actualFramesRecorded = historyList.Count;
                }
            }
        }

        // Calculate natural rewind time based on recorded frames (at 50fps)
        float naturalRewindTime = actualFramesRecorded / 50f;
        
        // Cap at maximum rewind time, but use natural time if shorter
        float targetRewindDuration = Mathf.Min(naturalRewindTime, maxRewindTime);
        
        // Calculate speed to rewind within target duration
        float framesPerSecond = (targetRewindDuration > 0) ? actualFramesRecorded / targetRewindDuration : 50f;
        float dynamicRewindSpeed = framesPerSecond / 50f;

        Debug.Log($"Rewind: {actualFramesRecorded} frames ({naturalRewindTime:F2}s natural), capped to {targetRewindDuration:F2}s at {dynamicRewindSpeed:F2}x speed");

        // Perform rewind until player reaches starting point
        while (Vector3.Distance(playerGameObject.transform.position, startCoords) > 0.05f)
        {
            rewindTimer += Time.deltaTime * dynamicRewindSpeed;

            while (rewindTimer >= Time.fixedDeltaTime)
            {
                foreach (TimeControlled timeObject in timeObjects)
                {
                    timeObject.TimedUpdate();
                }
                rewindTimer -= Time.fixedDeltaTime;
            }

            yield return null;
        }

        foreach (TimeControlled timeObject in timeObjects)
        {
            timeObject.StopRewind();
        }

        rewindTimer = 0f;
        isRewinding = false;

        // Snap player to start position
        playerGameObject.transform.position = startCoords;

        // If rewind was triggered by timer expiring, reload scene to reset timer and clocks
        if (triggeredByTimer)
        {
            Debug.Log("Rewind complete! Reloading scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("Rewind complete! Resuming timer...");
            // Hazard-triggered rewind: resume timer so player can continue
            if (timerScript != null)
            {
                typeof(Timer)
                    .GetField("stopTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(timerScript, false);
            }
        }
    }
}