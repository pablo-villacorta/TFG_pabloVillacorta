# Obstáculos móviles - LSTM - Hay que mejorar un poco más

Voy a probar a reducir la complejidad de la NN. Voy a quitarle una capa interna (que tenga 2), a ver si mantiene los resultados y aprende más rápido.

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: true
      sequence_length: 5
      memory_size: 512
```

RUN-ID: MovingObstacles_LSTM_2layers_1

Tiene un rendimiento parecido a la versión MovingObstacles_LSTM8_DR5_fullCL_1, incluso va un poco adelantado. De momento parto de la premisa de que con 2 capas internas vale. Ahora voy a probar a poner la memoria de LSTM a 128 (actualmente es 512, lo que creo que es excesivo, y sequence_length a 4).

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: true
      sequence_length: 4
      memory_size: 128
```

RUN-ID: MovingObstacles_LSTM_2layers_2

Veremos si hay una mejora sustancial en el ritmo de aprendizaje. Result: no solo ha aprendido algo más rápido, si no que ha sobrepasado el rendimiento de las versiones anteriores. De cada 100 intentos, consigue llegar al otro lado un 85% de las veces. No está mal...

### Prueba tonta: quitar LSTM (prueba de concepto)

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: false
      # sequence_length: 4
      # memory_size: 128
```

RUN-ID: MovingObstacles_NoLSTM_1

Pues vaya. Los resultados son prácticamente los mismos quel a versión MovingObstacles_LSTM_2layers_2. Parece que la memoria no ha aportado nada a MovingObstacles_LSTM_2layers_2. Me quedo bastante descolocado, aunque en el fondo me lo temía.

### Otra prueba: ahora con 10 sequence_length

```yaml
 network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: true
      sequence_length: 10
      memory_size: 128
```

A ver qué pasa.

RUN-ID: MovingObstacles_LSTM10_1

Nada, muy parecido a las dos versiones anteriores. Parece que la memoria no supone valor añadido.

## Incorporando diferentes velocidades máximas con CL

```yaml
max_obstacle_speed:
    curriculum:
      - name: OnlySlowObstacles
        completion_criteria:
          measure: progress
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 200
          threshold: 0.05
        value: 0.01
      - name: SlowMediumObstacles
        completion_criteria:
          measure: progress
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 200
          threshold: 0.1
        value: 0.05
      - name: AllObstacles
        value: 0.1
```

Los thresholds son 500k y 1M pasos.

RUN-ID: MovingObstacles_LSTM4_differentSpeeds_1

Buenos resultados. Hay que tener en cuenta que tienen menor velocidad máxima que todas las versiones anteriores (0.1f vs 0.15f). De cada 100 intentos, llega al otro lado más de 90. Con esto podríamos contentarnos (rodea muy bien obstáculos lentos). Tengo mis dudas de que la memoria sea necesaria...

## Último intento: poner DecisionPeriod a 1, para que en todo paso se solicite una acción

He leído en un artículo de medium (https://towardsdatascience.com/cubetrack-deep-rl-for-active-tracking-with-unity-ml-agents-6b92d58acb5d), que un problema que tuvo la autora fue que tenía el DecisionPeriod a 10, y tenía seleccionada la opción "Take actions between decisions", de forma que realizaba la acción indicada por la brain durante 10 pasos seguidos. Siguiendo esta lógica, esto podría explicar por qué el modelo a veces se choca sin explicación aparente:

Mis mejores modelos han sido con periodos de decisión de 5, y con la opción de tomar acciones entre decisiones seleccionada. Quizás esto pueda explicar el por qué se choca el agente contra obstáculos que o van muy rápido o los tiene cerca...

Pruebo a poner el decision period a 1 (en el código), de forma que solicite una acción cada step. Veamos qué ocurre.

RUN-ID: MovingObstacles_LSTM_DR1_10

Buf. Tras 5M de pasos de entrenamiento, tenemos un agente muy inestable, con una media de 60 puntos con 3 obstáculos con 0.1f de velocidad máxima. Calculo que para llegar a los 65 de media se necesitarían unos 2.5M de pasos más (siguiendo la línea del gráfico).

Mi mejor baza es esta: MovingObstacles_LSTM4_differentSpeeds_1, con velocidades máximas de 0.1f.

## TO-DO

- Seguir entrenando MovingObstacles_LSTM_2layers_2, es la mejor que tenemos de momento
- Hacer CL de verdad con la velocidad de los obstáculos. Empezar **muy lento**, y que empiece a acelerar una vez haya llegado a los 3 obstáculos (que entrene con 3 obstáculos muy lentos durante un rato, luego ya metemos velocidad).
- Probar a hacer los rayos que detectan los obstáculos más largos.
- Probar a dejar mucho rato con un periodo de decisión bajo (e.g. 2). Al hacer esto, duplicar la memoria (a ver qué pasa)

Luego voy a probar a cambiar:

- El número de agentes que entrenan a la vez (acutalmente es 36, me parece demasiado, igual lo reduzco a la mitad)
- El sequence_length del LSTM. Probar con más o con menos...





