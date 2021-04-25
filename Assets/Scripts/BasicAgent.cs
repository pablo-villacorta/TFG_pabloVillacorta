﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BasicAgent : Agent
{
    public static int finishedEpisodes = 0;
    public static int targetsReached = 0;
    public Transform targetTransform;

    private static int maximumToolStamina = 200;
    private static int recoverySteps = 100; // steps needed to unfreeze after being frozen
    private static float normalAgentRunSpeed = 5f;
    private static float frozenAgentRunSpeed = 5f;

    public AgentManager AgentManager;
    public BasicAgent OtherAgent;

    private Rigidbody rBody;
    private float agentRunSpeed = 5f;

    private int currentToolStamina = 0;
    private int recoveryStatus;

    private bool isFrozen;
    public bool hasExitedCorridor;

    public bool hasFlag;

    public Material normalMaterial;
    public Material frozenMaterial;
    public Material flagMaterial;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        this.MaxStep = 5000;
    }

    public override void OnEpisodeBegin()
    {
        if (!AgentManager.flagAvailable)
        {
            AgentManager.SpawnObstacles();
            AgentManager.SpawnFlag();
        }
        hasExitedCorridor = false;

        hasFlag = false;
        //hasFlag = true; // quitar esto
        //AgentManager.RemoveFlag(); // quitar esto

        agentRunSpeed = normalAgentRunSpeed;
        currentToolStamina = 0;
        recoveryStatus = recoverySteps;
        Unfreeze();
        this.GetComponent<MeshRenderer>().material = normalMaterial;
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-9, 9), 0.5f, -13.5f);
        transform.localRotation = Quaternion.identity;
        if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) == 0)
        {
            this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod = 10;
        } else
        {
            this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod = 5; // 5 es el bueno para entrenar
        }
        finishedEpisodes++;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);

        // Agent rotation
        sensor.AddObservation(transform.localRotation.y);

        //// Tool stamina (normalizado)
        //sensor.AddObservation(currentToolStamina / maximumToolStamina);

        //// Is frozen
        //sensor.AddObservation(isFrozen);

        // Flag
        sensor.AddObservation(hasFlag);
        sensor.AddObservation(AgentManager.flagAvailable);

        // Freeze state
        sensor.AddObservation(false);
        sensor.AddObservation(false);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        if (OtherAgent.isActiveAndEnabled)
        {
            // x10 más de existential penalty
            AddReward(-0.001f);
            return;
        }
        float multiplier = 1f;
        //if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) > 0) multiplier = 1f;
        AddReward(-0.01f * multiplier);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        //if (currentToolStamina < maximumToolStamina) currentToolStamina++;
        //if (recoveryStatus < recoverySteps) recoveryStatus++;
        //if (recoveryStatus >= recoverySteps && isFrozen)
        //{
        //    Unfreeze();
        //}

        //if (isFrozen) return;

        //if (currentToolStamina < maximumToolStamina)
        //{
        //    agentRunSpeed = frozenAgentRunSpeed;
        //} else
        //{
        //    agentRunSpeed = normalAgentRunSpeed;
        //}

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
            //case 5:
            //    if (currentToolStamina >= maximumToolStamina && !isFrozen)
            //    {
            //        if (!OtherAgent.isActiveAndEnabled) break;
            //        if (transform.localPosition.z < OtherAgent.transform.localPosition.z)
            //        {
            //            // usar herramienta
            //            currentToolStamina = 0;
            //            UseTool();
            //        }
            //    }
            //    break;
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
        //} else if (Input.GetKey(KeyCode.Space))
        //{
        //    discreteActionsOut[0] = 5;
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("wall"))
        {
            if (OtherAgent.isActiveAndEnabled)
            {
                SetReward(-1f);
                OtherAgent.AddReward(1f);
            } 
            else
            {
                SetReward(-30f);
            }
            
            AgentManager.EndEpisodes();
            //Debug.Log("Wall crash");
            //ResetPosition();
        }
        else if (other.CompareTag("obstacle"))
        {
            if (OtherAgent.isActiveAndEnabled)
            {
                SetReward(-1f);
                OtherAgent.AddReward(1f);
            }
            else
            {
                SetReward(-30f);
            }

            AgentManager.EndEpisodes();
            //Debug.Log("Obstacle crash");
            //ResetPosition();
        }
        else if (other.CompareTag("flag"))
        {
            hasFlag = true;
            this.GetComponent<MeshRenderer>().material = flagMaterial;

            if (!OtherAgent.isActiveAndEnabled)
            {
                // está solo
                SetReward(20f);
                AgentManager.RemoveFlag();
                return;
            }

            SetReward(0.1f);
            AgentManager.RemoveFlag();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("target"))
        {
            if (hasFlag)
            {
                if (OtherAgent.isActiveAndEnabled)
                {
                    SetReward(1);
                    OtherAgent.SetReward(-1);
                }
                else
                {
                    // está solo
                    SetReward(100f);
                }
                
                AgentManager.EndEpisodes();
                return;
            }
        }
        else if (collision.collider.CompareTag("corridorWall"))
        {
            if (OtherAgent.isActiveAndEnabled) return;
            SetReward(-30f);
            EndEpisode();
            AgentManager.SpawnFlag();
        } else if (collision.collider.CompareTag("agent"))
        {
            if (!OtherAgent.isActiveAndEnabled) return;

            if (!hasFlag && OtherAgent.hasFlag)
            {
                // empate, la única manera de ganar es con el pañuelito
                SetReward(0f);
                OtherAgent.SetReward(0f);
                AgentManager.EndEpisodes();
            }
        }
    }

    private void UseTool()
    {
        OtherAgent.Freeze();
    }

    private void ResetPosition()
    {
        transform.localPosition = new Vector3(Random.Range(-9, 9), 0.5f, -13.5f);
        transform.localRotation = Quaternion.identity;
    }

    public void Freeze()
    {
        if (this.isFrozen) return;
        if (recoveryStatus < recoverySteps) return;
        
        recoveryStatus = 0;
        this.isFrozen = true;
        this.GetComponent<MeshRenderer>().material = frozenMaterial;
    }

    public void Unfreeze()
    {
        if (!this.isFrozen) return;

        recoveryStatus = recoverySteps;
        this.isFrozen = false;
        this.GetComponent<MeshRenderer>().material = normalMaterial;
    }
}
