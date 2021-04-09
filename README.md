# Obstáculos móviles - LSTM ok, necesitamos que aprenda a rodear a los lentos

### TO-DO para esta siguiente versión

- Seguir entrenando el MovingObstaclesLSTM_EP10_3, ya que tiene buena pinta.
- Entrenar con el DecisionRequester a 2
- Hacer que la posición inicial de los obstáculos sea por fuera, para que no se spawneen justo encima de los agentes, y que estos puedan verlos venir
- Probar modelos anteriores con el DecisionRequester a 1 (quizás haya que reentrenarlos)
- Evaluar los modelos más prometedores en igualdad de condiciones (mismos castigos/recompensas)

## 1. Modificamos los obstáculos móviles para que su posición inicial sea fuera de los límites del plano

Ahora, cada episodio los obstáculos comienzan en la posición normal, pero, cuando deban volver a su posición inicial, vuelven a su X inicial * 2, de forma que aparecen más alejadas del plano (el efecto que buscábamos).

## 2. Modificación del DecisionRequester

Aunque mi idea inicial era ponerlo a 1, igual nos basta con ponerlo a 2 (pide acciones cada 2 steps/frames). Mi principal miedo era que, al tenerlo a 1, cogiera tantas imágenes por segundo que no fuera capaz de detectar velocidades. Veremos cómo funciona esto al entrenar.

# Entrenamiento

Vamos a entrenar el modelo anterior pero con estos nuevos cambios mencionados arriba. Lo dejamos un buen rato. Hay que estar atentos y fijarnos en si el agente decide en algun momento suicidarse antes que esperar. 

RUN-ID: MovingObstacles_LSTM_DR2_3

He tenido que hacer un pequeño apaño, ya que, con tantas decisiones por segundo, el agente era incapaz de intuir que tenía que ir hacia adelante en la lesson con cero obstáculos. Durante esa lesson pongo el decisionrequester a 10, y a partir de ahí a 2. Sin embargo, hay otra pega. La penalización existencial se realiza después de cada acción recibida. Al recibir muchas más acciones (por pedir muchas decisiones), la puntuación media de cada episodio es mucho menor. Hay que adaptar las condiciones de paso de CL y rebajarlas.

```yaml
 - name: OneObstacle # The '-' is important as this is a list
        completion_criteria:
          measure: reward
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 450
          threshold: 70.0
        value: 1.0
```

RUN-ID: MovingObstacles_LSTM_DR2_6

Vale esto tiene buena pinta. Aunque las puntuaciones son bajas, creo que está funcionando (el agente tiene mucho más control de lo que ocurre a su alrededor, y siguen emergiendo las estrategias de rodear los obstáculos lentos). Eso sí, quizás debería probar a entrenar a más agentes de forma simultánea, ya que ahora se tarda mucho más en completar un único episodio, por lo que la recolección de datos es significativamente menor que en las versiones previas (lo pruebo en el siguiente run, probando a duplicar el número de agentes).

No sé qué pasa. A partir del paso 370k ha decidido quedarse quieto, no terminar el episodio. Y de repente se ha vuelto tonto. Hemos registrado puntuaciones mínimas históricas (-511!!!!), y llevamos más de 150k así sin arreglarlo. No sé qué le ha llevado a tomar esta decisión, pero estaba logrando una media de unos 58 puntos, que, ajustados a la notablemente mayor penalización existencial, no está tan mal...

Lo voy a parar porque llevo más de 10 minutos mirando esto como un idiota. Voy a probar a cuadriplicar (x4) el número de agentes que entrenan en paralelo, a ver qué pasa.

RUN-ID: MovingObstacles_LSTM_DR2_7

Son ahora mismo 36 agentes entrenando a la vez. Veremos si esto supone un aumento significativo en el ritmo de entrenamiento. Si no lo es, quizás es porque sean demasiados a la vez, y con duplicar podría bastar (18 en total).

Lo primero que observo (con 36 agentes) es que el tiempo (en segundos) por step es mucho más reducido, al tener más agentes en paralelo. Para llegar a los 90k steps, esta versión ha requerido de 4 min 32 s, mientras que la anterior (con 9 agentes) ha requerido de 6 min 54 s.

Un problema que veo es que el agente cuando tiene que esperar se queda demasiado atrás, y le pilla el obstáculo que acaba de pasar. Esto pensaba que lo había solucionado con los rayos que van 90º a cada lado, pero parece que sigue (de todos modos tiene que seguir entrenando este bicho, a ver qué tal).

Nada. Tampoco está tan bien. Cogiendo el modelo ya entrenado y probando a medir su reward medio (quitando el -0.1f de penalización existencial), tenemos una media de unos 75, que no supone un gran logro. Falla bastante.

## Probando a reducir la cantidad de experiencias a recordar y aumentar el periodo de solicitud de decision

Pruebo a poner el DecisionPeriod a 5, reducir la penalización existencial a -0.08f y hacer que la cantidad de experiencias a recordar por el agente sea solo de 8, a ver qué pasa.

```c#
public override void OnEpisodeBegin()
    {
        SpawnObstacles();
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-9, 9), 0.5f, -11);
        transform.localRotation = Quaternion.identity;
        if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) == 0)
        {
            this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod = 10;
        } else
        {
            this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod = 5;
        }
    }
```

```c#
public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        float multiplier = 1f;
        if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) > 0) multiplier = 8f;
        AddReward(-0.01f * multiplier);
    }
```

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 3
      use_recurrent: true
      sequence_length: 8
      memory_size: 512
```

RUN-ID: MovingObstacles_LSTM8_DR5_1

Esto no va del todo mal, si lo ves en frío puede ser incluso decente. Veo tres grandes fallos:

- El agente se sale bastante de los límites. Creo que lo voy a solucionar añadiendo un rayo que detecte las paredes laterales (en los sensores que detectan el target, los verdes).
- El agente se choca contra contra obstáculos que ya ha pasado, se queda a su altura. Lo voy a intentar solucionar aumentando el ángulo de los rayos que detectan obstáculos (rojos).
- El agente se choca contra obstáculos que van rápido. Entiendo que de estos obstáculos tiene pocos recuerdos, por lo que igual hay que reducir aún más el número de experiencias (a 4 o así, veremos).

### Cambios

En RayPerceptionSensorObstacles, he metido 12 Rays Per Direction y 180 grados por cada lado (rayos por todos los lados).

En RayPerceptionSensorTarget, he metido una nueva tag (ahora son 2), target y wall.

En el config.yaml, hago que el sequence_length de LSTM sea 5.

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 3
      use_recurrent: true
      sequence_length: 5
      memory_size: 512
```

RUN-ID: MovingObstacles_LSTM8_DR5_2 <-- esta es buenísima (el archivo ___extended1.onnx contiene la mejor versión hasta la fecha)

Da unos resultados muy buenos, prácticamente me podría conformar con eso. Sin embargo, quiero intentar corregir dos tipos de fallos que veo:

- Se choca contra obstáculos que vienen más o menos rápido y que ha podido ver muy poco (casi siempre porque lo ha estado tapando otro obstáculo). Aquí lo que yo supongo que pasa es que se piensa que la velocidad de este es la misma que la del viejo. Igual se podría solucionar reduciendo aún más la cantidad de experiencias a recordar. **Probar qué pasa con 2 o 3 de sequence_length ** (si perdemos puntuación o si conseguimos corregir esto)
- Se choca a veces (muy pocas veces) con las paredes laterales. Entiendo que esto se corregirá dejándolo entrenar más tiempo, ya que el castigo por chocarse con estas cosas es bastante alto (30 pts). Update: esto se ha corregido con más tiempo de entrenamiento.

**También hay que probar con 3 obstáculos**. Ahora mismo el CL está configurado para que entre en acción cuando se llegue a los 91 puntos de media, que dista de lo ideal al haber cambiado el peso de la penalización existencial. Lo mejor será meterlo en un punto fijo (porcentual) del entrenamiento, por ejemplo, al haber llegado al 35% del entrenamiento. Una vez tengamos esto, probar con más obstáculos, para ver si generaliza.

He probado a coger MovingObstacles_LSTM8_DR5_2 tras 1M steps y meterle 3 obstáculos, se ve que generaliza bastante bien, pasa de tener un 90% de aciertos (acierto = llegar al target independientemente del tiempo necesitado para ello) a un 77%. También me he dado cuenta de que este porcentaje aumenta si se aumenta a la hora de hacer inferencia el periodo de decision (se ha entrenado con un periodo de 5, pero al hacer inferencia si lo ponemos a 1 el rendimiento aumenta). **Entiendo que lo mejor sería tener un periodo de decisión más bajo para entrenar, pero esto implicaría que tardaría mucho más en entrenar** (los episodios son notablemente más largos). Si tengo tiempo probar esto

## Prueba con mayor frecuencia de decisión (3 en vez de 5) - FRACASO

RUN-ID: MovingObstacles_LSTM8_DR3_1

```c#
public override void OnEpisodeBegin()
    {
        SpawnObstacles();
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-9, 9), 0.5f, -11);
        transform.localRotation = Quaternion.identity;
        if ((int)Academy.Instance.EnvironmentParameters.GetWithDefault("active_obstacles", 2.0f) == 0)
        {
            this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod = 10;
        } else
        {
            this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod = 3; // 5 es el bueno para entrenar
        }
        finishedEpisodes++;
    }
```

Allá vamos. El resto de configuración es la misma que la anterior versión. Entiendo que tardará bastante más. Lo dejo sin prisa.

RESULTADO: fracaso absoluto. A la hora de entrenar, es mejor con periodos de decisión mayores (5 ha dado buenos resultados).



