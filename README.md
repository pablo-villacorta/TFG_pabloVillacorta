# Obstáculos móviles - LSTM ok, necesitamos que aprenda a rodear a los lentos

### TO-DO para esta siguiente versión

Tratar de animar al agente a que no espere a que pasen los obstáculos lentos, y que intente rodearlos, probando estas estrategias (por separado o conjuntamente):

- Aumentar la penalidad existencial (probar a duplicarla o a triplicarla, para meterle prisa al agente)
- Penalizar de forma manual al agente por pasar más de X pasos sin realizar una acción (sería únicamente implementar un contador, pero perderíamos ese plus para que emerjan estrategias nuevas)
- Empezar con obstáculos muy lentos -> que el agente aprenda a esquivarlos. Luego, ir poco a poco elevando la velocidad.

Si conseguimos que el agente aprenda cuándo tiene que esperar y cuándo rodear, superaremos fácilmente la barrera de los 90 puntos.

## Estrategia 1: aumentar penalización existencial

## Situación inicial

````c#
public override void OnActionReceived(ActionBuffers actionBuffers)
{
	MoveAgent(actionBuffers.DiscreteActions);
    AddReward(-0.01f);
}
````

## Triplicarla (-0.03f)

RUN-ID: MovingObstaclesLSTM_EP3_1

Esperemos que el agente no aprenda a suicidarse. Parece que tenemos más de lo mismo. Sigue quedándose a esperar ante los obstáculos más lentos. En cuanto a resultados, parecido al anterior (menos puntos, pero hay que tener en cuenta la mayor penalidad existencial).

## x10 (-0.1f)

RUN-ID: MovingObstaclesLSTM_EP10_1

Demasiado duro. Opta por el suicidio.**Pero, podría ser que empezáramos con penalización suave, y una vez llegamos a los 2 obstáculos, incrementar la penalización**.

## x5 (-0.5f)

RUN-ID: MovingObstaclesLSTM_EP5_1

Lo mismo. Como al principio no sabe qué hacer, al tener una penalización existencial tan alta, no le animamos a explorar para que sepa que tiene que llegar al otro lado, y opta por suicidarse.

## Nueva estrategia: en la lesson con cero obstáculos la penalización es -0.01f, pero a partir de ahí es -0.1f

MovingObstaclesLSTM_EP10_2

Parece que funciona, pero al ser la penalización tan alta, los límites puestos para el CL son inalcanzables. Rebajo de 85 a 75 el límite para pasar de 1 a 2 obstáculos:

````yaml
- name: OneObstacle # The '-' is important as this is a list
        completion_criteria:
          measure: reward
          behavior: BasicAgent
          signal_smoothing: true
          min_lesson_length: 450
          threshold: 75.0
        value: 1.0
````

Veamos qué tal.

RUN-ID: MovingObstaclesLSTM_EP10_3

Esto está dando unos resultados muy buenos. Quizás las puntuaciones que da el entrenamiento no son muy altas, pero, viendo el modelo en frio ya entrenado, me he quedado asombrado de lo bien que funciona. Efectivamente, ha aprendido a rodear a los obstáculos lentos y a esperar a que pasen los más rápidos (aunque solo los rodea por el lado derecho, quizás con más tiempo de entrenamiento aprenda a rodearlos por ambos lados). 

Además, me he dado cuenta de que el hecho de que el agente únicamente solicite decisiones cada 10 steps es un problema gordo, ya que es una frecuencia demasiado baja (se puede ver dicha frecuencia al realizar inferencia en vista Scene, viendo los rayos parpadeando). 10 es demasiado poco. Al ser acciones continuas, creo que lo mejor será dejarlo en 1 (el máximo).

TO-DO:

- Seguir entrenando el MovingObstaclesLSTM_EP10_3, ya que tiene buena pinta.
- Entrenar con el DecisionRequester a 1
- Hacer que la posición inicial de los obstáculos sea por fuera, para que no se spawneen justo encima de los agentes, y que estos puedan verlos venir
- Probar modelos anteriores con el DecisionRequester a 1 (quizás haya que reentrenarlos)
- Evaluar los modelos más prometedores en igualdad de condiciones (mismos castigos/recompensas)