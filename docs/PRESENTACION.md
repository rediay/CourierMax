# Guía de presentación — CourierMax

Documento de preparación para defender el proyecto ante un líder técnico o
arquitecto. No es documentación del producto (eso ya está en el `README.md`
y en `docs/`) — es una guía para que **tú** domines el "por qué" detrás de
cada decisión y no te tomen por sorpresa.

---

## 1. El pitch de 30 segundos

> "CourierMax es una API REST que modela el ciclo de vida completo de un
> envío de courier: creación, asignación a vehículo/conductor con
> validación de capacidad en tiempo real, transición de estados, cálculo de
> tarifas, alertas de SLA en días hábiles, y métricas de eficiencia por
> conductor. Está construida con .NET 9 y Clean Architecture: el dominio
> (las reglas de negocio) no depende de ningún detalle técnico —ni de EF
> Core, ni de SQL Server, ni de ASP.NET Core—, lo cual permite probarlo con
> tests unitarios puros y cambiar infraestructura sin tocar la lógica de
> negocio."

Si solo recuerdas una frase de toda esta guía, que sea esa.

---

## 2. La arquitectura: qué hay y por qué

```
CourierMax.Domain          (el centro — no depende de nada)
   ↑
CourierMax.Application     (casos de uso — depende solo de Domain)
   ↑
CourierMax.Infrastructure   (EF Core, SQL Server — implementa lo que Application pide)
CourierMax.WebApi           (Controllers — orquesta Application e Infrastructure)
```

**La regla de oro que debes poder explicar:** las flechas de dependencia
apuntan **hacia adentro**, hacia el Dominio. `Domain` no sabe que existe EF
Core ni SQL Server. `Application` solo conoce **interfaces**
(`IShipmentRepository`, `IVehicleRepository`...), nunca implementaciones
concretas. Quien sí conoce EF Core es `Infrastructure`, que implementa esas
interfaces.

**¿Por qué esto importa y no es solo "porque así se hace"?**
- Puedo probar `Vehicle.LoadCargo()`, `SlaPolicy.IsOverdue()`,
  `Shipment.Cancel()` con tests unitarios **sin base de datos**, sin mocks
  de EF Core, sin levantar nada. Son clases de C# normales.
- Si mañana la empresa decide migrar de SQL Server a PostgreSQL, o de EF
  Core a Dapper, **no se toca una sola línea de `Domain` ni `Application`**
  — solo se reescribe `Infrastructure`.
- El dominio es expresivo: en vez de `decimal peso`, `string telefono`
  sueltos por todo el código, existen **Value Objects** (`Weight`, `Phone`,
  `Address`, `Dimensions`, `TrackingCode`) que validan sus propias reglas en
  el constructor. Es físicamente imposible construir un `Phone` con 9
  dígitos o un `Weight` de 500kg — el objeto simplemente no existe si la
  regla se viola. Eso es "hacer ilegal los estados inválidos" en vez de
  validar por todos lados con `if`.

**Si te preguntan "¿por qué Clean Architecture y no algo más simple?":**
> "El dominio tiene bastante lógica real: cálculo de tarifas con varias
> reglas combinadas, SLA en días hábiles colombianos, capacidad de
> vehículos en tiempo real, máquina de estados de envío. Esa lógica vale la
> pena aislarla y protegerla con tests puros. Para un CRUD sin reglas de
> negocio, habría sido sobre-ingeniería; aquí se justifica."

---

## 3. Las 5 decisiones de diseño que debes poder defender

### 3.1 — EF Core en vez de Dapper o memoria
El enunciado daba libertad total. Elegí EF Core porque el dominio tiene
relaciones reales (Shipment↔Vehicle↔Driver, Shipment→1:N→StatusHistory) y
necesitaba versionar el esquema (Migrations). Dapper habría significado
escribir y mantener SQL a mano sin ganancia real para este tamaño de
dominio. *Trade-off honesto:* EF Core agrega una capa de abstracción y
"magia" (change tracking, lazy loading) que en sistemas de muy alto
rendimiento puede ser un costo — aquí no es el caso.

### 3.2 — Value Objects en el dominio
`Phone`, `Weight`, `Dimensions`, `Address`, `TrackingCode` son `record`
inmutables que validan en el constructor. Evitan duplicar `if (peso < 0.1
|| peso > 100)` en cinco lugares distintos del código. EF Core los mapea a
columnas planas con `HasConversion`/`OwnsOne` — en la base de datos no se
ven como objetos, solo en C#.

### 3.3 — Repository pattern + interfaces en Domain
Cada repositorio (`IShipmentRepository`, `IVehicleRepository`...) es una
interfaz definida en `Domain.Interfaces` e implementada en
`Infrastructure.Repositories`. Esto es lo que permite testear
`ShipmentService` con **Moq** sin tocar SQL Server real.

### 3.4 — Servicios de dominio sin estado para reglas transversales
`ColombianBusinessCalendar` y `SlaPolicy` son clases estáticas porque no
tienen estado propio — son funciones puras de cálculo (dado un calendario
fijo y una fecha, ¿es día hábil?). No hay razón para inyectarlas como
dependencias ni para que tengan estado.

### 3.5 — Excepciones de dominio mapeadas a códigos HTTP específicos
En vez de que cada controller decida códigos de estado, las reglas de
negocio lanzan excepciones tipadas (`ArgumentException`,
`ShipmentStateConflictException`, `KeyNotFoundException`) y un único
middleware (`ExceptionHandlingMiddleware`) las traduce a 400/404/409/500.
Si necesito agregar una nueva regla de negocio, no toco el middleware ni
los controllers — solo decido qué excepción lanzar.

---

## 4. Recorrido por los requerimientos (qué hace cada uno y dónde vive)

| Requerimiento | Qué resuelve | Dónde está |
|---|---|---|
| RF-01 Creación | Valida ciudad, teléfono, peso, dimensiones; genera tracking code único | `ShipmentService.CreateAsync`, Value Objects en `Domain` |
| RF-02 Estados | Máquina de estados `CREADO→ASIGNADO→EN_TRANSITO→ENTREGADO`, `CANCELADO` desde cualquiera salvo entregado | `Shipment.Assign/MarkInTransit/Deliver/Cancel` |
| RF-03 Asignación | Verifica capacidad de peso/volumen, conductor activo, balanceo de carga automático (RN-01) | `ShipmentService.AssignAsync`, `Vehicle.HasCapacityFor/LoadCargo` |
| RF-04 Tarifas | Tarifa fija + recargo peso + distancia + % por tipo de paquete | `CostCalculationService.CalculateAsync` |
| RF-05 SLA | Días hábiles colombianos, consulta de atrasados por rango | `ColombianBusinessCalendar`, `SlaPolicy`, endpoint `/shipments/overdue` |
| RF-06 Métricas | Reporte por conductor: entregados, cancelados, tiempo promedio, % SLA, peso total | `DriverMetricsService.GetEfficiencyReportAsync` |

**Ejemplo concreto que puedes recitar de memoria** (el del enunciado):
paquete frágil, 5kg, express, Bogotá→Medellín:
```
Base express:        $15.000
Peso extra (5-2)kg:   $4.500  (3 × $1.500)
Distancia:           $12.000
Subtotal:            $31.500
Recargo frágil 30%:   $9.450
TOTAL:               $40.950
```

---

## 5. Las preguntas difíciles que probablemente te hagan (con respuesta honesta)

**"¿Qué pasa si dos requests intentan asignar el mismo vehículo al mismo
tiempo? ¿Puede sobrecargarse por una condición de carrera?"**
> Sí, es una limitación real que tengo identificada. Hoy el flujo es
> leer-capacidad → validar → guardar, sin un token de concurrencia
> optimista (`RowVersion`) en `Vehicle` ni una transacción serializable. Bajo
> alta concurrencia, dos asignaciones simultáneas podrían pasar la
> validación antes de que cualquiera persista. La solución sería agregar
> una columna de concurrencia en `Vehicle` (EF Core la soporta nativamente
> con `[Timestamp]` o `IsRowVersion()`) para que la segunda escritura falle
> con `DbUpdateConcurrencyException` y se reintente.

**"Veo que `AssignAsync` hace `UpdateAsync(shipment)` y luego
`UpdateAsync(vehicle)` por separado. ¿Eso es atómico?"**
> No completamente — son dos `SaveChangesAsync` independientes sobre el
> mismo `DbContext`. Si el proceso falla entre ambas, podría quedar el
> envío asignado sin la capacidad reservada, o viceversa. La forma correcta
> sería envolver ambas operaciones en una transacción explícita
> (`IDbContextTransaction`) o aplicar un patrón Unit of Work que comparta
> una sola operación de guardado. No lo prioricé porque el alcance de 3
> días pedía profundidad en reglas de negocio antes que en infraestructura
> transaccional, pero es lo primero que añadiría con más tiempo.

**"¿Por qué los festivos colombianos están en código y no en una tabla?"**
> Porque el enunciado los fija como una lista cerrada solo para 2026. Si el
> requerimiento fuera "soportar festivos de cualquier año de forma
> configurable", ahí sí justificaría una tabla y un endpoint de
> administración. Meter eso ahora habría sido sobre-ingeniería para un
> dato que no cambia en el alcance de esta prueba.

**"No veo autenticación ni autorización. ¿Cualquiera puede pegarle a la
API?"**
> Correcto, no hay capa de seguridad — el enunciado no la pedía y el
> alcance de 3 días priorizó reglas de negocio. En un sistema real
> agregaría JWT/OAuth2 con roles (operador, conductor, admin) y limitaría,
> por ejemplo, quién puede cancelar un envío o ver el reporte de otro
> conductor.

**"¿Cómo decides 400 vs 409 vs 404?"**
> 404 es "el recurso no existe" (`KeyNotFoundException`). 409 es "el
> recurso existe, pero la operación entra en conflicto con su estado
> actual" — por ejemplo, asignar un envío que ya está `ASIGNADO`, o
> cancelar uno ya `ENTREGADO` (`ShipmentStateConflictException`). 400 es
> todo lo demás: input inválido o una regla de negocio que rechaza la
> operación sin que sea, estrictamente, un conflicto de estado (capacidad
> de vehículo excedida, conductor inactivo, ciudad inválida).

**"¿Por qué no usaste Dapper si la prueba dice que se valora la decisión
arquitectónica?"**
> Evalué ambos. Dapper da más control sobre el SQL y menos "magia", pero
> aquí hay relaciones (1:1, 1:N) y necesitaba migraciones versionadas para
> demostrar evolución del esquema. El tamaño del dominio no justificaba
> escribir y mantener SQL a mano para cada query.

**"¿Cómo sabes que el cálculo de días hábiles es correcto?"**
> Tengo tests unitarios que verifican el ejemplo exacto del enunciado:
> envío creado un viernes con SLA de 1 día hábil debe vencer el lunes, no
> contando sábado ni domingo, y otro test que verifica que un festivo
> colombiano (29 de junio) no cuenta como día hábil aunque caiga en lunes.

**"¿Qué tests tienes y qué cubren?"**
> 104 tests: dominio puro (Value Objects, máquina de estados de `Shipment`,
> capacidad de `Vehicle`, calendario de días hábiles, política de SLA),
> servicios de aplicación con mocks de los repositorios (incluyendo casos
> de error: capacidad excedida, conductor inactivo, conflictos de estado),
> y controllers con el servicio mockeado.

---

## 6. Guion de demo en vivo (si te piden mostrarlo funcionando)

Orden recomendado, con los comandos exactos (ya validados en sesiones
anteriores — ver `docs/GUIA_API.md` para el detalle completo):

1. **Mostrar Swagger** (`/swagger`) — da una vista general de todos los
   endpoints sin tener que explicar cada uno de memoria.
2. **Crear un envío** frágil/express Bogotá→Medellín, 5kg → mostrar
   `201` con `trackingCode` y estado `CREADO`.
3. **Consultar el costo** (`GET /shipments/{code}/cost`) → mostrar que da
   exactamente $40.950, y explicar el desglose en vivo.
4. **Asignar sin `driverId`** → explicar que el sistema elige
   automáticamente el vehículo con menor carga actual (RN-01).
5. **Forzar un 409**: intentar asignar el mismo envío otra vez → mostrar
   el `409 Conflict` y explicar por qué no es un 400.
6. **Cambiar de estado** hasta `ENTREGADO`.
7. **Mostrar el reporte de conductor** (`GET /drivers/{id}/efficiency-report`)
   con el envío recién entregado reflejado en las métricas.
8. **Mostrar la consola** con los logs de cada paso anterior — demuestra
   que no es solo "que funcione", sino que queda trazabilidad.

---

## 7. Lo que dirías si te preguntan "¿qué harías diferente con más tiempo?"

Tener 2-3 respuestas listas demuestra criterio, no debilidad:
1. Token de concurrencia optimista en `Vehicle` + transacción explícita en
   `AssignAsync` (ver sección 5).
2. Autenticación/autorización con roles.
3. Mover los festivos colombianos a una tabla si el requerimiento creciera
   a "multi-año configurable".
4. Tests de integración contra una base de datos real (in-memory SQLite o
   Testcontainers) además de los unitarios con mocks, para cubrir las
   configuraciones de EF Core (`HasConversion`, `OwnsOne`) que hoy solo se
   validan manualmente.

---

## 8. Tu chuleta de una sola línea por capa (para no trabarte)

- **Domain**: "las reglas, sin dependencias externas"
- **Application**: "los casos de uso, orquesta el dominio vía interfaces"
- **Infrastructure**: "el cómo persistimos, EF Core implementando las interfaces"
- **WebApi**: "la puerta de entrada HTTP, traduce excepciones a códigos de estado"
