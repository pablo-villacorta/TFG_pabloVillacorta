using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AgentManager : MonoBehaviour
{
    public BasicAgent Agent1, Agent2;
    public List<GameObject> obstacles;
    public GameObject CorridorWalls;

    private Vector3 agent1pos, agent2pos;

    private void Awake()
    {
        Vector3 old = Vector3.zero;

        // apartar todos los obstaculos salvo el primero
        for (int i = 0; i < obstacles.Count; i++)
        {
            old = obstacles[i].transform.position;
            obstacles[i].transform.position = new Vector3(100000, old.y, old.z);
        }

        old = CorridorWalls.transform.position;
        CorridorWalls.transform.position = new Vector3(100000, old.y, old.z);
    }

    public void EndEpisodes()
    {
        Agent1.EndEpisode();
        if (Agent2.isActiveAndEnabled)
        {
            Agent2.EndEpisode();
        } 
        ResetAgentPositions();
        SpawnObstacles();
    }

    private void ResetAgentPositions()
    {
        float x1, x2;
        x1 = Random.Range(-9, 9);

        do
        {
            x2 = Random.Range(-9, 9);
        } while (Mathf.Abs(x1 - x2) < 1.5f);

        agent1pos = new Vector3(x1, 0.5f, -13.5f);
        agent2pos = new Vector3(x2, 0.5f, -13.5f);

        Agent1.transform.localPosition = agent1pos;
        Agent2.transform.localPosition = agent2pos;
    }

    private void SpawnObstacles()
    {
        int activeObstacles = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 1.0f);

        for (int i = 0; i < activeObstacles; i++)
        {
            obstacles[i].GetComponent<StaticObstacle>().Spawn();
        }

        if (activeObstacles > 0)
        {
            CorridorWalls.transform.localPosition = new Vector3(0.0546920598f, 1.22981977f, -10.5799999f);
        }
    }

}
