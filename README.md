# Obstáculos móviles - LSTM + GAIL

Vale, he probado a cambiar la configuración de los rayos del agente. He separado los rayos que detectaban target y walls, ahora son dos componentes separados. Lo he hecho para que la longitud de los rayos que detectan walls sea mucho menor (casi del tamaño del agente), de forma que el agente sea capaz de deducir si está cerca o no de una pared (esto puede ser útil para evitar colisiones con la pared). También he aumentado el ángulo de los rayos que detectan el target, ahora es 110, de forma que el agente, incluso cuando está mirando a los lados, sea capaz de encontrar vías libres hacia el target.

Problema: al cambiar las observaciones, todo cambia. Para empezar, ninguno de los modelos entrenados previamente nos vale. Encima, voy a tener que re-grabar todas las demostraciones, ya que las observaciones son diferentes. Pero confío en que este cambio nos permita llegar a valores más interesantes.

Por cierto, la prueba anterior con memoria adicional no ha sacado resultados concluyentes. Ha empezado aprendiendo más rapido y a mejor ritmo, pero al final ha empezado a converger y se ha estancado en un punto similar a sus versiones anteriores con LSTM más reducido. Encima, no ha conseguido solucionar el problema que teníamos en el que el agente se quedaba esperando a que el obstáculo se "despegara" de la pared, incluso cuando el obstáculo era muy lento. 



Vale, he re-grabado las demostraciones. Fichero: Demo5.demo



Hemos entrenado 3 versiones diferentes:

## MovingObstacles_GAIL_18

GAIL strength: 0.05

## MovingObstacles_GAIL_19

GAIL strength: 0.1

## MovingObstacles_GAIL_20

GAIL strength. 0.01

Esta última versión (20) ha sido la que mejores resultados me ha dado. Tras 6M de steps de entrenamiento (con 36 agentes entrenando en paralelo), obtenemos un mean reward de 78 (casi igual de bueno que yo mismo, un humano). Tiene un porcentaje de acierto que ronda el 93%, es muy alto. Se ha entrenado con LSTM 16 sequence_length.

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: true
      sequence_length: 16
      memory_size: 256
```

Es la mejor versión que tenemos hasta la fecha con obstáculos móviles. La velocidad máxima, como ya sabemos, es de 0.1 (la de los obstáculos). Tener en cuenta que se ha entrenado con el siguiente CL:

```yaml
environment_parameters:
  active_obstacles: 
    curriculum:
      - name: NoObstacles # The '-' is important as this is a list
        completion_criteria:
          measure: reward
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 100
          threshold: 50.0
        value: 0.0
      # - name: OneObstacle # The '-' is important as this is a list
      #   completion_criteria:
      #     measure: reward
      #     behavior: BasicAgent
      #     signal_smoothing: true
      #     min_lesson_length: 100
      #     threshold: 65.0
      #   value: 1.0
      # - name: TwoObstacles # This is the start of the second lesson
      #   completion_criteria:
      #     measure: reward
      #     behavior: BasicAgent
      #     signal_smoothing: true
      #     min_lesson_length: 100
      #     threshold: 65.0
      #     require_reset: true
      #   value: 2.0
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
    #       threshold: 0.05
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

Y estos hiperparámetros

```yaml
network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      use_recurrent: true
      sequence_length: 16
      memory_size: 256
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
      gail: 
        strength: 0.01
        gamma: 0.99
        demo_path: Assets/Demonstrations/Demo5.demo
        use_actions: false
        use_vail: false
```

Creo que me voy a quedar con esto. Un porcentaje de acierto del 93.5% me parece lo suficientemente bueno. Viendo al agente en frío (fuera del entrenamiento), tras los 6M steps, parece que tiene un criterio medianamente decente para saber cuándo pararse a esperar a un obstáculo y cuándo rodearlo. Además, se nota que se ha entrenado con GAIL, porque hay veces en las que se gira 90º y "adelanta" al obstáculo (tal y como hacía yo a veces). Tener en cuenta que no es perfecto, las demostraciones grabadas por mi tampoco lo eran (aunque esto último no debería suponer un gran problema, porque el GAIL strength es 0.01, bastante bajo, para que mis demos sean más una "guía" que un manual de instrucciones para el agente; solo quiero darle pautas generales). De todas formas, podemos estar orgullosos. Una cosa más: probablemente existe una combinación de hiperparametros que nos de un rendimiento aún mejor, pero no me renta dedicarle más tiempo a este tipo de entorno (movingObstacles), ya que este es un proyecto más de experimentación que otra cosa. si necesitáramos un modelo altamente preciso, usaríamos máquinas más potentes, haríamos entrenamientos más largos y realizaríamos un estudio mucho más exhausto de lo que el modelo necesita para ser más preciso. Estamos experimentando: RL, CL, GAIL, etc. Y hemos sacado un modelo medianamente decente, así que ni tan mal.

Nota: el modelo GAIL 20 a veces comete algún que otro fallo tonto, si te preguntan por ello responder que muy seguramente estos fallos desaparecerían si dejáramos entrenar aún más tiempo al modelo, de forma que pueda "pulir" estos fallos.