using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static float gravity = 100;


    public struct RecordedData
    {
        public Vector2 pos;
        public Vector3 vel;
    }

    readonly RecordedData[][] recordedData;
    int recordMax = 100000;
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
        bool stepBack = Input.GetKey(KeyCode.LeftArrow);

        if (stepBack)
        {
            foreach(TimeControlled timeObject in timeObjects)
            {
                timeObject.TimedUpdate();
            }
        }

    }
}
