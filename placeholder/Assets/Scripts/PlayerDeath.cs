using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Rewind Settings")]
    public float rewindSpeed = 0.5f; // How fast the rewind plays back

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
        // Timer-based rewind
        if (!isRewinding && timerScript != null && timerScript.gameTime <= 0f)
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
        Vector3 startCoords = playerInfoScript.originalPosition;

        StartCoroutine(RewindToStart(startCoords));
    }

    private IEnumerator RewindToStart(Vector3 startCoords)
    {
        isRewinding = true;

        // Stop the timer while rewinding
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

        // Perform rewind until player reaches starting point
        while (Vector3.Distance(playerGameObject.transform.position, startCoords) > 0.05f)
        {
            rewindTimer += Time.deltaTime * rewindSpeed;

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

        // Snap player back to the start position
        playerGameObject.transform.position = startCoords;

        // Reset and restart the timer ONLY if rewind was triggered by timer
        if (triggeredByTimer && timerScript != null)
        {
            timerScript.gameTime = timerScript.timerSlider.maxValue;
            timerScript.timerSlider.value = timerScript.gameTime;

            typeof(Timer)
                .GetField("stopTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(timerScript, false);
        }
        else if (timerScript != null)
        {
            // Resume timer if it was hazard-triggered rewind
            typeof(Timer)
                .GetField("stopTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(timerScript, false);
        }
    }
}