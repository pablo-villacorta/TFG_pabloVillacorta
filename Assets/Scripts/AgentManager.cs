using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AgentManager : MonoBehaviour
{
    public BasicAgent Agent1, Agent2;
    public List<GameObject> obstacles;

    private Vector3 agent1pos, agent2pos;

    private void Awake()
    {
        // apartar todos los obstaculos salvo el primero
        for (int i = 0; i < obstacles.Count; i++)
        {
            var old = obstacles[i].transform.position;
            obstacles[i].transform.position = new Vector3(100000, old.y, old.z);
        }
    }

    public void EndEpisodes()
    {
        Agent1.EndEpisode();
        Agent2.EndEpisode();
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

        agent1pos = new Vector3(x1, 0.5f, -11f);
        agent2pos = new Vector3(x2, 0.5f, -11f);

        Agent1.transform.localPosition = agent1pos;
        Agent2.transform.localPosition = agent2pos;
    }

    private void SpawnObstacles()
    {
        int activeObstacles = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 3.0f);

        for (int i = 0; i < activeObstacles; i++)
        {
            obstacles[i].GetComponent<StaticObstacle>().Spawn();
        }
    }

}
