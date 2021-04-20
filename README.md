# Self Play - Freeze 1 vez con pasillo

Idea: Congelar solo si vas por detrás, máximo 1 uso de la herramienta por episodio y agente. Para igualar las condiciones iniciales (y obligar a que un agente se ponga por delante del otro), crear un pasillo al principio por el que solo quepa un agente al mismo tiempo.

RUN-ID: SelfPlay_FreezeTool_Corridor_5

Aquí, he entrenado un agente solitario (desactivando el rival, el rosa) con curriculum learning (reward, como está solo funciona bien), y una vez tengo el agente funcionando solo más o menos bien con todos los obstáculos, paro el entrenamiento, activo el agente rival, y activo el self play en el config.yaml.

Ahora, comenzamos directamente con dos agentes que saben moverse. Ahora falta que aprendan a competir. De momento, no consigo que emerjan grandes estrategias. Sin embargo, es posible que deba darle una vuelta a la función de recompensas, ya que estoy leyendo que para que funcione correctamente la función de ELO, es necesario tener una función de recompensas más simple (+1 por ganar, -1 por perder, 0 por empate), y quitar las recompensas intermedias. Quizás podría meter una variable de entorno en el fichero de configuración YAML que indique si estamos en un entorno competitivo o no, y que me permita decidir qué función de recompensa usar.

Ahora mismo, el ELO del agente (que ha comenzado siendo 1200) es ahora 1190...

Otra idea para igualar más el juego sería que haya dos posiciones de inicio (una a cada lado), a la misma distancia cada una de la salida del pasillo. De esta forma, ningún agente tiene ventaja posicional sobre el otro.

Si esto no funciona, podríamos hacer que los agentes se deban "ganar" la recompensa, recogiendo algún tipo de objeto que al cogerlo les de un uso de la herramienta. No sé....