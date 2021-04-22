# Self Play - Pañuelito v2

## Entrenando por partes

### Parte 1: solo agente, sin pañuelito [hecho en el commit anterior]

Que el agente aprenda a llegar de un lado al otro (igual que el escenario básico con obstáculos estáticos).

RUN-ID: SelfPlay_Flag_NoTool_Single_4 (entrenado durante 300k)

### Parte 2: solo agente, con pañuelito [hecho en el commit anterior]

Ahora el agente tiene que reentrenarse para pasar primero por el pañuelito, y luego ya hacer como hacía antes. 

RUN-ID: SelfPlay_Flag_NoTool_SingleWFlag_5 (entrenado durante 1M steps)

Ahora tenemos un agente que recoge el pañuelito y lo lleva al otro lado de forma muy optimizada. Este modelo va a poder utilizarse como base de todas las pruebas que hagamos con el juego del pañuelito ya en versión competitiva (lo podemos reutilizar todas las veces que necesitemos, en modo single player lo hace prácticamente perfecto - 112 de media sobre un máximo de 120).

### Parte 3: entrenar competitivo

#### Parte 3a: entrenar directamente competitivo

Vamos a probar esto. Lo pruebo antes que el 3b porque pienso que en el 3b es posible que ningun agente acabe queriendo coger el pañuelito, al proporcionarle una desventaja significativa al ir más lento que su rival.

```c#
public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        if (OtherAgent.isActiveAndEnabled)
        {
            AddReward(-0.00001f);
            return;
        }
        float multiplier = 1f;
        AddReward(-0.01f * multiplier);
    }
```

He metido una pequeña penalización existencial (-0.00001f) para intentar que los agentes no pierdan demasiado el tiempo (en el readme anterior llegué a la conclusión de que quizás fuera necesario, tras los 3M steps de entrenamiento).

Activo los agentes rosas, el self play, y quito el CL (empiezan entrenando directamente con 3 obstáculos). A ver qué tal...

RUN-ID: SelfPlay_Flag_NoTool_Multi_3 (entrenado 3M steps)

Vale, tenemos algo que más o menos ha entendido cómo funciona el juego. El problema? Creo que reside en la función de recompensas. Tras 1.5M steps de entrenamiento, los agentes tienen "miedo" a coger el pañuelito: han llegado a la conclusión de que es más rentable no cogerlo y después pillar al que lo ha cogido. Se quedan constantemente esperando el uno al otro ("calma tensa"). Podría hacer que la máxima recompensa solo pueda ser conseguida al coger el pañuelito y llegar al otro lado, y que, en caso de que te pillen, el castigo sea menor (y la recompensa del que ha "cazado" al otro también es menor que la recompensa cuando se coge el pañuelito y se llega al target). De esta forma, quizás podría alentar a los agentes coger el pañuelito (también podría aumentar la recompensa por cogerlo). Otra opción sería aumentar la penalización existencial. 

Pero este modelo es bastante prometedor. El mero hecho de quedarse esperando en vez de coger el pañuelito hace ver que los agentes han "llegado a la conclusión" de que es mejor dejar que el otro coja el pañuelo. También es destacable el hecho de que han aprendido a empujarse el uno al otro para ganar sin tener que hacer uso del pañuelito (empujar al otro hacia un obstáculo o hacia una pared).

**Prueba**

Paro el entrenamiento de SelfPlay_Flag_NoTool_Multi_3, creo un nuevo modelo basado en este y aumento la penalización existencial. Pongo algo significativamente más heavy. A ver si conseguimos que esa "calma tensa" desaparezca metiendo prisa a los agentes... Igual podemos forzarles a que se empujen el uno al otro. Y también podemos aumentar la recompensa por coger el pañuelito... De esta forma siempre tendrá cierta ventaja (en términos de recompensa) aquel agente que coja el pañuelo. Vamos a probar...

#### Parte 3b: entrenar competitivo pero el agente con pañuelito va más lento

Descartamos este escenario, hemos solucionado el problema de "deducir" que lo que hay que hacer si no coges el pañuelito es ir a por el otro (gracias a que hemos metido más raycasting con esferas de mayor tamaño, por lo que es más facil encontrar al rival y seguirlo). En el modelo del apartado 3a se ve claramente que lo deducen.





## Posibles ideas si vemos que falla

- Yo de momento creo que las dos primeras fases del entrenamiento (single agent sin y con pañuelito) están bastante bien tal y como están entrenados SelfPlay_Flag_NoTool_Single_4 y SelfPlay_Flag_NoTool_SingleWFlag_5. De cambiar algo, supongo que sería el entrenamiento multi agente.
- Dentro del multiagente:
  - Probar a hacer que el agente que haya cogido el pañuelito vea su velocidad reducida (un 10 o 20%, por ejemplo, de forma que el otro agente tenga más posibilidades de aprender que tiene que pillarlo - si no, es muy dificil que le pille las suficientes veces como para aprender que es eso lo que tiene que hacer).
  - Entrenar multi agente con menos obstáculos (empezar con 1 o 2, entrenar el modelo y luego entrenar uno nuevo con 3 obstáculos). 
  - Igual meter un poco de lstm (habría que reentrenar todo de 0). De esta forma, los agentes pueden recordar dónde estaba el rival antes de que cojan el pañuelito (si no, muchas veces no saben si cogerlo o no porque no ven al rival, que puede estar al otro lado del pañuelito). También puede servir para predecir el movimiento del rival para "pillarlo".

