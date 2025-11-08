using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public Animator animator;
    public static float gravity = 100;
    public float rewindSpeed = 0.5f; // 0.5 = half speed, 2 = double speed
    private float rewindTimer = 0f;

    public struct RecordedData

    {
        public Vector2 pos;
        public Vector3 vel;
    }

    readonly RecordedData[][] recordedData;
    TimeControlled[] timeObjects;
    private void Awake()
    {
        timeObjects = GameObject.FindObjectsOfType<TimeControlled>();

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool stepBack = Input.GetKey(KeyCode.R);

        if (stepBack)
        {
            // Tell all objects they're rewinding
            foreach(TimeControlled timeObject in timeObjects)
            {
                timeObject.StartRewind();
            }
            
            rewindTimer += Time.deltaTime * rewindSpeed;
            
            // Only rewind when timer reaches the next frame interval
            while (rewindTimer >= Time.fixedDeltaTime)
            {
                foreach(TimeControlled timeObject in timeObjects)
                {
                    timeObject.TimedUpdate();
                }
                rewindTimer -= Time.fixedDeltaTime;
            }
        }
        else
        {
            // Stop rewinding
            foreach(TimeControlled timeObject in timeObjects)
            {
                timeObject.StopRewind();
            }
            rewindTimer = 0f; // Reset when not rewinding
        }

    }
}
