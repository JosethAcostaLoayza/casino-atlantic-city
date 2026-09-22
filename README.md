# Casino Atlantic City — Reto Técnico Backend

Solución backend basada en microservicios para la carga y procesamiento asíncrono de archivos Excel.

## 1. Tecnologías

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* RabbitMQ
* SeaweedFS
* JWT
* MailKit
* Docker / Docker Compose
* Postman

## 2. Arquitectura

```text
                    ┌──────────────┐
                    │   Postman    │
                    └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐
                    │ API Gateway  │
                    └──────┬───────┘
                           │
              ┌────────────┼────────────┐
              ▼            ▼            │
       Authentication    Control        │
                            │            │
                            ├──► SeaweedFS
                            │
                            └──► RabbitMQ
                                  │
                                  ▼
                           ┌──────────────┐
                           │ CargaMasiva  │
                           └──────┬───────┘
                                  │
                                  ├──► PostgreSQL
                                  │
                                  └──► RabbitMQ
                                         │
                                         ▼
                                  ┌──────────────┐
                                  │Notificaciones│
                                  └──────┬───────┘
                                         │
                                         ▼
                                        SMTP
```

## 3. Microservicios

### Authentication

* Login mediante JWT.
* Validación de credenciales.
* Generación del token.
* Persistencia de usuarios con PostgreSQL.

### Control

* Recibe la carga del archivo.
* Registra la trazabilidad de la carga.
* Almacena el Excel en SeaweedFS.
* Publica el mensaje `carga_masiva`.
* Consulta historial y estado de cargas.
* Actualiza los estados de procesamiento.

### CargaMasiva

* Consume `carga_masiva`.
* Descarga el archivo desde SeaweedFS.
* Procesa y valida el Excel.
* Persiste la información en PostgreSQL.
* Actualiza los estados `En proceso`, `Cargado` y `Finalizado`.
* Publica el mensaje `notificaciones`.

### Notificaciones

* Consume `notificaciones`.
* Envía el correo mediante MailKit/SMTP.
* Actualiza la carga a `Notificado`.

## 4. Flujo de procesamiento

```text
POST archivo
    ↓
Control
    ↓
Estado: Pendiente
    ↓
SeaweedFS
    ↓
RabbitMQ → carga_masiva
    ↓
CargaMasiva
    ↓
Estado: En proceso
    ↓
Validación + procesamiento Excel
    ↓
PostgreSQL
    ↓
Estado: Cargado
    ↓
Estado: Finalizado
    ↓
RabbitMQ → notificaciones
    ↓
Notificaciones
    ↓
Correo
    ↓
Estado: Notificado
```

## 5. Estados

```text
Pendiente
   ↓
En proceso
   ↓
Cargado
   ↓
Finalizado
   ↓
Notificado
```

## 6. Ejecución con Docker

Requisitos:

* Docker Desktop
* Docker Compose

Desde la carpeta raíz del proyecto:

```powershell
docker compose up -d --build
```

Verificar servicios:

```powershell
docker compose ps
```

PostgreSQL y RabbitMQ cuentan con `healthcheck` para verificar su disponibilidad antes de iniciar los servicios dependientes.

Los consumidores de RabbitMQ de CargaMasiva y Notificaciones cuentan con reintento automático de conexión. Si RabbitMQ todavía no está disponible durante el arranque, el servicio espera unos segundos y vuelve a intentar la conexión automáticamente.

Ver logs:

```powershell
docker compose logs -f
```

Logs por servicio:

```powershell
docker compose logs -f control
docker compose logs -f carga-masiva
docker compose logs -f notificaciones
```

Detener la solución:

```powershell
docker compose down
```

Para realizar un reinicio completo eliminando también los datos de los volúmenes:

```powershell
docker compose down -v
docker compose up -d --build
```

> `docker compose down -v` elimina los datos persistidos de PostgreSQL, RabbitMQ y SeaweedFS.

## 7. Migraciones

Las migraciones de Entity Framework Core se encuentran en:

```text
src/
├── Authentication.Infrastructure/Migrations/
├── Control.Infrastructure/Migrations/
└── CargaMasiva/Migrations/
```

Las migraciones se aplican automáticamente al iniciar los servicios mediante `Database.MigrateAsync()`.

No es necesario ejecutar manualmente `dotnet ef database update`.

### Procedimiento almacenado

Control incluye la migración:

```text
20260921181259_AddActualizarEstadoProcedure.cs
```

que crea el procedimiento:

```text
sp_actualizar_estado_carga
```

Este procedimiento actualiza el estado de una carga y registra `FechaFin` cuando el estado pasa a `Finalizado`.

## 8. Servicios y puertos

| Servicio            | Puerto |
| ------------------- | -----: |
| API Gateway         |   5287 |
| Control             |   5264 |
| Authentication      |   5170 |
| CargaMasiva         |   5270 |
| PostgreSQL          |   5433 |
| RabbitMQ            |   5672 |
| RabbitMQ Management |  15672 |
| SeaweedFS Filer     |   8888 |
| SeaweedFS Master    |   9333 |
| SMTP                |   2525 |
| smtp4dev Web        |   5000 |

## 9. RabbitMQ

Colas utilizadas:

```text
carga_masiva
notificaciones
```

RabbitMQ utiliza ACK manual para confirmar el procesamiento de los mensajes.

## 10. Postman

La solución no incluye frontend React.

La colección de Postman se encuentra en:

```text
postman/Casino-Atlantic-City.postman_collection.json
```

Incluye los endpoints necesarios para probar:

* Login
* Crear carga
* Consultar historial
* Consultar carga
* Consultar contenido
* Actualizar estado, cuando corresponda

Flujo recomendado:

```text
1. Login
2. Crear carga
3. Consultar historial
4. Consultar estado
5. Consultar contenido
```

El procesamiento de la carga es asíncrono. El token JWT obtenido en el login se utiliza para los endpoints protegidos.

## 11. Archivos de prueba

Los archivos utilizados para las pruebas se encuentran en:

```text
test-data/
├── carga-prueba.xlsx
└── carga-duplicados.xlsx
```

### `carga-prueba.xlsx`

Contiene registros válidos y casos de validación como:

* registros duplicados
* código vacío
* precio vacío

### `carga-duplicados.xlsx`

Permite validar el comportamiento frente a códigos repetidos tanto dentro del mismo archivo como respecto a registros existentes.

## 12. Estructura principal

```text
CasinoAtlanticCity/

├── src/
│   ├── ApiGateway/
│   ├── Authentication/
│   ├── Authentication.Application/
│   ├── Authentication.Domain/
│   ├── Authentication.Infrastructure/
│   ├── Control/
│   ├── Control.Application/
│   ├── Control.Domain/
│   ├── Control.Infrastructure/
│   ├── CargaMasiva/
│   └── Notificaciones/
│
├── postman/
├── test-data/
├── docker-compose.yml
└── CasinoAtlanticCity.sln
```

## 13. Resultado

La solución implementa el flujo completo:

```text
Postman
   ↓
API Gateway
   ↓
Control
   ↓
SeaweedFS + RabbitMQ
   ↓
CargaMasiva
   ↓
PostgreSQL
   ↓
Notificaciones
   ↓
SMTP
```

El procesamiento es asíncrono mediante RabbitMQ y permite realizar la trazabilidad de una carga desde `Pendiente` hasta `Notificado`.


## Video de demostración

Video corto mostrando el flujo completo de la solución funcionando.

[Ver video de demostración](https://github.com/JosethAcostaLoayza/casino-atlantic-city/blob/main/docs/RetoTecnico_AtlanticCity_JosethAcosta.mp4)

