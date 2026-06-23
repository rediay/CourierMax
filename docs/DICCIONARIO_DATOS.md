# Diccionario de Datos — CourierMax

Motor: SQL Server. Esquema generado y versionado por EF Core Migrations
(`CourierMax.Infrastructure/Migrations`). No se edita el esquema a mano.

---

## 1. Shipments

Tabla principal: representa un envío y su ciclo de vida completo.

| Columna | Tipo SQL | Nulo | Descripción |
|---|---|---|---|
| `Id` | `int` (PK, identity) | No | Identificador interno autogenerado. |
| `TrackingCode` | `nvarchar(20)` (único) | No | Código de rastreo público, formato `CM-XXXXXXXX` (8 dígitos). Generado automáticamente al crear el envío; el sistema garantiza que no se repita. |
| `SenderName` | `nvarchar(100)` | No | Nombre del remitente. |
| `SenderPhone` | `nvarchar(20)` | No | Teléfono del remitente. Se valida formato colombiano: 10 dígitos, inicia en 3 o 6. |
| `SenderAddress` | `nvarchar(200)` | No | Dirección de recogida. No puede estar vacía. |
| `RecipientName` | `nvarchar(100)` | No | Nombre del destinatario. |
| `RecipientPhone` | `nvarchar(20)` | No | Teléfono del destinatario (mismo formato que el remitente). |
| `RecipientAddress` | `nvarchar(200)` | No | Dirección de entrega. No puede estar vacía. |
| `PackageWeight` | `decimal(10,2)` | No | Peso del paquete en kg. Rango válido: 0.1 – 100 kg. |
| `PackageLength` | `decimal(10,2)` | No | Largo del paquete en cm. Rango válido: 1 – 200 cm. |
| `PackageWidth` | `decimal(10,2)` | No | Ancho del paquete en cm. Rango válido: 1 – 200 cm. |
| `PackageHeight` | `decimal(10,2)` | No | Alto del paquete en cm. Rango válido: 1 – 200 cm. |
| `PackageType` | `int` (enum) | No | Tipo de paquete. Ver tabla de enums §6. |
| `ServiceType` | `int` (enum) | No | Tipo de servicio. Ver tabla de enums §6. |
| `Origin` | `nvarchar(50)` | No | Ciudad de origen. Debe existir en la lista de ciudades de referencia (§5). |
| `Destination` | `nvarchar(50)` | No | Ciudad de destino. Debe existir en la lista de ciudades de referencia (§5). |
| `Status` | `int` (enum) | No | Estado actual del envío. Ver tabla de enums §6. |
| `VehicleId` | `int` (FK → `Vehicles.Id`) | Sí | Vehículo asignado. `NULL` mientras el envío esté en `CREADO`. Se limpia si el vehículo se elimina (`ON DELETE SET NULL`). |
| `DriverId` | `int` (FK → `Drivers.Id`) | Sí | Conductor asignado. Mismas reglas que `VehicleId`. |
| `TotalCost` | `decimal(18,2)` | Sí | Costo total calculado al momento de asignar (RF-04). `NULL` hasta que el envío se asigna. |
| `CreatedAt` | `datetime2` | No | Fecha/hora UTC de creación. Es la fecha base para calcular SLA y atrasos (RF-05). |
| `UpdatedAt` | `datetime2` | Sí | Fecha/hora UTC de la última transición de estado. |

**Índices:** único sobre `TrackingCode`; no-único sobre `VehicleId` y `DriverId`.

---

## 2. ShipmentStatusHistories

Bitácora de cada cambio de estado de un envío (RF-02). Un envío tiene muchos registros aquí (relación 1:N).

| Columna | Tipo SQL | Nulo | Descripción |
|---|---|---|---|
| `Id` | `int` (PK, identity) | No | Identificador del registro de historial. |
| `ShipmentId` | `int` (FK → `Shipments.Id`, cascada) | No | Envío al que pertenece este cambio. Si se borra el envío, se borra su historial. |
| `PreviousStatus` | `int` (enum) | Sí | Estado anterior. `NULL` solo en el primer registro (creación, estado inicial `CREADO`). |
| `NewStatus` | `int` (enum) | No | Estado al que transicionó. |
| `ChangedAt` | `datetime2` | No | Fecha/hora UTC exacta del cambio. Se usa para calcular tiempos de asignación→entrega (RF-06) y fecha de entrega real (RF-05). |
| `Reason` | `nvarchar(500)` | Sí | Motivo del cambio. Obligatorio (mínimo 5 caracteres) solo cuando `NewStatus = CANCELADO`. |
| `ChangedBy` | `nvarchar(100)` | No | Identificador de quién hizo el cambio (usuario/conductor/operador, o `"system"` para el registro de creación). |

**Índice:** no-único sobre `ShipmentId`.

---

## 3. Drivers

Conductores de la flota.

| Columna | Tipo SQL | Nulo | Descripción |
|---|---|---|---|
| `Id` | `int` (PK, identity) | No | Identificador del conductor. |
| `Name` | `nvarchar(100)` | No | Nombre completo. |
| `Phone` | `nvarchar(20)` | Sí | Teléfono de contacto (informativo, sin validación de formato). |
| `Email` | `nvarchar(100)` | Sí | Correo de contacto (informativo). |
| `IsActive` | `bit` | No | Si es `false`, el conductor no puede recibir nuevas asignaciones (RF-03). |
| `CreatedAt` | `datetime2` | No | Fecha de alta del conductor. |
| `UpdatedAt` | `datetime2` | Sí | Última modificación. |

**Relación:** 1:1 con `Vehicles` (un conductor tiene a lo sumo un vehículo, y viceversa).

**Datos semilla** (ver §5):

| Id | Nombre |
|---|---|
| 1 | Juan Pérez |
| 2 | María López |
| 3 | Carlos Ruiz |

---

## 4. Vehicles

Vehículos de la flota y su capacidad de carga.

| Columna | Tipo SQL | Nulo | Descripción |
|---|---|---|---|
| `Id` | `int` (PK, identity) | No | Identificador del vehículo. |
| `Plate` | `nvarchar(20)` (único) | No | Placa del vehículo. |
| `DriverId` | `int` (FK → `Drivers.Id`, único) | Sí | Conductor asignado a este vehículo (relación 1:1). |
| `MaxWeightKg` | `decimal(10,2)` | No | Capacidad máxima de peso, en kg. |
| `MaxVolumeM3` | `decimal(10,2)` | No | Capacidad máxima de volumen, en m³. |
| `CurrentWeightKg` | `decimal(10,2)` | No | Peso actualmente cargado (suma de los envíos asignados sin entregar/cancelar). Se actualiza en tiempo real al asignar/cancelar (RN-01). |
| `CurrentVolumeM3` | `decimal(10,2)` | No | Volumen actualmente cargado. Misma lógica que `CurrentWeightKg`. |
| `CreatedAt` | `datetime2` | No | Fecha de alta del vehículo. |
| `UpdatedAt` | `datetime2` | Sí | Última vez que cambió su carga actual. |

**Datos semilla** (ver §5):

| Id | Placa | Conductor | MaxWeightKg | MaxVolumeM3 |
|---|---|---|---|---|
| 1 | ABC-123 | Juan Pérez (1) | 500 | 10 |
| 2 | DEF-456 | María López (2) | 300 | 6 |
| 3 | GHI-789 | Carlos Ruiz (3) | 800 | 15 |

---

## 5. CityDistances

Tabla de referencia de rutas habilitadas entre ciudades, con su tarifa de distancia fija (RF-04). También define implícitamente la **lista de ciudades válidas del sistema** (RN-04): solo se acepta como `Origin`/`Destination` de un envío una ciudad que aparezca en esta tabla.

| Columna | Tipo SQL | Nulo | Descripción |
|---|---|---|---|
| `Id` | `int` (PK, identity) | No | Identificador de la ruta. |
| `Origin` | `nvarchar(50)` | No | Ciudad de origen de la ruta. |
| `Destination` | `nvarchar(50)` | No | Ciudad de destino de la ruta. |
| `DistanceKm` | `decimal(10,2)` | No | Distancia en kilómetros (informativo). |
| `DistanceFee` | `decimal(18,2)` | No | Recargo fijo por distancia, en pesos, usado en el cálculo de tarifa (RF-04). |

**Índice:** único sobre `(Origin, Destination)`.

> La búsqueda de ruta es **bidireccional**: una fila `Bogotá → Medellín` también sirve para calcular un envío `Medellín → Bogotá` con la misma tarifa.

**Datos semilla:**

| Id | Origen | Destino | Distancia (km) | Tarifa distancia |
|---|---|---|---|---|
| 1 | Bogotá | Medellín | 480 | $12.000 |
| 2 | Bogotá | Cali | 360 | $9.000 |
| 3 | Bogotá | Barranquilla | 950 | $20.000 |
| 4 | Medellín | Cali | 310 | $8.000 |
| 5 | Medellín | Barranquilla | 650 | $15.000 |
| 6 | Cali | Barranquilla | 900 | $18.000 |

**Ciudades válidas del sistema** (derivadas de esta tabla): `Bogotá`, `Medellín`, `Cali`, `Barranquilla`.

---

## 6. Enumerados (almacenados como `int` en la base de datos)

### PackageType (tipo de paquete)
| Valor | Nombre | Recargo sobre tarifa (RF-04) |
|---|---|---|
| 0 | Documento | Sin recargo |
| 1 | Paquete | Sin recargo |
| 2 | Fragil | +30% |
| 3 | Perecedero | +25% |

### ServiceType (tipo de servicio)
| Valor | Nombre | Tarifa base | SLA (días hábiles) |
|---|---|---|---|
| 0 | Estandar | $8.000 | 5 |
| 1 | Express | $15.000 | 2 |
| 2 | MismoDia | $25.000 | 0 (mismo día) |

### ShipmentStatus (estado del envío)
| Valor | Nombre | Descripción |
|---|---|---|
| 0 | CREADO | Estado inicial al registrar el envío. |
| 1 | ASIGNADO | Se asignó vehículo/conductor y se calculó `TotalCost`. |
| 2 | EN_TRANSITO | El envío está en camino. |
| 3 | ENTREGADO | Estado final exitoso. No admite más transiciones. |
| 4 | CANCELADO | Estado final. Se puede llegar desde cualquier estado excepto `ENTREGADO`. |

Transiciones válidas: `CREADO → ASIGNADO → EN_TRANSITO → ENTREGADO`, y `CANCELADO` desde cualquiera de los tres primeros.

---

## 7. Reglas de negocio que dependen de varias tablas

| Regla | Tablas involucradas | Resumen |
|---|---|---|
| RN-01 — Capacidad de vehículo | `Vehicles`, `Shipments` | `CurrentWeightKg/CurrentVolumeM3` de `Vehicles` se incrementan al asignar (`Shipments.VehicleId` no nulo) y se liberan al cancelar. |
| RN-02 — Días hábiles | `Shipments.CreatedAt`, `ShipmentStatusHistories.ChangedAt` | El SLA se cuenta en días hábiles colombianos (excluye sábados, domingos y los 12 festivos de 2026 definidos en código, no en base de datos). |
| RN-03 — Cancelación | `Shipments`, `ShipmentStatusHistories`, `Vehicles` | Cancelar exige `Reason` ≥ 5 caracteres y libera la capacidad del vehículo si había uno asignado. |
| RN-04 — Validación de ciudades | `CityDistances`, `Shipments` | `Origin`/`Destination` deben existir como ciudad de referencia. |
| RN-05 — Código único | `Shipments.TrackingCode` | Se valida unicidad contra la tabla antes de persistir; se regenera automáticamente si hay colisión. |

---

## 8. Notas importantes

- **No hay tabla de usuarios/autenticación.** El campo `ChangedBy` es un texto libre que identifica quién hizo el cambio; no hay validación contra una tabla de usuarios.
- **Los Value Objects del dominio** (`Phone`, `Address`, `Weight`, `Dimensions`, `TrackingCode`) se validan en código C# antes de persistirse; EF Core los convierte a columnas planas (`HasConversion`) por lo que en la base de datos solo se ven tipos primitivos.
- **Los festivos colombianos están hardcodeados en código** (`CourierMax.Domain.Services.ColombianBusinessCalendar`), no en una tabla, porque el enunciado los fija explícitamente solo para 2026.
