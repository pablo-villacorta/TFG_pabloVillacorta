# Obstáculos móviles - LSTM + GAIL

Se me ha ocurrido que antes de pasar a los entornos multi agente, podríamos probar una última cosa: que el agente trate de "clonar" el comportamiento del humano. Para ello, juego yo unos cuantos episodios (cuantos más mejor) que quedan grabados en un archivo ".demo", al que posteriormente puedo hacer referencia en el config.yaml para que el agente trate de replicar mi comportamiento. Esto se supone que debería acelerar considerablemente el aprendizaje.

Hay varias maneras de hacer que el agente aprenda del comportamiento del humano. La que utilizo es GAIL, que coge la idea de las GANs para detectar qué episodios (o pasos, no estoy seguro) son del humano y cuáles son de la máquina. La máquina tiene que intentar que la NN discriminadora se piense que es un humano el que está jugando.

## Prueba 1

Demo file: Demo2_1.demo (289 episodios completos, mean reward de 82,26)

Settings:

```yaml
reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
      gail: 
        strength: 0.8
        gamma: 0.99
        demo_path: Assets/Demonstrations/Demo2_1.demo
        use_actions: true
        use_vail: false
environment_parameters:
  active_obstacles: 
    curriculum:
      - name: NoObstacles
        completion_criteria:
          measure: reward
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 100
          threshold: 50.0
        value: 0.0
        - name: ThreeObstacles
        value: 3.0
  max_obstacle_speed: 0.1
```

He modificado el CL para que pase directamente de 0 obstáculos a 3, ya que todas las demos se han hecho con 3 obstáculos.

RUN-ID: MovingObstacles_GAIL_6

Bastante esperanzador. Aunque la mean reward no es demasiado alta, si haces inferencia con el modelo generado tras 600k pasos, vemos cómo el agente tiene un comportamiento similar al mío (esquiva muy bien obstáculos lentos). Tiene un porcentaje de éxtio del 70%. Ahora toca probar a refinar los parámetros del GAIL, porque una strength de 0.8 es demasiado alta (le está dando prácticamente la misma importancia a los resultados de GAIL que a los de las recompensas extrínsecas, cuando debería ser menor - es posible que haga overfitting de las demostraciones y no esté llegando a generalizar bien, por eso hay que darle más importancia las recompensas extrínsecas, para que las demostraciones sean más una "guía" y no un manual).

## Prueba 2

Pruebo a reducir la strength del GAIL, ahora es 0.3.

```yaml
 reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
      gail: 
        strength: 0.3
        gamma: 0.99
        demo_path: Assets/Demonstrations/Demo2_1.demo
        use_actions: true
        use_vail: false
```

RUN-ID: MovingObstacles_GAIL_7

## Prueba 7 (he hecho bastantes que no han sido documentadas entre medias)

```yaml
gail:
        gamma: 0.99
        strength: 0.3
        encoding_size: 64
        learning_rate: 0.0003
        use_actions: true
        use_vail: false
        demo_path: Assets/Demonstrations/Demo3_0.demo
```

```yaml
curriculum:
    - value:
        sampler_type: constant
        sampler_parameters:
          seed: 612
          value: 0.0
      name: NoObstacles
      completion_criteria:
        behavior: BasicAgent
        measure: reward
        min_lesson_length: 100
        signal_smoothing: true
        threshold: 50.0
        require_reset: false
    - value:
        sampler_type: constant
        sampler_parameters:
          seed: 613
          value: 3.0
      name: ThreeObstacles
      completion_criteria: null
  max_obstacle_speed:
    curriculum:
    - value:
        sampler_type: constant
        sampler_parameters:
          seed: 614
          value: 0.1
      name: max_obstacle_speed
      completion_criteria: null
```

RUN-ID: MovingObstacles_GAIL_13 (Demo3_0.demo)

Me he dado cuenta de que la demo que tenía (Demo2_1.demo) había sido grabada con un decision period (en el código especificado) de 1, por lo que el agente aprendía peor (o muchísimo más despacio, como ya hemos podido comprobar en versiones anteriores). He re-grabado unos 300 episodios con el periodo de decision a 5 (que es el número que mejores resultados me ha dado hasta el momento), y estoy probando a entrenar el agente con esto, para que coincidan el DR de las demos y el del agente real. En este caso, lo he entrenando con la configuración de arriba (strengh=0.3, CL que pasa de 0 a 3 obstáculos).

Una cosa a tener en cuenta es que el tiempo de entrenamiento por step es bastante mayor con GAIL (unos 50-60 segundos de media por cada 5k pasos, en comparación con los 25-30 que había antes). Pero los resultados no son malos. Con este modelo, tras unos 600k pasos de entrenamiento, aunque tan solo obtenemos unos 60 de mean reward, obtenemos un porcentaje de exito de 81%, que no está mal teniendo en cuenta que el record absoluto es de 90%.

Tener en cuenta que el mean reward de la nueva demo (Demo3_0.demo) ronda los 78 puntos, por lo que no esperemos que el agente tenga una puntuación mucho mayor que eso (de mean reward digo, no de % de exito).

Creo que en esta versión el strength del GAIL ha sido demasiado alto, ya que al evaluar el comportamiento del modelo ya entrenado en frío, me parece que hace muchas cosas "estúpidas", creo que está haciendo overfitting de los episodios que hay en las demos. Por eso, ahora voy a probar a reducir la strengh a 0.095, para ver si conseguimos que el agente aprenda por su cuenta, y no únicamente "recuerde" lo que ha visto en las demos (que en realidad no lo ha visto él si no la otra NN discriminadora, pero bueno ya me entiendo).

## Prueba 8

RUN-ID: MovingObstacles_GAIL_14 (Demo3_0.demo)

Cambios con respecto a la versión anterior:

```yaml
gail: 
        strength: 0.095
        gamma: 0.99
        demo_path: Assets/Demonstrations/Demo3_0.demo
        use_actions: true
        use_vail: false
```

Muy bien, tras 1.2M de steps de entrenamiento llegamos a un mean reward de 70 ptos (parece que converge ya y no avanza).

## Prueba 9

Probamos ahora con GAIL strength=0.05

```yaml
 gail: 
        strength: 0.05
        gamma: 0.99
        demo_path: Assets/Demonstrations/Demo3_0.demo
        use_actions: true
        use_vail: false
```

RUN-ID: MovingObstacles_GAIL_15 (Demo3_0.demo)

Muy parecido a la versión anterior (al menos durante los primeros 450k steps)

## Prueba 10

Ahora prueba con GAIL use_actions = false

```yaml
gail: 
        strength: 0.05
        gamma: 0.99
        demo_path: Assets/Demonstrations/Demo3_0.demo
        use_actions: false
        use_vail: false
```

RUN-ID: MovingObstacles_GAIL_16 (Demo3_0.demo)

Sin más. Parecido a los dos anteriores (~60pts tras 350k steps).

Tras analizar la inferencia del modelo en frío, veo que 2/3 de las colisiones son con obstáculos, y el resto con paredes. El modelo llega al otro lado ~82.5% de las veces.

## Prueba 11: más memoria?

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: true
      sequence_length: 16
      memory_size: 256
```

Todo lo demás es igual que la versión anterior (solamente hemos cuadruplicado sequence_length y dulpicado memory_size). Quiero probar si, ahora que el agente tiene más claro que hacer gracias al GAIL, puede llegar a saber usar mejor la memoria (y así detectar mejor cuándo un obstáculo es demasiado lento como para esperar a que se mueva, que es algo que todavía no hemos conseguido del todo).

RUN-ID: MovingObstacles_GAIL_17 (Demo3_0.demo)

# TO-DO

- Probar con curiosity
- Probar con más memoria (sequence_length)