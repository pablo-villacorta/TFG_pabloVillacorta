# Obstáculos estáticos con curriculum learning

En este escenario ya hemos implementado correctamente el curriculum learning (a diferencia de la versión anterior, en la que lo hacíamos "manualmente", desde el código, basándonos en el número de episodios exitosos realizados en cada momento). Esto nos permite modelar muchísimo lo que podemos hacer y cómo podemos evolucionar el escenario para que el agente sea capaz de hacer acciones más complejas cada vez.

En esta versión el curriculum learning (CL) únicamente tiene 3 fases, en la primera hay un solo obstáculo, en la segunda 2, en la tercera 3. De esta forma, el agente en la primera fase ya puede aprender fácilmente que su objetivo primordial es el de llegar al otro lado del plano (y que tiene que evitar el obstáculo). Cuando llega a una puntuación concreta (90 en este caso), se cambia de fase, y pasamos a tener dos obstáculos. Vemos cómo se reduce el rendimiento del agente (la puntuación baja de golpe), pero poco a poco se recupera, a medida que se acostumbra a tener dos obstáculos. Al llegar a los 85, el rendimiento vuelve a bajar a causa de un nuevo cambio de fase, esta vez para tener un total de 3 obstáculos activos, dificultando la tarea del agente.

Los runs importantes de esta sesión son:

- CurriculumLearning1 (el primero de prueba, satisfactorio pero no se le ha dejado entrenar mucho, los thresholds para cambio de fase eran 90 y 85)
- CurriculumLearning3 (igual que el 1, pero dejándolo más tiempo para hacer capturas interesantes del tensorboard, thresholds ahora son 95 y 95) -> se llega a una media de 95 tras unos 5 minutos de entrenamiento (con 3 obstáculos)

Es una pena que en los gráficos no se ve cómo baja el reward al cambiar de fase, igual es demasiado fácil para el agente (se nota que aprende muy rápido, la verdad - la fase 2 apenas dura, es muy rápida)

También he aprovechado para refarctorizar el código de la clase BasicAgent, haciendo que las referencias a los obstáculos se mantengan a través de una List<GameObject> obstacles, para evitar tener que modificar el código para meter o quitar obstáculos.

Para saber cómo implementar el currículum learning: https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-ML-Agents.md#environment-parameters