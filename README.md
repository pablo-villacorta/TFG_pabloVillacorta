# Self Play - Pañuelito v1

Vale, vamos a intentar jugar con el pañuelito. He puesto una capsula verde a modo de pañuelito, se posiciona al principio de cada episodio en el hueco que hay entre los obstáculos (en el código se ven las coordenadas):

```c#
public void SpawnFlag()
    {
        float x = Random.Range(-8.7f, 8.7f);
        float y = 0.8f;
        float z = Random.Range(-3.36f, -1.68f);
        if (Random.value > 0.5f) z += 4.98f;
        flag.transform.localPosition = new Vector3(x, y, z);
        flagAvailable = true;
    }
```

Tengo el siguiente esquema de recompensas:

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
                SetReward(-10f);
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
                SetReward(-10f);
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

            SetReward(0.01f);
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
                SetReward(1f);
                OtherAgent.SetReward(-1f);
                AgentManager.EndEpisodes();
            }
        }
    }
```

Y de momento he hecho que el agente solo tenga 5 acciones posibles (las de moverse, no hay herramienta de momento). Si vemos que es necesaria una herramienta para el agente que no ha cogido el pañuelito (para poder pillar al otro más facilmente, porque quizás sea demasiado complicado sin ninguna ayuda externa).

Le paso al agente las siguientes observaciones:

```c#
public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);

        // Agent rotation
        sensor.AddObservation(transform.localRotation.y);

        // Tool stamina (normalizado)
        sensor.AddObservation(currentToolStamina / maximumToolStamina);

        // Is frozen
        sensor.AddObservation(isFrozen);

        // Flag
        sensor.AddObservation(hasFlag);
        sensor.AddObservation(AgentManager.flagAvailable);
    }
```

Por último, he añadido al componente de raycasting ya existente llamado RayPerceptionSensorOtherAgent una segunda etiqueta a detectar: flag (la que tiene asignada el gameobject verde que hace de pañuelito). Ese componente detecta tanto al otro agente como al pañuelito (no he cambiado ninguna propiedad del mismo, ni numero de rayos ni angulo ni nada).

Entreno inicialmente un modelo sin LSTM para que el agente (en solitario, rosa desactivado) aprenda a buscar el pañuelito y a llevarlo al target (una vez lo ha cogido). Desde la primera lesson de CL estoy incluyendo la lógica del pañuelito, de forma que el llegar al target no hace nada a no ser que se tenga el pañuelito.

RUN-ID: SelfPlay_Flag_NoTool_1

Tras 1.5M steps, tenemos un mean reward de unos 108 puntos (máximo posible 120). El agente en solitario lo hace relativamente bien, no lo hace perfecto, pero teniendo en cuenta que no tiene LSTM está relativamente bien (no puede recordar por dónde ha mirado). Creo que con esto puede ser suficiente para probar si la idea del pañuelito nos sirve.

Además, he descubierto que el comando mlagents-learn tiene una opción --initialize-from que me permite crear un nuevo modelo tomando como base otro modelo ya entrenado (en vez de tener que hacer cambios y después --resume, así queda más elegante y nos permite entrenar aún más el modelo base en solitario si vemos que necesitar refinarse).

Activo self play y el agente rosa (y confío en que el código c# funcione como espero que lo haga).

Vale, he lanzado el siguiente comando: mlagents-learn config/basic_config.yaml --run-id=SelfPlay_Flag_noTool_Multi_2 --initialize-from=SelfPlay_Flag_NoTool_1

Habiendolo dejado entrenar 75k pasos, queda en evidencia que sigue habiendo una importante ventaja posicional. El que llega primero al pañuelito tiene todas las de ganar. Para equilibrarlo, puedo probar a reducir un poquito la velocidad del agente que ha cogido el pañuelo (para que el otro pueda pillarlo). De momento no lo voy a hacer, voy a dejarlo un par de millones de pasos. Si veo que no mejora, implemento un cambio en la velocidad (que la velocidad del agente que tiene el pañuelo se reduzca en un 20% o algo así).

Otra cosa que veo es que hay demasiado pocos rayos para detectar al agente rival. Si está medianamente lejos, es poco probable que lo vea (los rayos se van separando a medida que se alejan del agente origen).

No tiene mala pinta. Creo que ya empeiza a pillar que el que no pilla el pañuelito tiene que ir a por el otro (lo digo habiendo entrenado multi agente durante 900k steps). Lo voy a dejar un buen rato más. Si veo que no mejora del todo, pruebo a meter más rayos que detecten agentes y flags (esto hay que hacerlo fijo, si está medianamente lejos no detecta nada), y añadir más complejidad a la red, ya sea:

- metiendo una capa intermedia adicional
- más hidden units
- LSTM??? (igual complica demasiado las cosas, idk) pero puede que sea improtante, porque pasa que un agente va hacia un flag, pero de repente se le cuela el rival en medio, y deja de ver el flag, y al no tener memoria, se desorienta totalmente..
- Igual puedo meter CL al entrenamiento multiagente. Que varíe la velocidad del agente que ha cogido al pañuelito (comienza lento, para que pueda deducir más facilmente que la tarea del otro agente es pillarle), que vaya de más lenta a más rápida.

Conclusiones tras casi 3M de steps de entrenamiento, vemos pequeñas trazas de estrategia, pero los agentes en realidad están bastante perdidos. Para empezar, la experimentación es bastante pobre (encontrar y llegar al pañuelito les cuesta demasiado. Luego, se siguen pensando que cuando la observación correspondiente a que el pañuelito está disponible es la que les hace saber si tienen que ir al target o no (en verdad debería ser la combinación entre hasFlag y flagAvailable). Por último, hay que meter penalización existencial (aunque sea mínima), porque a veces hacen cosas muy raras, como que un agente que está solo frente al pañuelito no lo coge,  se queda esperando, aún cuando no está cerca el agente rival.

## Entrenando por partes

### Parte 1: solo agente, sin pañuelito

Que el agente aprenda a llegar de un lado al otro (igual que el escenario básico con obstáculos estáticos).

RUN-ID: SelfPlay_Flag_NoTool_Single_4 (entrenado durante 300k)

### Parte 2: solo agente, con pañuelito

Ahora el agente tiene que reentrenarse para pasar primero por el pañuelito, y luego ya hacer como hacía antes. 

RUN-ID: SelfPlay_Flag_NoTool_SingleWFlag_5 (entrenado durante 1M steps)

Ahora tenemos un agente que recoge el pañuelito y lo lleva al otro lado de forma muy optimizada. Este modelo va a poder utilizarse como base de todas las pruebas que hagamos con el juego del pañuelito ya en versión competitiva (lo podemos reutilizar todas las veces que necesitemos, en modo single player lo hace prácticamente perfecto - 112 de media sobre un máximo de 120).

### TO-DO:

- La próxima vez que entrene un modelo basado en este del pañuelito (desde cero), quitar las observaciones innecesarias (isFrozen, currentToolStamina, etc.).
- Cambiar la forma en la que se entrena el agente individual para el pañuelito: comenzar de 0 a 3 obstáculos sin pañuelito. Cuando ya domine el ir de un lado al otro esquivando obstáculos, meter la funcionalidad del pañuelito (igual así se facilita el proceso de búsqueda del pañuelo, haciendo que por defecto recorra de abajo a arriba el escenario, que cuando vea el pañuelo vaya a por él, y luego siga yendo hacia el target - también te digo, cuando metamos el pañuelito empezará a buscarlo directamente, en vez de empezar recorriendo de abajo a arriba, me imagino).



