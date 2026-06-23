# Guía de uso de la API — CourierMax

Todos los ejemplos están probados contra la API levantada localmente
(`https://localhost:7139` o el puerto que asigne tu `launchSettings.json`;
en los ejemplos se usa `http://localhost:5299` como referencia — ajusta el
puerto al que te muestre la consola al ejecutar `dotnet run`).

Puedes ejecutar cada paso en **Swagger UI** (`/swagger`), Postman, o `curl`.

**Datos semilla disponibles desde el primer arranque** (ver diccionario de datos):
- Ciudades válidas: `Bogotá`, `Medellín`, `Cali`, `Barranquilla`
- Conductores: `1=Juan Pérez (ABC-123, 500kg/10m³)`, `2=María López (DEF-456, 300kg/6m³)`, `3=Carlos Ruiz (GHI-789, 800kg/15m³)`

> ⚠️ Si tu base de datos local tiene datos manuales viejos de pruebas anteriores
> con IDs distintos a los de arriba, bórrala y vuelve a levantar la API para
> que el seed se aplique limpio (`DROP DATABASE CourierMax;` y `dotnet run`).

---

## 1. Crear un envío — RF-01

**`POST /api/shipments`**

```json
{
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
}
```

**Respuesta esperada — `201 Created`:**
```json
{
  "id": 1,
  "trackingCode": "CM-XXXXXXXX",
  "senderName": "Juan Perez",
  "status": "CREADO",
  "vehicleId": null,
  "driverId": null,
  "totalCost": null,
  "createdAt": "2026-06-23T..."
}
```
Guarda el `id` y el `trackingCode` — los usarás en los siguientes pasos.

**Valores válidos:**
- `packageType`: `Documento`, `Paquete`, `Fragil`, `Perecedero`
- `serviceType`: `Estandar`, `Express`, `MismoDia`
- `origin`/`destination`: deben ser una de las 4 ciudades de referencia

**Errores esperados (para validar que las reglas funcionan):**

| Caso | Cambio en el body | Respuesta |
|---|---|---|
| Ciudad inválida | `"origin": "Quibdó"` | `400` → `{ "error": "Origin city 'Quibdó' is not a valid reference city." }` |
| Peso fuera de rango | `"packageWeight": 150` | `400` con detalle de validación (`Weight must be between 0.1 and 100 kg`) |
| Teléfono inválido | `"senderPhone": "123"` | `400` (`Phone number must have 10 digits...`) |
| Dimensión fuera de rango | `"packageLength": 300` | `400` (`Length must be between 1 and 200 cm`) |

---

## 2. Consultar un envío por código de rastreo — RF-01/RF-02

**`GET /api/shipments/{trackingCode}`**

Ejemplo: `GET /api/shipments/CM-12345678`

**Respuesta esperada — `200 OK`:** el mismo objeto del paso 1, con el estado actual.

**Si no existe — `404 Not Found`:**
```json
{ "error": "Shipment with tracking code 'CM-99999999' not found" }
```

---

## 3. Consultar el costo estimado — RF-04

**`GET /api/shipments/{trackingCode}/cost`**

Usando el envío del paso 1 (frágil, express, 5kg, Bogotá→Medellín):

**Respuesta esperada — `200 OK`:**
```json
{
  "origin": "Bogotá",
  "destination": "Medellín",
  "distanceKm": 480,
  "baseFee": 15000,
  "weightSurcharge": 4500,
  "distanceFee": 12000,
  "packageSurcharge": 9450,
  "totalCost": 40950
}
```
Cálculo: base express $15.000 + (5-2)kg×$1.500=$4.500 + distancia $12.000 = $31.500; recargo frágil 30% = $9.450 → total $40.950.

---

## 4. Asignar a un conductor específico — RF-03

**`POST /api/shipments/{id}/assign`** (usa el `id` numérico, no el tracking code)

```json
{ "driverId": 3, "changedBy": "operador1" }
```

**Respuesta esperada — `200 OK`:**
```json
{
  "id": 1,
  "status": "ASIGNADO",
  "vehicleId": 3,
  "driverId": 3,
  "totalCost": 40950,
  ...
}
```

**Errores esperados:**

| Caso | Body | Respuesta |
|---|---|---|
| Conductor inactivo | conductor marcado `IsActive=false` | `400` → `{ "error": "Driver '...' is not active..." }` |
| Conductor inexistente | `"driverId": 999` | `404` |
| Excede capacidad | ver sección 5 más abajo | `400` |
| Envío ya asignado | repetir el mismo `assign` dos veces | `400` (`Cannot assign shipment in status ASIGNADO...`) |

---

## 5. Validar capacidad acumulada del vehículo — RN-01

Como el peso máximo permitido por envío es 100kg, para forzar el rechazo por
capacidad debes **acumular varios envíos sobre el mismo vehículo**, no uno solo.

Usa el vehículo de María López (`driverId=2`, `DEF-456`, máx. **300kg/6m³**):

1. Crea 3 envíos de 80kg cada uno (payload del paso 1, cambiando `packageWeight` a `80`)
2. Asigna los 3 al `driverId: 2` → los tres deben dar `200 OK` (carga acumulada: 80→160→240kg)
3. Crea un 4º envío de 80kg y asígnalo también al `driverId: 2`

**Respuesta esperada en el 4º — `400 Bad Request`:**
```json
{ "error": "Vehicle DEF-456 does not have enough capacity for this shipment (weight: 80,00kg, volume: 0,1250m3)." }
```
(240kg + 80kg = 320kg > 300kg de capacidad máxima)

---

## 6. Balanceo de carga automático — RN-01

**`POST /api/shipments/{id}/assign`**, **omitiendo** `driverId` por completo:

```json
{ "changedBy": "operador1" }
```

> ⚠️ Si pruebas esto en Swagger UI y el formulario te genera `"driverId": ""`
> (string vacío), bórralo del JSON antes de ejecutar — debe quedar omitido o
> en `null`, nunca como cadena vacía, o dará `400` por error de conversión.

**Respuesta esperada — `200 OK`:** el sistema elige automáticamente, entre los
conductores activos cuyo vehículo tenga capacidad suficiente, el que tenga
**menor carga actual** (`CurrentWeightKg + CurrentVolumeM3` más bajo).

---

## 7. Transicionar estados — RF-02

**`PATCH /api/shipments/{id}/status`**

De `ASIGNADO` a `EN_TRANSITO`:
```json
{ "newStatus": "EN_TRANSITO", "changedBy": "operador1" }
```
**`200 OK`**, `"status": "EN_TRANSITO"`.

De `EN_TRANSITO` a `ENTREGADO`:
```json
{ "newStatus": "ENTREGADO", "changedBy": "operador1" }
```
**`200 OK`**, `"status": "ENTREGADO"`.

**Errores esperados:**

| Caso | Body | Respuesta |
|---|---|---|
| Saltar etapas | pasar de `CREADO` directo a `ENTREGADO` | `400` (`Cannot deliver shipment in status CREADO...`) |
| Asignar por esta vía | `"newStatus": "ASIGNADO"` | `400` (`Use the assign endpoint...`) |
| Estado inválido | `"newStatus": "EN_CAMINO"` | `400` (`'EN_CAMINO' is not a valid shipment status.`) |

---

## 8. Cancelar un envío — RN-03

**`PATCH /api/shipments/{id}/status`**

Motivo muy corto (< 5 caracteres):
```json
{ "newStatus": "CANCELADO", "reason": "no", "changedBy": "operador1" }
```
**`400 Bad Request`** (`Cancellation reason must be at least 5 characters.`)

Motivo válido, sobre un envío `ASIGNADO` o `EN_TRANSITO`:
```json
{ "newStatus": "CANCELADO", "reason": "Cliente canceló el pedido", "changedBy": "operador1" }
```
**`200 OK`**, `"status": "CANCELADO"` — y si tenía vehículo asignado, su capacidad se libera automáticamente (puedes comprobarlo reintentando el escenario de la sección 5: tras cancelar uno de los 3 envíos de 80kg, el 4º que antes fallaba ahora debería poder asignarse).

Sobre un envío ya `ENTREGADO`:
**`400 Bad Request`** → `{ "error": "Cannot cancel a delivered shipment." }`

---

## 9. Ver historial de estados de un envío — RF-02

**`GET /api/shipments/{id}/history`**

**Respuesta esperada — `200 OK`:**
```json
[
  { "id": 1, "previousStatus": null, "newStatus": "CREADO", "changedAt": "...", "reason": null, "changedBy": "system" },
  { "id": 2, "previousStatus": "CREADO", "newStatus": "ASIGNADO", "changedAt": "...", "reason": null, "changedBy": "operador1" },
  { "id": 3, "previousStatus": "ASIGNADO", "newStatus": "EN_TRANSITO", "changedAt": "...", "reason": null, "changedBy": "operador1" }
]
```

---

## 10. Consultar envíos atrasados — RF-05

**`GET /api/shipments/overdue?from={fecha}&to={fecha}`**

```
GET /api/shipments/overdue?from=2026-06-01T00:00:00Z&to=2026-06-30T00:00:00Z
```

**Respuesta esperada — `200 OK`:** lista de envíos creados en ese rango que ya
superaron su SLA en días hábiles sin llegar a `ENTREGADO` (o que se entregaron
fuera de tiempo). Devuelve `[]` si no hay ninguno — lo cual es normal para
envíos creados hoy, porque el SLA se cuenta en días hábiles **transcurridos**,
no en el día de creación.

> Para forzar un resultado sin esperar varios días reales, puedes crear un
> envío `MismoDia` y pedir que se le retroceda manualmente el `CreatedAt` en
> la base de datos (avísame y lo hacemos juntos, confirmando antes el `id`
> exacto a modificar).

**Validación de parámetros:**
| Caso | Query | Respuesta |
|---|---|---|
| `to` antes que `from` | `?from=2026-06-20&to=2026-06-01` | `400` → `{ "error": "'to' must be greater than or equal to 'from'." }` |

---

## 11. Reporte de eficiencia por conductor — RF-06

**`GET /api/drivers/{id}/efficiency-report`**

Ejemplo: `GET /api/drivers/3/efficiency-report`

**Respuesta esperada — `200 OK`:**
```json
{
  "driverId": 3,
  "driverName": "Carlos Ruiz",
  "totalAssigned": 1,
  "totalDelivered": 1,
  "totalCancelled": 0,
  "totalInTransit": 0,
  "averageDeliveryDays": 0.0,
  "onTimeDeliveryPercentage": 100.0,
  "totalWeightTransportedKg": 5.0
}
```

- `averageDeliveryDays`: promedio en días (con decimales) entre la fecha de
  asignación (`ASIGNADO`) y la de entrega (`ENTREGADO`), solo sobre envíos
  ya entregados.
- `onTimeDeliveryPercentage`: % de envíos entregados que NO superaron su SLA.
- `totalWeightTransportedKg`: suma de peso de envíos `ASIGNADO` + `EN_TRANSITO`
  + `ENTREGADO` (excluye `CANCELADO`).

**Conductor inexistente:**
```
GET /api/drivers/999/efficiency-report
```
**`404 Not Found`**

---

## 12. Orden recomendado para una prueba completa de punta a punta

1. `POST /api/shipments` → crea el envío
2. `GET /api/shipments/{trackingCode}/cost` → verifica la tarifa antes de asignar
3. `POST /api/shipments/{id}/assign` → asigna (con o sin `driverId`)
4. `GET /api/shipments/{trackingCode}` → confirma `status: ASIGNADO` y `totalCost`
5. `PATCH /api/shipments/{id}/status` con `EN_TRANSITO`
6. `PATCH /api/shipments/{id}/status` con `ENTREGADO`
7. `GET /api/shipments/{id}/history` → confirma la bitácora completa
8. `GET /api/drivers/{driverId}/efficiency-report` → confirma que el envío entregado se refleja en las métricas

---

## 13. Nota sobre el depurador de Visual Studio

Si ejecutas la API con `F5` desde Visual Studio y cada error de validación te
"salta" al depurador en vez de mostrarte solo la respuesta JSON: eso es el
comportamiento por defecto de VS de detenerse en toda excepción lanzada
(`first-chance exception`), aunque el middleware ya la capture y la convierta
en una respuesta 400/404 limpia. Para que no te interrumpa:

**Debug → Windows → Exception Settings** (`Ctrl+Alt+E`) → desmarca
**"Common Language Runtime Exceptions"**.

Con eso, el depurador solo se detendrá ante errores realmente no controlados.
