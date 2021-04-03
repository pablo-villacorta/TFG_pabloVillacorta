# Obstáculos móviles - no LSTM no additional layers

Manteniendo el esquema de castigos de la última versión, pruebo a retar al agente (con exactamente la misma configuración que para los obstáculos estáticos) a hacer lo mismo con obstáculos móviles (usando CL, por lo que empieza con un solo obstáculo). Es importante recalcar que los obstáculos móviles tienen todos exactamente la **misma velocidad** (en términos absoultos, claro). Los obstáculos se mueven o hacia la izquierda o hacia la derecha.

## Todos los obstáculos tienen la misma velocidad

RUN-ID: MovingObstaclesNoLSTM2 (el 1 fue un fail mio, irrelevante).

Tras 50.000 pasos no ha conseguido ni una vez llegar al otro lado. Cambio de estrategia: añado una lesson nueva al CL, haciendo un stage previo con 0 obstáculos para que sepa qué hacer. Veremos qué ocurre.

RUN-ID: MovingObstaclesNoLSTM6 <- este tiene buenos graficos de CL en el tensorboard

Con esta estrategia hemos conseguido el objetivo inicial. Tras menos de 15.000 pasos ya hemos conseguido algo que, con un obstáculo, es capaz de llegar a 50 puntos de media, y subiendo. La clave está en decirle al agente, sin ningún tipo de obstáculo, que esto va de llegar al otro lado. Luego ya meteremos stuff adicional. Pero la base es esa. Tras unos 30k pasos, pasamos a la fase 2 -> 2 obstáculos. La media baja sustancialmente (hasta 58 puntos con 2 obstaculos). Sin embargo, le cuesta mucho avanzar, una vez ha llegado a los 85 de media. Está bastante atascado, llevamos ya 110k pasos y no parece que vayamos a llegar a los 95 puntos que marcan el cambio de fase. Creo que nos va a tocar tirar de LSTM o de redes más profundas. De todos modos lo voy a dejar entrenando un rato más.

Vaya. Me ha pasado a la última fase, pero no porque haya mejorado, sino porque, en el yaml de configuración, la duración de las fases del CL está puesta a 100 episodios, lo que es poco en mi opinión, así que, si tiene un poco de suerte, es posible que saque una secuencia de 100 episodios relativamente sencillos y que saque de media más de 95, para pasar a la siguiente fase, aunque no se lo "merezca" el agente en realidad. Al meterle 3 obstáculos móviles, se vuelve a apreciar el bajonazo: bajamos hasta los 59, aunque se recupera para llegar a los 70 y pico. Lo dejo un rato entrenando y vuelvo para ver si consigue mejorar sustancialmente. Aprovecho este pequeño error para poner duraciones minimas de cada lesson mas largas: 450 para los que tienen algun obstaculo y 200 para el "libre" inicial (sin obstaculos), para que los cambios de fase sean más "correctos" y convenientes (evitar que sean prematuros).

He vuelto. Tras 300k pasos, conseguimos una media de unos 83 puntos (lo que no está nada mal, teniendo en cuenta que el agente no tiene ningún tipo de memoria). También hay que tener en cuenta que todas las velocidades de los obstáculos son las mismas, por lo que la NN puede aprender los "tiempos" de espera, sin tener que llegar a pillar la velocidad de cada obstáculo en cada momento. Voy a repetir el experimento, aleatorizando un poco las velocidades de los obstáculos, para ver si mi teoría se confirma y el agente tiene peor rendimiento. Puntación máxima obtenida: 88

## La velocidad de los obstáculos está ligeramente aleatorizada

Ahora he añadido una propiedad pública estática a la clase MovingObstacle, maxStep = 0.35f. Cada vez que se haga spawn del objeto (onEpidodeBegin), se asignará a la velocidad del obstáculo (xStep) un valor entre -maxStep y maxStep [-0.35f, 0.35f).

RUN-ID: MovingObstaclesNoLSTM7

Le está costando arrancar bastante más que al anterior (se ve bien en el gráfico), al menos durante los primeros 50k steps. Sin embargo, hay esperanzas. Veo cómo el agente ha aprendido a esperar (en los obstáculos que son más lentos se ve claramente cómo espera a que pase). Probablemente en el anterior tmb lo hiciera, pero aquí se puede ver mucho más claro al tener velocidades tan distintas. Otra cosa que recalco es que apenas gira. Como sabe que el obstáculo se va a acabar apartando, prefiere esperar antes que rodearlo (esto es muy útil cuando los obstáculos son rápidos, que es lo que sucede el 95% de las veces). Sin embargo, esto tiene una pega. Cuando la velocidad del obstáculo es muy cercana a cero (el obstáculo apenas se mueve), el agente se encuentra ante una situación desconocida: si se queda esperando a que el obstáculo pase, se le agotará el tiempo (máximo numero de pasos / episodio), pero no ha aprendido a rodear el obstáculo. Igual habría que tirar de CL y empezar con velocidades más lentas, e ir aumentándolas poco a poco (primero que aprenda a rodear, después a esperar).

A todo esto, llevamos 125k pasos y el agente todavía no ha llegado a los 80 de media. Claramente el proceso de aprendizaje es más lento que en el caso anterior (la comparación en tensorboard no deja lugar a dudas). Vamos por los 200k y lo mejor que hemos conseguido ha sido 87 y una media overall de 85. Voy a pararlo, y modificar la estrategia de CL para que incremente progresivamente la velocidad de los obstáculos (para que primero aprenda a rodear, despues a esperar). Añado un segundo parametro, max_obstacle_speed en el config.yaml para ir modificando la velocidad máxima en cada momento.

### La velocidad de los obstáculos se rige gracias al CL

Teniendo 3 lessons -> solo lentos; lentos + medios; todos

RUN-ID: MovingObstaclesNoLSTM14

A diferencia del parametro de máximos obstáculos, este parametro (max_obstacle_speed) avanza a medida que progresa el entrenameinto (% de steps máximos ejecutados), y no por su reward (de esta forma, pueden ir de forma independiente los dos parametros).

Otra cosa importante. He reducido el porcentaje de reward necesario para introducir el primer obstáculo. Mi idea es que,si dejamos al agente demasiado tiempo en un espacio sin obstaculos, se va a pensar que con ir recto hasta el final basta. Y si pasa eso, luego es posible que le cueste más aprender a esquivar y hacer otras cosas. Solo necesita la "intuición" inicial esa de que tiene que ir hacia lo rojo, no queremos que se vuelva un maestro de ir en línea recta.

No sé muy bien por qué, pero con esta estrategia no funciona. Y aunque dejemos de regirlo con el CL (sobreescribiendo el valor recibido del CL), no consigue nada decente. Creo que lo mejor va a ser dejar a un lado esta estrategia y probar con LSTM y ver si nos da mejores resultados. Si no, igual toca pararse a pensar qué puede estar fallando (pensar bien lo de esperar / rodear, puede que ahí esté la clave, aunque debería ser el agente el que la deduzca en principio)