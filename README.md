# Self Play - Corridor (no se puede morir dentro) con freeze

### Qué hacer ahora

Se me ocurren varias ideas:

1. Basándome en este mismo escenario, rehacer todo con los siguientes cambios:
   - Las paredes del corridor no son "letales" (chocar con ellas no provoca nada)
   - Los límites de pista antes del corridor tampoco son letales 
     - Todo esto lo hago para forzar a los agentes a salir del corridor
2. Una vez implementado 1 y habiendo probado pequeñas variaciones, probar esta otra idea (en una branch nueva): el pañuelito. La idea es que ambos agentes empiezan abajo, y ponemos un pañuelito en un punto concreto del escenario. El objetivo de los agentes es coger el primero el pañuelito y llegar al target con él. Si el otro agente ha cogido antes el pañuelito, el objetivo del agente será "robárselo", intentando chocar con él. Si chocan, el agente que no había cogido el pañuelito gana. Este escenario puede ser interesante porque los comportamientos tienen varias "fases" y variaciones (primero, llegar el primero al pañuelito; luego, dependiendo de si lo ha cogido o no, hacer una cosa u otra). Este escenario puede ser muy interesante.

## Idea 1: que  no se pueda ganar sin salir del corridor

He implementado la idea 1. Vuelvo a repetir el procedimiento realizado con la versión anterior: 

- en el yaml, comento la parte de self play
- en unity, desactivo los agentes rosas

De esta forma, el agente se hace más o menos bueno él solo. Lo dejo entrenando un rato, y cuando ya sea más o menos bueno, activo el self play y el agente rosa, y que aprendan a competir (esta vez deberían acabar dejándose salir, porque es la única forma de obtener una recompensa, tanto positiva como negativa).

RUN-ID: SelfPlay_FreezeTool_Corridor_9

Vale, ha estado entrenadno unos 650k steps solo. Ahora activo self play y el agente rosa y lo dejo entrenando un buen rato. A ver qué emerge...

Pues, a los 2M de steps, claramente no hay estrategia. Tenemos ELO por debajo del inicial (1170). Ahora los agentes tratan de salir rápidamente del corridor, pero aun así no logran encontrar una manera de encontrar la herramienta de forma que les proporcione una ventaja sustancial sobre el otro.

Tal y como está ahora no va a emerger ninguna estrategia, porque sinceramente creo que no existe. Lo único que se me ocurre que podría dar algo que hacer en este escenario podría ser que solo tenga disponible la herramienta el último en salir del corridor. Voy a probar esto, y en caso de que no funcione, pasamos a  probar lo del pañuelito.

Vale, lo he parado.

Ahora, he implementado lo descrito en el parrafo anterior. He añadido un trigger a la salida del corridor, por defecto el currentToolStatus está a 0 en ambos agentes, y al entrar al trigger de salida del corridor se comprueba si hemos sido los primeros o los segundos en salir del mismo. En caso de ser los segundos, currentToolStatus se pone al máximo (para poder usarlo una vez).

Vale, en vez de reentrenar el bicho en solitario de cero, voy a intentar tomar donde lo dejé en el ejemplo anterior (2M steps, SelfPlay_FreezeTool_Corridor_9), y a ver si aprenden esta nueva situación.

RUN-ID: SelfPlay_FreezeTool_Corridor_9 --resume (tras 2M steps con la nueva configuración).

Vale, no me ha dejado reentrenar (no sé por qué). Nada, no me deja. Puesh abrá que hacerlo de cero...

RUN-ID: SelfPlay_FreezeTool_Corridor_16

Vale, tras encontrarme con bastantes problemas para que algente aprendiera a psar del corridor (estando él solo), he llegado a la conclusión de que el problema estaba en que el castigo por chocarse con los obstáculos y paredes era demasiado alto (-30), por lo que el agente prefería quedarse quieto antes de intentar explorar. Ahora, lo he puesto a -10, y el agente ya está dando con la tecla (antes ni con 200k pasos lograba aprender, ahora a los 50k ya entiende de qué va la cosa, con 1 obstáculo al menos). Lo dejo un rato más para que afine y aprenda con 2 y 3 obstáculos, y probamos el competitivo con un único agente que puede usar la herramienta (el último en salir del corridor). Si esto no lleva a ninguna estrategia, cambiamos al escenario del pañuelito.

Vale, lo he dejado entrenando en solitario 565k pasos. Ha llegado a los 3 obstáculos y con un mean reward de 80+. Con eso vale de momento. Ahora activo self play y el agente rosa.

Vale, lo he dejado entrenando con self play durante 3M steps (casi). Resultado: apenas hay estrategia. Básicamente, utilizar la herramienta tan pronto como esté disponible (al salir del corridor). Esta es una estrategia de media da buenos resultados, siempre y cuando el agente que va por delante no tenga una ventaja demasiado grande. Los agentes no intentan, por ejemplo, empujar al rival para que se choque con un obstáculo. A veces se quedan atascados en la salida del corridor (no caben los dos a la vez).

Creo que este escenario no me va a permitir ir mucho más allá. Voy a probar a implementar el pañuelito.

# UPDATE

He cogido SelfPlay_FreezeTool_Corridor_16 y he creado un nuevo modelo a partir de él, SelfPlay_FreezeTool_Corridor_17. Lo he dejado entrenando durante 6+M de steps. El modelo generado al final de dicho entrenamiento hacía que los agentes nunca salieran de la sala (saben que el que sale primero tiene mayor probabilidad de perder, así que ninguno quiere salir primero). sin embargo, si cogemos el modelo generado con tan solo 1.6M de entrenamiento, vemos una actividad mucho más interesante, en la que los agentes más o menos juegan como lo haría un humano. Esto me puede servir para decir que, al entrenar más, los agentes llegan a esa conclusión de que es mejor no salir primero, y por eso juegan tan "mal". También es importante mencionar que no tiene penalización existencial, por lo que no se les está metiendo ningún tipo de prisa.

### TO-DO

- Probar a reentrenar SelfPlay_FreezeTool_Corridor_16 con penalización existencial. Para ver si los agentes llegan a la misma conclusión que la versión sin penalización existencial. Esa va a ser la última prueba que voy a hacer.



