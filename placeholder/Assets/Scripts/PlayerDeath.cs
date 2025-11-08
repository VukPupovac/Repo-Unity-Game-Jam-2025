using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Needed to reload scenes

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

        // Snap player to start position
        playerGameObject.transform.position = startCoords;

        // If rewind was triggered by timer, reload scene AFTER rewind
        if (triggeredByTimer)
        {
            // Small delay to ensure the final frame of rewind is visible
            yield return null; // You can also increase delay with yield return new WaitForSeconds(0.1f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Hazard-triggered rewind: resume timer
            if (timerScript != null)
            {
                typeof(Timer)
                    .GetField("stopTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(timerScript, false);
            }
        }
    }
}