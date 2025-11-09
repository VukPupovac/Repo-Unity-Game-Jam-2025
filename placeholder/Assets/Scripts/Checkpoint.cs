using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player passed through
        if (other.CompareTag("Player"))
        {
            // Get the PlayerStartInfo component from the player
            PlayerStartInfo playerInfo = other.GetComponent<PlayerStartInfo>();
            
            if (playerInfo != null)
            {
                // Update checkpoint to player's current position as they pass through
                playerInfo.UpdateCheckpoint(other.transform.position);
                
                Debug.Log($"Checkpoint activated! New respawn point: {other.transform.position}");
            }
        }
    }
}
