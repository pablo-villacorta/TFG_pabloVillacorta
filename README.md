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

