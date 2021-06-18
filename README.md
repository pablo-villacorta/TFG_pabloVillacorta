# TFG Pablo Villacorta

## Diseño y experimentación con técnicas de aprendizaje por refuerzo en diferentes escenarios de juego con Unity ML-Agents

Este repositorio contiene todas las versiones que se han ido creando durante el proceso de experimentación del proyecto. El repositorio tiene 4 branches, una para cada escenario con el que se ha experimentado:

- **master**: desarrollo inicial y escenario 1 (obstáculos inmóviles/estáticos)
- **movingObstacles**: escenario 2 (obstáculos móviles)
- **competitiveFreeze**: escenario 3
- **competitiveFlag**: escenario 4 (juego del pañuelo)

Cada commit cuenta con un README.md en el que se ha ido describiendo el proceso de experimentación. Dentro del directorio /results están todos los modelos .onnx que se han ido entrenando, identificados por su RUN-ID (los modelos más relevantes han sido comentados en alguna de las diferentes versiones del README, así que es posible encontrar una descripción de un modelo concreto en este fichero, aunque habrá que buscar entre las diferentes versiones de este README).

A continuación, se indica el modelo que se corresponde con cada una de las iteraciones que han sido incluidas en la memoria del proyecto:

| Escenario | Iteración | Branch            | Commit                                                       | RUN-ID (modelo)                 |
| :-------: | :-------: | ----------------- | ------------------------------------------------------------ | ------------------------------- |
|     1     |     1     | master            | [Para la memoria] Escenario 1 (más cambios)                  | StaticObstacles1AgentNoCL_2     |
|     1     |     2     | master            | [Para la memoria] Escenario 1 (más cambios)                  | StaticObstacles1AgentWithCL_1   |
|     1     |     3     | master            | [Para la memoria] Escenario 1 (más cambios)                  | StaticObstacles9AgentsWithCL_2  |
|     2     |     1     | movingObstacles   | Probando LSTM                                                | MovingObstaclesLSTM_16_256_3    |
|     2     |     2     | movingObstacles   | LSTM en duda, pero tenemos 90% de exito                      | MovingObstacles_LSTM_2layers_2  |
|     2     |     3     | movingObstacles   | GAIL 20 (tenemos algo decente)                               | MovingObstacles_GAIL_20         |
|     3     |     1     | competitiveFreeze | [Self Play] Corridor - Estrategia de empuje pero no salen del corridor | SelfPlay_FreezeTool_Corridor_7  |
|     3     |     2     | competitiveFreeze | [Freeze Tool] Juegan más o menos decente tras 1.6M steps, tras 6M no | SelfPlay_FreezeTool_Corridor_17 |
|     4     |     1     | competitiveFlag   | Pañuelito – Tenemos algo, pero demasiada “calma tensa”       | SelfPlay_Flag_NoTool_Multi_3    |
|     4     |     2     | competitiveFlag   | Conclusiones adicionales flag                                | SelfPlay_Flag_NoTool_Multi_7    |

Nota: es posible que, dentro de un mismo escenario, un modelo que sea válido en una iteración no lo siga siendo para una iteración posterior (al haberse hecho cambios en el agente), por lo que, para poder probar un modelo, se recomienda hacer un rollback temporal al commit de dicha iteración.

## Versiones

- Unity: 2020.1.17f1
- ML-Agents (paquete de Unity): 1.8.1-preview
- Python: 3.7.9

**Versiones librerías python (pip freeze)**:

absl-py==0.11.0
attrs==20.3.0
cached-property==1.5.2
cachetools==4.2.1
cattrs==1.0.0
certifi==2020.12.5
chardet==4.0.0
cloudpickle==1.6.0
google-auth==1.27.0
google-auth-oauthlib==0.4.2
grpcio==1.36.0
h5py==3.2.0
idna==2.10
importlib-metadata==3.7.0
Markdown==3.3.4
mlagents==0.24.0
mlagents-envs==0.24.0
numpy==1.20.1
oauthlib==3.1.0
Pillow==8.1.1
protobuf==3.15.3
pyasn1==0.4.8
pyasn1-modules==0.2.8
pypiwin32==223
pywin32==300
PyYAML==5.4.1
requests==2.25.1
requests-oauthlib==1.3.0
rsa==4.7.2
six==1.15.0
tensorboard==2.4.1
tensorboard-plugin-wit==1.8.0
torch==1.7.1+cu110
typing-extensions==3.7.4.3
urllib3==1.26.3
Werkzeug==1.0.1
zipp==3.4.0