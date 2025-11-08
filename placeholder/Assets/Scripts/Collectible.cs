using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int timeValue = 30;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Assuming there's a GameManager script that handles the time
            Destroy(gameObject);
            Debug.Log("Collected! Time added: " + timeValue);
        }
    }
}
