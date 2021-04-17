# Self Play - Probando

Vale, he hecho unos cuantos cambios en el proyecto. Tomando la última versión (commiteada), los cambios más significativos son los siguientes:

- En cada training Area hay 2 agentes (he modificado el prefab para eso), cada uno con sus BehaviorParameters pero con distinto Team ID

- La clase AgentManager que sirve para coordinar ambos agentes en cada training area (por ejemplo, para terminar los episodios de los dos agentes a la vez)

- La clase agent tiene una referencia al AgentManager de su training area, y otra referencia al otro agente para poder restarle puntos al ganarle

- Se ha añadido un nuevo componente raycast sensor, para poder detectar al otro agente

- Se ha modificado el fichero de config.yaml para incluir el self play ( y he quitado GAIL):

  - ```yaml
    network_settings:
          normalize: false
          hidden_units: 128
          num_layers: 2
          use_recurrent: false
          # sequence_length: 16
          # memory_size: 256
    self_play:
          window: 10
          play_against_latest_model_ratio: 0.5
          save_steps: 5000
          swap_steps: 2000
          team_change: 10000
    ```

Bien, y con estos cambios ya me he puesto a probar. Tras unas cuantas rondas fallidas (por fallos tontos que no merece la pena mencionar), el primer entrenamiento relativamente satisfactorio con self play ha sido SelfPlay_NoTools_6 (he empezado manteniendo las acciones de los agentes, en breve añadiré alguna herramienta especial para poner el asunto más interesante). De momento el objetivo de cada agente es llegar al otro lado antes que su rival. Destacar que durante este entrenamiento las colisiones entre agentes se han desactivado, de forma que los agentes podían "atravesar" a sus rivales como si nada (aunque sí que los podían ver).

Esquema de recompensas:

- +100 por llegar al otro lado -> -50 al rival por perder
- -30 por chocarse con un obstáculo o pared (en vez de perder directamente, hago que vuelva a una posición de salida inicial - random)
- -0.01f de existential penalty (me he cargado el multiplicador que había en versiones anteriores)
- El decision period lo he dejado como estaba, a 10 cuando no hay obstáculos, a 5 cuando sí que los hay.

Ahora voy a probar a activar las colisiones entre los agentes, para ver si, dejándolos entrenar un rato, desarrollan algún comportamiento interesante (activando la casilla correspondiente en la matriz de colisiones del proyecto).

RUN-ID: SelfPlay_NoTools_7

Vale, tras 140k pasos de entrenamiento, vemos cómo los agentes no han desarrollado ninguna habilidad increíble (que involucre el uso de las colisiones entre agentes para "empujarse"). Voy a probar a dar recompensa al rival cuando un agente se choque con un obstáculo o una pared (a ver si acaba deduciendo que le puede beneficiar eso).

RUN-ID: SelfPlay_NoTools_8

Vale, he hecho que, cuando un agente colisione con un obstáculo o una pared, éste tenga una penalización de -30, y su rival tenga una recompensa de +5. No conseguimos nada espectacular parece. Por el momento voy a dejar esta recompensa adicional, por si más adelante acaba siendo interesante.



