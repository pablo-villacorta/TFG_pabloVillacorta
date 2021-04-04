# Obstáculos móviles - Probando LSTM

He hecho las modificaciones necesarias en el config.yaml para habilitar el uso de redes recurrentes (LSTM). Mi configuración inicial es la siguiente:

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: true
      sequence_length: 16
      memory_size: 256
```

Consecuentemente (y en principio), al poner sequence_length = 16 estoy haciendo que la NN recuerde las últimas 16 observaciones, de forma que debería bastarle para poder intuir la velocidad del obstáculo que tiene delante. Para este RUN utilizo la misma versión que la de la versión anterior (la velocidad máxima de los obstáculos establecida por el CL se sobreescribe para poner una fija, 0.15, para poder sacar conclusiones más facilmente).

RUN-ID: MovingObstaclesLSTM_16_256_1

Con un obstáculo, se nota que el agente aprende más despacio. Pero aprende, poco a poco va consiguiendo más recompensa media, tras 50k pasos ronda los 85. Pero esto no tiene mucho mérito ya que la versión sin LSTM conseguía resultados similares sin hacer uso de ningún tipo de memoria. La clave está en los dos obstáculos.

Observamos un bajonazo importante, bajamos hasta los 56 puntos con dos obstáculos. Subrayar que, a diferencia de como ocurría con versiones anteriores, el agente no decide únicamente ir hacia delante o esperar, sino que hace uso extensivo de los giros para evitar obstáculos, especialmente cuando son lentos.

El agente es bastante inconsistente de momento. Tras 145k pasos, parecía que iba a empezar a subir (ha llegado a los 64 puntos), pero después ha bajado hasta los 48. Posteriormente vuelve a subir hasta los 63, pero parece que de ahí no pasa. Lo voy a dejar 10 minutos más, si veo que no consigue avanzar notablemente, paro y duplico memory_size (igual, aunque tenga 16 observaciones, no tiene suficiente memoria para retenerlas todas - no sé muy bien cómo funciona eso).

Tras 330k pasos, ha consguido un máximo de 77, que no está mal, pero es un poco decepcionante. Paro el entrenamiento, importo el modelo y lo ejecuto realizando inferencia para ver si soy capaz de identificar en qué situaciones falla (y qué comportamientos negativos/positivos tiene el modelo).

No estoy muy seguro de esto, pero creo que es posible que, cuando el primer obstáculo "tapa" al segundo, aunque el agente sea capaz de pasar el primer obstáculo, se piensa que el segundo tiene la misma velocidad que el primero, haciendo que se choque con él. El agente también presenta deficiencias que no se mostraban antes, como el chocarse con las paredes laterales o incluso cometer "suicidio", al darse la vuelta y chocar con la pared del fondo nada más empezar.

Otra cosa que he visto es que a veces el agente pasa un primer obstáculo, pero se para demasiado pronto, y se queda a la misma altura que el obstáculo que acaba de librar. Consecuentemente, si se queda esperando, se acabará chocando con el primero obstáculo. Para evitar esto, he modificado los ángulos de los rayos, para que sean 90º a cada lado, de forma que el agente tenga visión lateral y pueda ver venir los obstáculos que están a sus lados. No he modificado la cantidad de rayos por cada dirección (sigue siendo 6). **Me acabo de dar cuenta de que llevo varias versiones con 4 stacked raycast sensors, lo que le ha estado dando memoria todo este tiempo al agente :((((((**.

Lo primero que hago es hacer que vuelva a ser 1 stacked raycast sensors. Vuelvo a lanzar el entrenamietno, a ver si esta vez se aprecia algo más de mejoría (al menos, debería aprender bastante más rápido, al tener un vector de entrada 4 veces más reducido).

RUN-ID: MovingObstaclesLSTM_16_256_2

Empiezo a pensar que quizás la lesson de un único obstáculo debería ser más corta, ya que al final parece que el agente aprende a ir recto y esperar, únicamente. Veremos.

Un desastre. Tras 150k pasos, no ha conseguido los 95 de media necesarios para pasar a la siguiente lesson (2 obstáculos). Modifico la duración de esta lesson para que sea más asequible.

````yaml
- name: OneObstacle # The '-' is important as this is a list
        completion_criteria:
          measure: reward
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 450
          threshold: 85.0
        value: 1.0
````

RUN-ID: MovingObstaclesLSTM_16_256_3

Tras 50k pasos ya tenemos dos obstáculos. Hemos pegado un bajonzado similar al del principio, ahora con 51 puntos. Veremos si consigue mejorar (no parece). Si esto no funciona creo que el siguiente paso va a ser meter más complejidad a la red. Si no funciona, podemos probar a meter más capacidades LSTM y que se combine con el aumento del número de capas.

Fracaso. Tras 320k pasos, la media no supera los 77 puntos (una máxima de 83, muy outlier). Paro y pruebo a meter más capas (ya para la siguiente versión).

#### Reflexiones 

Cosas que pueden estar obstaculizando el correcto aprendizaje del agente:

- Poco LSTM :arrow_right: meter más sequence_length y memory_size
- Demasiado LSTM (entrenamiento muy lento) :arrow_right: lo contrario a lo anterior
- Poca complejidad de la NN (con las pocas capas que tiene no le da para desarrollar comportamientos lo suficientemente complejos) :arrow_right: meter más capas intermedias
- Los rayos no aportan suficiente información como para poder distinguir las velocidades de diferentes obstáculos. Esto creo que es un problema que con los rayos es inevitable. Sin embargo, quizás sea posible resolverlo a base de meter más capas. Otra posible solución podría ser meter más rayos, pero no estoy seguro de que esto pueda ayudar.
- Igual podemos probar a hacer entrenar con obstáculos más lentos, y una vez que sabemos que funciona, meter más velocidad **poco a poco**