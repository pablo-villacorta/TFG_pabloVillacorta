# Self Play - Pañuelito v2

## Entrenando por partes

### Parte 1: solo agente, sin pañuelito [hecho en el commit anterior]

Que el agente aprenda a llegar de un lado al otro (igual que el escenario básico con obstáculos estáticos).

RUN-ID: SelfPlay_Flag_NoTool_Single_4 (entrenado durante 300k)

### Parte 2: solo agente, con pañuelito [hecho en el commit anterior]

Ahora el agente tiene que reentrenarse para pasar primero por el pañuelito, y luego ya hacer como hacía antes. 

RUN-ID: SelfPlay_Flag_NoTool_SingleWFlag_5 (entrenado durante 1M steps)

Ahora tenemos un agente que recoge el pañuelito y lo lleva al otro lado de forma muy optimizada. Este modelo va a poder utilizarse como base de todas las pruebas que hagamos con el juego del pañuelito ya en versión competitiva (lo podemos reutilizar todas las veces que necesitemos, en modo single player lo hace prácticamente perfecto - 112 de media sobre un máximo de 120).

### Parte 3: entrenar competitivo

#### Parte 3a: entrenar directamente competitivo

Vamos a probar esto. Lo pruebo antes que el 3b porque pienso que en el 3b es posible que ningun agente acabe queriendo coger el pañuelito, al proporcionarle una desventaja significativa al ir más lento que su rival.

```c#
public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        if (OtherAgent.isActiveAndEnabled)
        {
            AddReward(-0.00001f);
            return;
        }
        float multiplier = 1f;
        AddReward(-0.01f * multiplier);
    }
```

He metido una pequeña penalización existencial (-0.00001f) para intentar que los agentes no pierdan demasiado el tiempo (en el readme anterior llegué a la conclusión de que quizás fuera necesario, tras los 3M steps de entrenamiento).

Activo los agentes rosas, el self play, y quito el CL (empiezan entrenando directamente con 3 obstáculos). A ver qué tal...

RUN-ID: SelfPlay_Flag_NoTool_Multi_3 (entrenado 3M steps)

Vale, tenemos algo que más o menos ha entendido cómo funciona el juego. El problema? Creo que reside en la función de recompensas. Tras 1.5M steps de entrenamiento, los agentes tienen "miedo" a coger el pañuelito: han llegado a la conclusión de que es más rentable no cogerlo y después pillar al que lo ha cogido. Se quedan constantemente esperando el uno al otro ("calma tensa"). Podría hacer que la máxima recompensa solo pueda ser conseguida al coger el pañuelito y llegar al otro lado, y que, en caso de que te pillen, el castigo sea menor (y la recompensa del que ha "cazado" al otro también es menor que la recompensa cuando se coge el pañuelito y se llega al target). De esta forma, quizás podría alentar a los agentes coger el pañuelito (también podría aumentar la recompensa por cogerlo). Otra opción sería aumentar la penalización existencial. 

Pero este modelo es bastante prometedor. El mero hecho de quedarse esperando en vez de coger el pañuelito hace ver que los agentes han "llegado a la conclusión" de que es mejor dejar que el otro coja el pañuelo. También es destacable el hecho de que han aprendido a empujarse el uno al otro para ganar sin tener que hacer uso del pañuelito (empujar al otro hacia un obstáculo o hacia una pared).

**Prueba**

Paro el entrenamiento de SelfPlay_Flag_NoTool_Multi_3, creo un nuevo modelo basado en este y aumento la penalización existencial. Pongo algo significativamente más heavy. A ver si conseguimos que esa "calma tensa" desaparezca metiendo prisa a los agentes... Igual podemos forzarles a que se empujen el uno al otro. Y también podemos aumentar la recompensa por coger el pañuelito... De esta forma siempre tendrá cierta ventaja (en términos de recompensa) aquel agente que coja el pañuelo. Vamos a probar...

Cambios: reward por coger el pañuelito pasa de ser 0.01 a 0.5

```c#
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

            SetReward(0.5f); /// aquí!!!
            AgentManager.RemoveFlag();
        }
    }
```

Penalización existencial pasa de ser -0.00001f a -0.001f

```c#
public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        if (OtherAgent.isActiveAndEnabled)
        {
            AddReward(-0.001f);
            return;
        }
        float multiplier = 1f;
        //if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) > 0) multiplier = 1f;
        AddReward(-0.01f * multiplier);
    }
```

La penalización existencial es significativamente mayor, por lo que es posible que opten por suicidarse. En este caso, habrá que disminuirla. Vamos a ver qué tal va con esta configuración...

RUN-ID: SelfPlay_Flag_NoTool_Multi_4

TO-DO: si vemos que la penalización existencial es demasiado alta, bajarla

Update*: quizás está un poco alta, pero casi mejor, hace el combate más interesante. Qué ocurre? Ahora, a no ser que un agente tenga claro que puede coger el pañuelito e irse sin peligro de que le pillen, irá a chocarse con su rival, y lucharán por empujar al otro contra un obstáculo o una pared fuera de límites. La estrategia me parece buena, evitamos la "calma" tensa que teníamos antes, y los agentes tienen formas de ganar. 

Update**: la estrategia que he descrito en el anterior update ha parece que se ha disminuido. Los agentes siguen peleándose de vez en cuando, pero al haber aumentado la recompensa por coger el pañuelo, tienen más interés por cogerlo, y la calma tensa vuelve (aunque de forma menos acusada parece).

Update*** (tras 1.85M steps): confirmamos que la calma tensa no ha vuelto. Pero ahora los agentes tienen una estrategia más clara: si ven muy viable el lanzarse a por el pañuelito y llegar al target, lo hacen. Sin embargo, si no lo ven del todo claro, pelean entre sí para echarse el uno al otro (o, incluso, hay veces en las que tratan de empujar a su rival hacia el target, de forma que puedan fácilmente pillarlo y ganar -  esta estrategia es muy interesante). A veces hay calma tensa, pero siempres hay alguno que se decante por ir a por el pañuelo finalmente (que suele acabar perdiendo). Cabe destacar también que, por primera vez en el escenario del pañuelito, el ELO creción significativamente (llegamos a los 1350 de ELO, aunque, al parar el entrenamiento en este punto, se resetea a 1200 cuando hagamos --resume).

Update**** (tras 3.75M steps): los agentes han perdido la estrategia. Totalmente. Ya no se pelean entre ellos. La calma tensa ha vuelto. Y no sé por qué les ha dado por irese hasta el target (a los dos) sin haber conseguido el pañuelito. Se quedan ahí arriba quietos hasta que expire el tiempo del episodio... No sé qué estoy pasando por alto...

Update 5: (tras 5.5M steps): el agente ha empeorado (comparándolo con la versión tras 4.4M steps de entrenamiento). Se choca demasiado con las paredes y obstáculos. Y no sabe perseguir.

No saben perseguirse. Da la casualidad de que durante el entrenamiento casi siempre coincide que el trayecto que hace el agente verde hacia el target es el mismo que el que tiene que realizar el otro, pero, si juego yo manualmente, se ve claramente que el agente "perseguidor" lo único que hace es seguir su camino hacia el target, y no hacia el agente que tiene que pillar.

Update 6 (tras 6.6M steps): han recuperado un poquito de estrategia, pero la calma tensa ha vuelto (y de qué manera). A no ser que puedan recoger el obstáculo mientras miran hacia arriba (para no perder tiempo girando), son bastante reacios a cogerlo (viendo o no viendo al rival, no pueden saber dónde está).



#### Parte 3b: entrenar competitivo pero el agente con pañuelito va más lento

Descartamos este escenario, hemos solucionado el problema de "deducir" que lo que hay que hacer si no coges el pañuelito es ir a por el otro (gracias a que hemos metido más raycasting con esferas de mayor tamaño, por lo que es más facil encontrar al rival y seguirlo). En el modelo del apartado 3a se ve claramente que lo deducen.



# Probando cositas

Voy a probar a entrenar algo de 0. Le voy a meter LSTM y curiosity. La fase inicial del entrenamiento (agente solo con pañuelito - nos saltamos la fase de agente solo sin pañuelito, ya que no la veo muy necesaria). Además he metido 2 observaciones manuales más, por si acaso más adelante quiero meter lo de que se congelen cuando se choquen. De momento esos dos valores son dos "false" sin más. Valores del yaml:

```yaml
behaviors:
  BasicAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 10
      buffer_size: 100
      learning_rate: 3.0e-4
      beta: 5.0e-4
      epsilon: 0.2
      lambd: 0.99
      num_epoch: 3
      learning_rate_schedule: linear
    network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: true
      sequence_length: 64
      memory_size: 256
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
      curiosity:
        strength: 0.0000001 # 0.02
        gamma: 0.99
      # gail: 
      #   strength: 0.1
      #   gamma: 0.99
      #   demo_path: Assets/Demonstrations/Corridor1.demo
      #   use_actions: false
      #   use_vail: false
    # self_play:
    #   window: 10
    #   play_against_latest_model_ratio: 0.5
    #   save_steps: 20000
    #   swap_steps: 2000
    max_steps: 10000000
    time_horizon: 64
    summary_freq: 5000

environment_parameters:
  active_obstacles: 
    curriculum:
      - name: NoObstacles # The '-' is important as this is a list
        completion_criteria:
          measure: reward
          behavior: BasicAgent
          signal_smoothing: false
          min_lesson_length: 200
          threshold: 110 #95
        value: 0.0
      - name: OneObstacle # The '-' is important as this is a list
        completion_criteria:
          measure: reward
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 200
          threshold: 105 #93
        value: 1.0
      - name: TwoObstacles # This is the start of the second lesson
        completion_criteria:
          measure: reward
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 200
          threshold: 100 #90 # 100k pasos
          require_reset: true
        value: 2.0
      - name: ThreeObstacles
        value: 3.0
  max_obstacle_speed: 0.1
    # curriculum:
    #   - name: OnlySlowObstacles
    #     completion_criteria:
    #       measure: progress
    #       behavior: BasicAgent
    #       signal_smoothing: true
    #       min_lesson_length: 200
    #       threshold: 0.01
    #     value: 0.01
    #   - name: SlowMediumObstacles
    #     completion_criteria:
    #       measure: progress
    #       behavior: BasicAgent
    #       signal_smoothing: true
    #       min_lesson_length: 200
    #       threshold: 0.1
    #     value: 0.05
    #   - name: AllObstacles
    #     value: 0.1
```

Para la fase inicial, pongo curiosity muy bajo (prácticamente 0, porque no es muy necesario para el agente en solitario - luego en entorno competitivo le pongo un valor mayor).

RUN-ID: SelfPlay_Flag_NoTool_SingleWFlag_8

Juega solo muy bien. A partir de este modelo he entrenado los siguientes:

Cada uno de los siguientes modelos tiene diferentes características (alguno es más agresivo - solo busca confrontarse, ni se preocupa por el pañuelo-, otros son demasiado cautos - casi nunca cogen el pañuelo, tienen miedo de que les pille el otro agente).

### SelfPlay_Flag_NoTool_Multi_7

entrenado a partir de SelfPlay_Flag_NoTool_SingleWFlag_8

El agente que pilla gana

### SelfPlay_Flag_NoTool_Multi_MoreCuriosity_8

Este se ha entrenado a partir del SelfPlay_Flag_NoTool_Multi_7 tras 6.6M steps de entrenamiento. La única diferencia es que este tiene más curiosidad (0.1 de strength). No ha habido beneficios significativos.

### SelfPlay_Flag_NoTool_Multi_8

El agente que pilla empata (solo se puede ganar la recompensa máxima llegando al target con el pañuelito, si no, únicamente puede perder o empatar).







## Posibles ideas si vemos que falla

- Yo de momento creo que las dos primeras fases del entrenamiento (single agent sin y con pañuelito) están bastante bien tal y como están entrenados SelfPlay_Flag_NoTool_Single_4 y SelfPlay_Flag_NoTool_SingleWFlag_5. De cambiar algo, supongo que sería el entrenamiento multi agente.
- Dentro del multiagente:
  - Probar a hacer que el agente que haya cogido el pañuelito vea su velocidad reducida (un 10 o 20%, por ejemplo, de forma que el otro agente tenga más posibilidades de aprender que tiene que pillarlo - si no, es muy dificil que le pille las suficientes veces como para aprender que es eso lo que tiene que hacer).
  - Entrenar multi agente con menos obstáculos (empezar con 1 o 2, entrenar el modelo y luego entrenar uno nuevo con 3 obstáculos). 
  - Igual meter un poco de lstm (habría que reentrenar todo de 0). De esta forma, los agentes pueden recordar dónde estaba el rival antes de que cojan el pañuelito (si no, muchas veces no saben si cogerlo o no porque no ven al rival, que puede estar al otro lado del pañuelito). También puede servir para predecir el movimiento del rival para "pillarlo".

