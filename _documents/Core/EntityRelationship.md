```mermaid
erDiagram
    JUGADOR {
        string JugadorID PK
        string NombreUsuario
    }

    PARTIDA {
        string PartidaID PK
        datetime FechaInicio
        float DuracionSegundos
    }

    ESTADISTICA_PARTIDA {
        string EstadisticaID PK
        string PartidaID FK
        string JugadorID FK
        int PosicionFinal "Rango en el Battle Royale"
        int Eliminaciones
        float DanoTotalInfligido
        float DanoTotalRecibido
        int PalabrasCorrectas
        float PrecisionOrtografica "Porcentaje de precisión"
        float PromedioWPM "Palabras por minuto"
    }

    JUGADOR ||--o{ ESTADISTICA_PARTIDA : "genera"
    PARTIDA ||--o{ ESTADISTICA_PARTIDA : "registra"