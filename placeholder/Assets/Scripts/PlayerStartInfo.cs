using UnityEngine;

public class PlayerStartInfo : MonoBehaviour
{
    // Original spawn position - NEVER changes after scene loads
    public Vector3 originalPosition { get; private set; }
    
    // Current checkpoint position - updates when player passes checkpoints
    public Vector3 currentCheckpoint { get; private set; }

    void Start()
    {
        // Store the initial spawn position - this is where timer expiry sends you
        originalPosition = transform.position;
        
        // Initially, checkpoint is the same as original spawn
        currentCheckpoint = originalPosition;
    }
    
    // Called by Checkpoint script when player passes through
    public void UpdateCheckpoint(Vector3 newCheckpointPosition)
    {
        currentCheckpoint = newCheckpointPosition;
    }
}
