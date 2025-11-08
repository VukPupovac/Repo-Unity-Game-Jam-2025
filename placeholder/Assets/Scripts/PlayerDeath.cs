using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("UI References")]
    public GameObject deathScreenUI; // Assign in Inspector if needed

    [Header("Rewind Settings")]
    public float rewindSpeed = 0.5f; // 0.5 = half speed, 2 = double speed

    [Header("Player References")]
    public GameObject playerGameObject;
    public PlayerStartInfo playerInfoScript;

    private TimeControlled[] timeObjects;
    private bool isRewinding = false;
    private float rewindTimer = 0f;

    private void Awake()
    {
        timeObjects = FindObjectsOfType<TimeControlled>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazard"))
        {
            Die();
        }
    }

    private void Die()
    {
        // Optional: show death UI
        if (deathScreenUI != null)
            deathScreenUI.SetActive(true);

        // Get player starting position
        playerInfoScript = playerGameObject.GetComponent<PlayerStartInfo>();
        Vector3 startCoords = playerInfoScript.originalPosition;

        // Begin rewind
        StartCoroutine(RewindToStart(startCoords));
    }

    private System.Collections.IEnumerator RewindToStart(Vector3 startCoords)
    {
        isRewinding = true;

        // Tell all time-controlled objects to start rewinding
        foreach (TimeControlled timeObject in timeObjects)
        {
            timeObject.StartRewind();
        }

        // Keep rewinding until the player is close to start position
        while (Vector3.Distance(playerGameObject.transform.position, startCoords) > 0.05f)
        {
            rewindTimer += Time.deltaTime * rewindSpeed;

            // Rewind in fixed steps
            while (rewindTimer >= Time.fixedDeltaTime)
            {
                foreach (TimeControlled timeObject in timeObjects)
                {
                    timeObject.TimedUpdate();
                }
                rewindTimer -= Time.fixedDeltaTime;
            }

            yield return null; // Wait until next frame to prevent freezing
        }

        // Stop rewinding
        foreach (TimeControlled timeObject in timeObjects)
        {
            timeObject.StopRewind();
        }

        isRewinding = false;
        rewindTimer = 0f;

        // Optional: Hide death screen and reset player state
        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        // Ensure player is exactly at the start position
        playerGameObject.transform.position = startCoords;
    }
}