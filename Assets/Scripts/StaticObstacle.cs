using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class StaticObstacle : MonoBehaviour
{
    public static float minX = -7f;
    public static float maxX = 7f;

    public float z = 0f;
    public float initialX = minX;

    public void Spawn()
    {
        initialX = Random.Range(minX, maxX);
        transform.localPosition = new Vector3(initialX, 1.5f, z);
    }
}
