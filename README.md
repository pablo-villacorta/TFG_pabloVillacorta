# Self Play - Corridor (no se puede morir dentro) con freeze y pen. existencial

RUN-ID: SelfPlay_FreezeTool_Corridor_18

He creado este modelo a partir del SelfPlay_FreezeTool_Corridor_16, y la única diferencia es que este tiene penalización existencial, que es -0.00001f:

```c#
public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        if (OtherAgent.isActiveAndEnabled)
        {
            AddReward(-0.00001f);
        }
        float multiplier = 1f;
        //if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) > 0) multiplier = 1f;
        AddReward(-0.01f * multiplier);
    }
```

A ver si conseguimos que, a diferencia de su primo, el SelfPlay_FreezeTool_Corridor_17, este decida que es mejor salir antes que quedarse dentro todo el rato. Si vemos que sigue ocurriendo, probar a multiplicar x10 la penalización existencial (que sea -0.0001f).

