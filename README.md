# CourierMax

API REST para la gestión del ciclo de vida de envíos de una empresa de
courier: creación, seguimiento de estados, asignación a vehículo/conductor,
cálculo de tarifas, alertas de SLA y métricas de eficiencia por conductor.

Construida con **.NET 9 / ASP.NET Core Web API** como prueba técnica de
seniority (ver [enunciado](../enunciado-prueba-dotnet-seniority.md)).

---

## Tecnologías utilizadas

| Categoría | Tecnología |
|---|---|
| Runtime / Framework | .NET 9.0, ASP.NET Core Web API |
| Persistencia | Entity Framework Core 9.0 (Code-First + Migrations), SQL Server |
| Documentación de API | Swagger / OpenAPI (`Swashbuckle`) |
| Pruebas | xUnit, Moq, FluentAssertions |
| Arquitectura | Clean Architecture (Domain / Application / Infrastructure / WebApi) |

---

## Arquitectura

```
CourierMax.Domain          <- Entidades, Value Objects, Enums, reglas de
                               negocio puras (sin dependencias externas)
CourierMax.Application     <- Casos de uso (Services), DTOs, interfaces de
                               repositorio (puertos)
CourierMax.Infrastructure  <- EF Core, DbContext, repositorios concretos,
                               migraciones
CourierMax.WebApi          <- Controllers, middleware de errores,
                               configuración de DI, Swagger
```

**¿Por qué Clean Architecture?**
El dominio (reglas de negocio: cálculo de tarifas, SLA en días hábiles,
capacidad de vehículos, transiciones de estado) es el corazón del problema y
cambia con mucha menos frecuencia que la infraestructura. Separar `Domain` de
`Infrastructure` permite:
- Probar las reglas de negocio (`Vehicle.LoadCargo`, `SlaPolicy.IsOverdue`,
  `Shipment.Cancel`, etc.) con pruebas unitarias puras, sin base de datos.
- Cambiar el motor de persistencia (por ejemplo de EF Core a Dapper) sin
  tocar una sola línea de `Domain` o `Application`, porque `Application`
  solo conoce interfaces (`IShipmentRepository`, `IVehicleRepository`, etc.),
  no implementaciones concretas.
- Mantener el dominio expresivo: en vez de pasar `decimal`/`string` sueltos,
  se usan **Value Objects** (`Phone`, `Weight`, `Dimensions`, `Address`,
  `TrackingCode`) que validan sus propias invariantes en el constructor
  (RN-04), evitando que un envío inválido pueda existir en memoria.

**¿Por qué Entity Framework Core en vez de Dapper o memoria?**
El dominio tiene relaciones (Shipment↔Vehicle↔Driver, Shipment→StatusHistory
1:N) y reglas de integridad (código de rastreo único, capacidad en tiempo
real) que se benefician de un ORM con `Migrations` para versionar el esquema
y `HasConversion`/`OwnsOne` para mapear Value Objects sin exponerlos como
columnas planas. Dapper habría significado escribir y mantener SQL a mano
para cada query sin ganancia real, dado el tamaño del dominio.

**Servicios de dominio sin estado** (`CourierMax.Domain.Services`):
- `ColombianBusinessCalendar`: cálculo de días hábiles colombianos (RN-02),
  con los festivos de 2026 fijados en código porque el enunciado los define
  como una lista cerrada solo para ese año — no se modela como tabla porque
  no es un dato que cambie en tiempo de ejecución de esta prueba.
- `SlaPolicy`: determina si un envío está atrasado (RF-05), combinando el
  calendario anterior con el SLA por tipo de servicio.

**Manejo de errores centralizado**: `ExceptionHandlingMiddleware` traduce
excepciones de dominio/aplicación a respuestas HTTP consistentes
(`400` para validaciones y reglas de negocio violadas, `404` para recursos
no encontrados, `500` para errores no esperados), evitando duplicar
try/catch en cada controller.

---

## Requisitos previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server accesible (local, contenedor Docker, o remoto) con un usuario
  que pueda crear bases de datos

## Cómo ejecutar el proyecto localmente

1. Clona el repositorio.
2. Configura tu cadena de conexión en
   [`CourierMax/CourierMax.WebApi/appsettings.json`](CourierMax/CourierMax.WebApi/appsettings.json):

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=TU_SERVIDOR;Database=CourierMax;User ID=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;"
     }
   }
   ```

   > ⚠️ El valor que viene en el repositorio es el de un entorno local de
   > desarrollo y **no debe usarse en producción**. Para un entorno real, usa
   > variables de entorno o `dotnet user-secrets` en vez de texto plano.

3. Ejecuta la API. **No necesitas correr ningún script SQL a mano**: al
   arrancar, la aplicación aplica automáticamente todas las migraciones de
   EF Core (`Database.Migrate()` en `Program.cs`), que crean el esquema
   completo **y** los datos de referencia (ciudades, distancias, vehículos y
   conductores semilla).

   ```bash
   cd CourierMax/CourierMax.WebApi
   dotnet run
   ```

4. La consola te indicará la URL real (varía según tu `launchSettings.json`,
   típicamente `http://localhost:5171` o `https://localhost:7139`). Abre
   Swagger UI en `/swagger` sobre esa URL, por ejemplo:

   ```
   http://localhost:5171/swagger
   ```

### Si tu base de datos local ya tenía datos de pruebas manuales

Si la base ya existía con filas insertadas a mano (por ejemplo de pruebas
previas) que no coincidan con los IDs/datos semilla esperados, lo más simple
es recrearla antes de levantar la API:

```sql
ALTER DATABASE CourierMax SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE CourierMax;
```

Al volver a ejecutar `dotnet run`, EF Core la recreará desde cero.

## Ejecutar las pruebas

```bash
dotnet test
```

---

## Ejemplos de uso de la API

A continuación, un flujo mínimo de punta a punta. Para la guía completa con
**todos** los endpoints, casos de error esperados y un escenario detallado
para validar la capacidad de vehículos y el balanceo de carga, ver
**[docs/GUIA_API.md](docs/GUIA_API.md)**.
Para el detalle de cada tabla, columna y regla de negocio en base de datos,
ver **[docs/DICCIONARIO_DATOS.md](docs/DICCIONARIO_DATOS.md)**.

### 1. Crear un envío

```bash
curl -X POST http://localhost:5171/api/shipments \
  -H "Content-Type: application/json" \
  -d '{
    "senderName": "Juan Perez",
    "senderPhone": "3001234567",
    "senderAddress": "Calle 123",
    "recipientName": "Maria Lopez",
    "recipientPhone": "3102345678",
    "recipientAddress": "Carrera 456",
    "packageWeight": 5,
    "packageLength": 30,
    "packageWidth": 20,
    "packageHeight": 10,
    "packageType": "Fragil",
    "serviceType": "Express",
    "origin": "Bogotá",
    "destination": "Medellín"
  }'
```

Respuesta `201 Created`:
```json
{
  "id": 1,
  "trackingCode": "CM-XXXXXXXX",
  "status": "CREADO",
  "totalCost": null,
  "...": "..."
}
```

### 2. Consultar el costo estimado

```bash
curl http://localhost:5171/api/shipments/CM-XXXXXXXX/cost
```

### 3. Asignar a un conductor (con balanceo de carga automático si se omite `driverId`)

```bash
curl -X POST http://localhost:5171/api/shipments/1/assign \
  -H "Content-Type: application/json" \
  -d '{ "driverId": 3, "changedBy": "operador1" }'
```

### 4. Cambiar de estado

```bash
curl -X PATCH http://localhost:5171/api/shipments/1/status \
  -H "Content-Type: application/json" \
  -d '{ "newStatus": "EN_TRANSITO", "changedBy": "operador1" }'
```

### 5. Consultar envíos atrasados

```bash
curl "http://localhost:5171/api/shipments/overdue?from=2026-06-01T00:00:00Z&to=2026-06-30T00:00:00Z"
```

### 6. Reporte de eficiencia de un conductor

```bash
curl http://localhost:5171/api/drivers/3/efficiency-report
```

---

## Estructura del repositorio

```
CourierMax/                  Solución .NET (Domain/Application/Infrastructure/WebApi)
tests/CourierMax.Tests/       Pruebas unitarias (dominio, aplicación, controllers)
docs/                          Diccionario de datos y guía de la API
enunciado-prueba-dotnet-seniority.md   Enunciado original de la prueba
```
