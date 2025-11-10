using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Timer;

public class Collectible : MonoBehaviour
{
    public float timeValue = 10;
    private Timer TimerScript;
    public float bobSpeed = 1f;
    public float bobHeight = 0.5f;
    private Vector3 startPosition;

    void Start()
    {
        TimerScript = GameObject.FindGameObjectWithTag("Timer").GetComponent<Timer>();
        startPosition = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.position = new Vector3(startPosition.x, newY, startPosition.z);  
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Assuming there's a GameManager script that handles the time
            Destroy(gameObject);
            TimerScript.AddTime(timeValue);
        }
    }
}
