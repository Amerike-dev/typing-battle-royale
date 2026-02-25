```mermaid
graph TD
    A([Inicio]) --> B[Abrir aplicación de Windows]
    B --> C[Ver PIN en la pantalla del Host]
    C --> D[Ingresar nombre y PIN para acceder]
    D --> E[Buscar la sala en Unity Lobby con el PIN]
    E --> F{¿PIN válido?}
    F -- No --> G[Mostrar error]
    G --> D
    F -- Sí --> H[Obtener datos de conexión del Unity Relay]
    H --> I[Conexión con el Host mediante <br/>Netcode for GameObjects]
    I --> J[Esperar a que el Host inicie la partida]
    J --> K([Fin])