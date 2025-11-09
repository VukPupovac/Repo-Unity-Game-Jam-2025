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
    }

    private void Update()
    {
        // Timer-based rewind - check if timer just ran out
        if (!isRewinding && timerScript != null && timerScript.gameTime <= 0.01f)
        {
            triggeredByTimer = true;
            StartRewind();
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

        playerInfoScript = playerGameObject.GetComponent<PlayerStartInfo>();
        
        // Choose which position to rewind to based on why we're rewinding
        Vector3 startCoords;
        if (triggeredByTimer)
        {
            // Timer ran out - go back to ORIGINAL spawn point (scene will reload after)
            startCoords = playerInfoScript.originalPosition;
        }
        else
        {
            // Hazard death - go back to current checkpoint
            startCoords = playerInfoScript.currentCheckpoint;
        }

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

        // Perform rewind until player reaches starting point
        float distanceToTarget = Vector3.Distance(playerGameObject.transform.position, startCoords);
        Debug.Log($"Starting rewind. Distance to target: {distanceToTarget}, triggeredByTimer: {triggeredByTimer}");
        
        float rewindElapsedTime = 0f;
        
        while (Vector3.Distance(playerGameObject.transform.position, startCoords) > 0.05f)
        {
            rewindTimer += Time.deltaTime * dynamicRewindSpeed;
            rewindElapsedTime += Time.deltaTime;

            // Safety: If rewind takes longer than maxRewindTime, something is wrong - force scene reload
            if (rewindElapsedTime >= 6f)
            {
                Debug.LogWarning($"Rewind exceeded max time ({maxRewindTime}s). Game may be in failed state. Force reloading scene.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                yield break; // Exit coroutine immediately
            }

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

        Debug.Log($"Rewind complete. triggeredByTimer = {triggeredByTimer}, final distance = {Vector3.Distance(playerGameObject.transform.position, startCoords)}");

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
            Debug.Log("Timer triggered - reloading scene NOW");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("Hazard triggered - resuming timer");
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