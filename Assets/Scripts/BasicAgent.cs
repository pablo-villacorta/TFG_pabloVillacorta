using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BasicAgent : Agent
{

    public Transform targetTransform;

    public List<GameObject> obstacles;

    private Rigidbody rBody;
    private float agentRunSpeed = 5f;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        this.MaxStep = 5000;

        // apartar todos los obstaculos salvo el primero
        for (int i = 0; i < obstacles.Count; i++)
        {
            var old = obstacles[i].transform.position;
            obstacles[i].transform.position = new Vector3(100000, old.y, old.z);
        }
    }

    public override void OnEpisodeBegin()
    {
        SpawnObstacles();
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-9, 9), 0.5f, -11);
        transform.localRotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);

        // Agent rotation
        sensor.AddObservation(transform.localRotation.y);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        AddReward(-0.01f);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        rBody.AddForce(dirToGo * agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("target"))
        {
            SetReward(100f);
            EndEpisode();
        }
        else if (other.CompareTag("wall"))
        {
            SetReward(-30f);
            EndEpisode();
        }
        else if (other.CompareTag("obstacle"))
        {
            SetReward(-30f);
            EndEpisode();
        }
    }

    private void SpawnObstacles()
    {
        int activeObstacles = (int) Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f);
        
        for (int i = 0; i <  activeObstacles; i++)
        {
            obstacles[i].GetComponent<MovingObstacle>().Spawn();
            //var old = obstacles[i].transform.localPosition;
            //obstacles[i].transform.localPosition = new Vector3(Random.Range(-7, 7), old.y, old.z);
        }
    }

}
