using UnityEngine;

public class PlayerStartInfo : MonoBehaviour
{
    // Make the initial position public or use a property so other scripts can access it
    public Vector3 originalPosition { get; private set; }

    void Start()
    {
        // Store the current position when the object first wakes up (before Start)
        originalPosition = transform.position;
    }
}
