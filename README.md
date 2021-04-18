# Self Play - Probando con herramientas

## Herramienta: congelar al enemigo

Idea: que el agente, en cualquier momento, pueda congelar a su enemigo durante x tiempo. La herramienta tardará un tiempo en reponerse (tendrá un contador), y al usar la herramienta, el agente verá su velocidad reducida durante un tiempo también (recupera estamina perdida al utilizar la herramienta).

```c#
using System.Collections;
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
    private static float frozenAgentRunSpeed = 2f;

    public AgentManager AgentManager;
    public BasicAgent OtherAgent;

    private Rigidbody rBody;
    private float agentRunSpeed = 5f;

    private int currentToolStamina = 0;
    private int recoveryStatus;

    private bool isFrozen;

    public Material normalMaterial;
    public Material frozenMaterial;

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
        agentRunSpeed = normalAgentRunSpeed;
        currentToolStamina = maximumToolStamina;
        recoveryStatus = recoverySteps;
        Unfreeze();
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-9, 9), 0.5f, -11);
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

        // Tool stamina (normalizado)
        sensor.AddObservation(currentToolStamina / maximumToolStamina);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        float multiplier = 1f;
        //if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) > 0) multiplier = 1f;
        AddReward(-0.01f * multiplier);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        if (currentToolStamina < maximumToolStamina) currentToolStamina++;
        if (recoveryStatus < recoverySteps) recoveryStatus++;
        if (recoveryStatus >= recoverySteps && isFrozen)
        {
            Unfreeze();
        }

        if (isFrozen) return;

        if (currentToolStamina < maximumToolStamina)
        {
            agentRunSpeed = frozenAgentRunSpeed;
        } else
        {
            agentRunSpeed = normalAgentRunSpeed;
        }

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
            case 5:
                if (currentToolStamina >= maximumToolStamina && !isFrozen)
                {
                    // usar herramienta
                    currentToolStamina = 0;
                    UseTool();
                }
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
        } else if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[0] = 5;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("target"))
        {
            SetReward(100f);
            OtherAgent.SetReward(-50f);
            //targetsReached++;
            //if (targetsReached % 100 == 0)
            //{
            //    Debug.Log("Success rate: " + (targetsReached * 1.0f / finishedEpisodes));
            //}
            AgentManager.EndEpisodes();
        }
        else if (other.CompareTag("wall"))
        {
            SetReward(-30f);
            OtherAgent.AddReward(5f);
            //Debug.Log("Wall crash");
            ResetPosition();
        }
        else if (other.CompareTag("obstacle"))
        {
            SetReward(-30f);
            OtherAgent.AddReward(5f);
            //Debug.Log("Obstacle crash");
            ResetPosition();
        }
    }

    private void UseTool()
    {
        OtherAgent.Freeze();
    }

    private void ResetPosition()
    {
        transform.localPosition = new Vector3(Random.Range(-9, 9), 0.5f, -11);
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

```

Ahora mismo, cada agente está congelado 100 steps, y la herramienta se resetea en 200 steps. Mientras la herramienta se carga, el agente en cuestión va a una velocidad menor (5 de normal, 2 en este caso). Esto genera que, cuando el agente congelado se recupera, hay un periodo de 100 steps en los que el agente víctima tiene velocidad normal, y el otro va lento (lo normal es que el primer agente congele al otro nada más descongelarse, igual podría cambiarse esto para que no pueda usar la herramienta nada más descongelarse).

El problema de esta herramienta es que su uso es tan "barato" que al final la utilizan todo el rato.

RUN-ID: SelfPlay_FreezeTool_11

Creo que esta tool es una herramienta, no da pie a ningun tipo de estrategia divertida. Creo que es por el hecho de que los agentes pueden utilizarla en cualquier momento. Sería más interesante tener algo tipo disparo para freezear a los rivales, o que solo pueda freezear a su rival cuando este se encuentre dentro de un radio (que el agente se tenga que acercar al agente rival). Esta herramienta (freezeo sin más) es una mierda, no da para nada.

Creo que la del disparo es la que más factible parece. Podría hacer que el agente disparase bolas relativamente grandes, para facilitar el disparo. Y que los rayos de los agentes que detectan los rivales tengan esferas más grandes, para poder detectar más facilmente a sus rivales.

Otra idea (que es la siguiente que voy a probar): que la herramienta consista en lanzar un "halo" de fuerza en todas las direcciones, de forma que podamos impulsar el agente rival. Podríamos hacer que cuanto más cerca esté mayor sea el impulso (de esta forma evitamos un uso desmesurado de esta herramienta).



