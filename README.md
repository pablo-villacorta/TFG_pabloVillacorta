# Self Play - Freeze 1 vez con pasillo

Vale, tenemos el pasillo montado. Ahora mismo he cambiado la estrategia de recompensas, de forma que, cuando el agente está entrenando solo (su rival está desactivado), la función de recompensas es la misma que antes. Ahora, cuando ambos agentes están funcionando, la función de recompensas se simplifica: 

- Se elimina le penalización existencial (se entiende que, al estar compitiendo entre ellos, la idea de llegar al otro lado cuanto antes emerge por sí sola)
- Al chocar con obstáculos o  paredes, los episodios terminan (el agente que se choca pierde, el otro gana)
- El agente que gana se lleva +1 reward, el que pierde -1 reward.

```c#
private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("target"))
        {
            if (OtherAgent.isActiveAndEnabled)
            {
                SetReward(1);
                //OtherAgent.SetReward(-50f); 
                OtherAgent.SetReward(-1); 
            } 
            else
            {
                // está solo
                SetReward(100f);
                EndEpisode();
            }
            //targetsReached++;
            //if (targetsReached % 100 == 0)
            //{
            //    Debug.Log("Success rate: " + (targetsReached * 1.0f / finishedEpisodes));
            //}
            AgentManager.EndEpisodes();
        }
        else if (other.CompareTag("wall"))
        {
            if (OtherAgent.isActiveAndEnabled)
            {
                SetReward(-1f);
                OtherAgent.AddReward(1f);
                AgentManager.EndEpisodes();
            } 
            else
            {
                SetReward(-30f);
                EndEpisode();
            }
            //Debug.Log("Wall crash");
            //ResetPosition();
        }
        else if (other.CompareTag("obstacle"))
        {
            if (OtherAgent.isActiveAndEnabled)
            {
                SetReward(-1f);
                OtherAgent.AddReward(1f);
                AgentManager.EndEpisodes();
            }
            else
            {
                SetReward(-30f);
                EndEpisode();
            }
            //Debug.Log("Obstacle crash");
            //ResetPosition();
        }
    }
```

```c#
public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        if (OtherAgent.isActiveAndEnabled) return;
        float multiplier = 1f;
        //if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) > 0) multiplier = 1f;
        AddReward(-0.01f * multiplier);
    }
```

Vamos a probar a entrenar con esto. Primero, entrenamos unos cuantos episodios solos (sin self play). Cuando tengamos algo decente, paramos, activamos self play y el agente rosa, y seguimos entrenando. A ver qué tal...

RUN-ID: SelfPlay_FreezeTool_Corridor_7

Vale a ver. Lo he dejado entrenando en solitario durante 370k steps. Ronda los 80 puntos. No lo hace perfecto pero lo suficientemente bien como para poder empezar a experimentar (unos 20 minutos ha estado).

Activo el self play en el yaml y el agente rosa en unity.

Regular. Paro el entrenamiento a los 655k pasos. Modifico el fichero de configuración:

```yaml
self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: 20000
      swap_steps: 2000
```

Cambio save steps a 20k para que juguemos contra oponentes más diferentes (generaliza mejor, aunque puede que tarde algo más).

Quito también lo de team_change, para que sea el valor por defecto (5*save_steps = 100k), para poder ver mejor si el agente progresa.

Vale, por fin tenemos una estrategia más o menos clara!! Tras 2M steps de entrenamiento, vemos cómo los agentes tratan constantemente de empujar al rival hacia las paredes el pasillo o los límites del área de entrenamiento. La estrategia es más o menos clara: congelan al rival para intentar empujarlo mientras sigue inmóvil. El ELO sube de forma más o menos constante (aunque muy lentamente), a los 2.420M steps llevamos 1512 de ELO (empezando en 1200). Lo único malo de esto es que los agentes nunca salen del pasillo, se quedan dentro porque en verdad no necesitan llegar al target para ganar: basta con empujar al rival.

### Qué hacer ahora

Se me ocurren varias ideas:

1. Basándome en este mismo escenario, rehacer todo con los siguientes cambios:
   - Las paredes del corridor no son "letales" (chocar con ellas no provoca nada)
   - Los límites de pista antes del corridor tampoco son letales 
     - Todo esto lo hago para forzar a los agentes a salir del corridor
2. Una vez implementado 1 y habiendo probado pequeñas variaciones, probar esta otra idea (en una branch nueva): el pañuelito. La idea es que ambos agentes empiezan abajo, y ponemos un pañuelito en un punto concreto del escenario. El objetivo de los agentes es coger el primero el pañuelito y llegar al target con él. Si el otro agente ha cogido antes el pañuelito, el objetivo del agente será "robárselo", intentando chocar con él. Si chocan, el agente que no había cogido el pañuelito gana. Este escenario puede ser interesante porque los comportamientos tienen varias "fases" y variaciones (primero, llegar el primero al pañuelito; luego, dependiendo de si lo ha cogido o no, hacer una cosa u otra). Este escenario puede ser muy interesante.