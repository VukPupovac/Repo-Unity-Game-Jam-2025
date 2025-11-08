using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControlled : MonoBehaviour
{
    public Vector2 velocity;
    
    // Store position and velocity history
    private List<Vector3> positionHistory = new List<Vector3>();
    private List<Vector2> velocityHistory = new List<Vector2>();
    private int maxRecordings = 1000; // Store last 1000 frames
    protected bool isRewinding = false;
    
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        if (!isRewinding)
        {
            // Record current state
            RecordState();
        }
    }

    private void RecordState()
    {
        // Add current position and velocity to history
        positionHistory.Add(transform.position);
        if (rb != null)
        {
            velocityHistory.Add(rb.velocity);
        }
        
        // Keep only the most recent recordings
        if (positionHistory.Count > maxRecordings)
        {
            positionHistory.RemoveAt(0);
            velocityHistory.RemoveAt(0);
        }
    }

    // Called by TimeController when rewinding
    public virtual void TimedUpdate()
    {
        if (positionHistory.Count > 0)
        {
            // Get the last recorded position
            int lastIndex = positionHistory.Count - 1;
            transform.position = positionHistory[lastIndex];
            
            if (rb != null && velocityHistory.Count > 0)
            {
                rb.velocity = velocityHistory[lastIndex];
            }
            
            // Remove the state we just used
            positionHistory.RemoveAt(lastIndex);
            if (velocityHistory.Count > 0)
            {
                velocityHistory.RemoveAt(lastIndex);
            }
        }
    }

    public void StartRewind()
    {
        isRewinding = true;
    }

    public void StopRewind()
    {
        isRewinding = false;
    }
}
//Noah Hemmelgarn
