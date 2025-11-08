using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attaches to a platform and applies a continuous horizontal speed to the player
/// while they are standing on it, creating a treadmill effect.
/// </summary>
public class TreadmillPlatform : MonoBehaviour
{
    [Tooltip("The speed the player will be pushed at. Use a NEGATIVE value (e.g., -5) to pull the character backwards (left). Use a positive value (e.g., 5) to push them forwards (right).")]
    [SerializeField] private float treadmillSpeed = 5f;

    // Use a constant string for the player tag to avoid typos
    private const string PlayerTag = "Player";

    // OnCollisionStay2D is called once per frame for every collider that is touching this object.
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 1. Check if the colliding object is the Player
        if (collision.gameObject.CompareTag(PlayerTag))
        {
            // 2. Attempt to get the Player's Rigidbody2D component
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                // 3. Keep the player's current vertical speed (Y-axis) for jumping/falling
                float currentYVelocity = playerRb.velocity.y;

                // 4. Set the player's velocity to the new horizontal speed while preserving vertical movement.
                // This forces the movement, overriding character controller friction/stopping logic.
                playerRb.velocity = new Vector2(treadmillSpeed, currentYVelocity);
            }
        }
    }

    // Optional: Add visual feedback in the editor
    private void OnDrawGizmos()
    {
        if (treadmillSpeed != 0)
        {
            Collider2D collider = GetComponent<Collider2D>();
            if (collider == null) return; // Skip if no collider
            
            // Gizmos logic adapted for speed (velocity)
            Gizmos.color = (treadmillSpeed < 0) ? Color.red : Color.green;
            // Draw an arrow indicating the direction and magnitude of the speed
            Vector3 center = transform.position + new Vector3(0, collider.bounds.size.y / 2 + 0.5f, 0);
            Vector3 end = center + new Vector3(treadmillSpeed * 0.5f, 0, 0); // Scale the visual based on speed
            
            // Draw the main line
            Gizmos.DrawLine(center, end);
            
            // Draw the arrowhead (simplified)
            Vector3 perpendicular = Vector3.up * 0.1f;
            Gizmos.DrawLine(end, end - (end - center).normalized * 0.3f + perpendicular);
            Gizmos.DrawLine(end, end - (end - center).normalized * 0.3f - perpendicular);
        }
    }
}