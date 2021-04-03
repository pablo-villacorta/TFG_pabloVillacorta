using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MovingObstacle : MonoBehaviour
{
    public static float minX = -7f;
    public static float maxX = 7f;

    public float z = 0f;
    public float initialX = minX;
    private float xStep = 0.1f;

    public bool startsMoving = false;
    private bool moving = false;

    private void Awake()
    {
        moving = startsMoving;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (moving)
        {
            Move();
        }
    }

    public void Spawn()
    {
        float maxStep = Academy.Instance.EnvironmentParameters.GetWithDefault("max_obstacle_speed", 1.0f);
        maxStep = 0.15f;
        xStep = Random.Range(-maxStep, maxStep);
        initialX = xStep > 0 ? minX : maxX;
        transform.localPosition = new Vector3(initialX, 1.5f, z);
        moving = true;
    }

    public void Move()
    {
        float newX = transform.localPosition.x + xStep;

        
        if ((xStep > 0 && newX > maxX) || (xStep < 0 && newX < minX))
        {
            newX = initialX;
        }
        

        transform.localPosition = new Vector3(newX, 1.5f, z);
    }

}
