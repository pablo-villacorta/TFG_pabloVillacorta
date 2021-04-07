# Obstáculos móviles - Probando LSTM + NN más compleja

Añado una hidden layer adicional, manteniendo 128 unidades en cada una de las capas (128 es el valor por defecto, si no funciona igual lo duplico).

````yaml
 network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 3
      use_recurrent: true
      sequence_length: 16
      memory_size: 256
````

El resto de configuración es la misma que donde lo dejamos en la versión anterior.

RUN-ID: MovingObstaclesLSTM_16_256_4

Empiezo a pensar que quizás debería hacer que el periodo del DecisionRequester sea más bajo. Si no he entendido mal, con dicho periodo puesto a 10, se debería pedir una decisión cada 10 frames (o steps, no sé). Si reduzco ese número, igual el agente tendrá mayor control y recogerá más observaciones, siendo más preciso a la hora de detectar la velocidad de un obstáculo (esto es solo una hipótesis).

Al meter 2 obstáculos, bajonazo importante (44 puntos).

Otro fracaso. Tras 220k pasos, máxima de 75. Cambio para duplicar las unidades/capa interna.

````yaml
 network_settings:
      normalize: false
      hidden_units: 256
      num_layers: 3
      use_recurrent: true
      sequence_length: 16
      memory_size: 256
````

RUN-ID: MovingObstaclesLSTM_16_256_5

Si esto no funciona, aumentamos los números de LSTM. Si no funciona, reducimos la cantidad de pasos entre decisiones. Forzamos la máquina al máximo. 

No funciona. Pruebo a meterle más memoria al LSTM:

```yaml
network_settings:
      normalize: false
      hidden_units: 256
      num_layers: 3
      use_recurrent: true
      sequence_length: 16
      memory_size: 512
```

Probamos.

RUN-ID:MovingObstaclesLSTM_16_512_1

Ha sido un poco montaña rusa. Ha empezado lento, y después ha tenido un momento de colapso, en el que los agentes más o menos llegaban al otro lado, pero no llegaban a colisionar con el target (se quedaban dando vueltas), y tenía -44 de puntuación cada vez. Tras un rato, han "reaprendido", y han ido mejorando hasta llegar a una media de unos 75 y un máximo de 79. Lo he parado a los 500k pasos.

Habiendo examinado el modelo ya entrenado, saco las conclusiones de que el agente no sabe distinguir las velocidades de dos obstáculos diferentes. Creo que eso va a ser un problema gordo en este caso. Esto me acaba de dar una idea. Hasta ahora, la longitud de los rayos era muy grande (90, daba para pillar cualquier punto del entorno). Si lo modifico para que sea tan corto que solo pueda pillar un obstáculo cada vez, esto podría darnos ventaja. Hago que el agente esté "ciego", y que solo sea capaz de pillar los obstáculos que tiene delante. Ahora mismo he puesto la longitud de los rayos a 5.5. Y si más adelante necesitamos detectar otros objetos que están más lejos, podemos añadir otro componente raycast sensor dedicado, que tenga más rango que el que detecta los obstáculos. Vamos a probar así (con la misma configuración que la anterior run). También me cargo la etiqueta "target" del componente de raycast sensor ya que no nos va a servir de mucho al tener un rango tan corto (si más adelante veo que es necesario detectar el target, añado un comopnente raycast nuevo). Vale, creo que es evidente que necesitamos tener unos rayos que detecten el target. Añado un nuevo componente raycast sensor para los targets con más rango.

RUN-ID:MovingObstaclesLSTM_16_512_4

Esto nos ha dado resultados muy prometedores. Tras 500k pasos de entrenamiento (media de unos 85 puntos, máxima de 90 - muy positivo en comparación con otros, aunque no ha pasado de los 2 obstáculos), tenemos un agente que apenas se choca con los obstáculos (la única situación en la que se choca es cuando un obstáculo vuelve a su posición original, lo que es entendible, y haré para qué la posición inicial del obstáculo sea fuera del plano, para que el agente pueda verlo venir).

Ahora mismo el principal problema es que la única estrategia del agente es la de esperar a que el obstáculo pase, y esto es un inconveniente cuando el obstáculo que tiene delante es lento (se queda demasiado tiempo esperando en lugar de rodear el obstáculo). Formas que se me ocurren de mitigar esto:

- Aumentar la penalidad existencial (probar a duplicarla o a triplicarla, para meterle prisa al agente)
- Penalizar de forma manual al agente por pasar más de X pasos sin realizar una acción (sería únicamente implementar un contador, pero perderíamos ese plus para que emerjan estrategias nuevas)

Probaré estas estrategias en el próximo commit.

Antes de eso, voy a probar a rebajar la cantidad de unidades por capa intermedia, ya que no ha demostrado ser una opción más eficaz, y cuantas menos unidades mejor para tener un agente más estable y que aprende más rápido. Rebajo las unidades/capa a 128 (el valor por defecto). Settings:

````yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 3
      use_recurrent: true
      sequence_length: 16
      memory_size: 512
````

Vamos allá.

RUN-ID: MovingObstaclesLSTM_16_512_5

Parece que objetivo conseguido. Tarda menos tiempo (22 min vs 26 mins) que el anterior en llegar a los 250k en comparación con el anterior. Los resultados son parecidos, son buenos: máximas de 87 media de 84.

Ahora voy a probar a reducir la memoria de LSTM de 512 a 256, ya que la he subido un poco sin criterio, y tal vez no sea necesario. New settings:

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 3
      use_recurrent: true
      sequence_length: 16
      memory_size: 256
```

Vamos allá.

RUN-ID: MovingObstaclesLSTM_16_512_6

Parece que no acelera apenas el proceso de entrenamiento, y el rendimiento parece ser ligeramente inferior. Lo voy a dejar como estaba, por si acaso (en 512).

Antes, pruebo a poner la memoria a 1024, para ver si conseguimos una mejora de verdad.

RUN-ID: MovingObstaclesLSTM_16_512_7

Obtenemos resultados muy similares a la versión de 512. Lo dejo como estaba (512).

### TO-DO para la siguiente versión

Tratar de animar al agente a que no espere a que pasen los obstáculos lentos, y que intente rodearlos, probando estas estrategias (por separado o conjuntamente):

- Aumentar la penalidad existencial (probar a duplicarla o a triplicarla, para meterle prisa al agente)
- Penalizar de forma manual al agente por pasar más de X pasos sin realizar una acción (sería únicamente implementar un contador, pero perderíamos ese plus para que emerjan estrategias nuevas)

Si conseguimos que el agente aprenda cuándo tiene que esperar y cuándo rodear, superaremos fácilmente la barrera de los 90 puntos.



