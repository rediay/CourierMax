Prueba Técnica — Desarrollador .NET
Core
Fecha: 2026-06-10 Tecnología: .NET 8+ (ASP.NET Core Web API) Plazo de
entrega: 3 días
Contexto del Problema
CourierMax es una empresa de courier en expansión que está modernizando su sistema
de gestión de envíos. Actualmente, todos los procesos se manejan en hojas de cálculo y
Excel, lo que genera errores de asignación, entregas atrasadas y pérdida de visibilidad
sobre el rendimiento de conductores y vehículos.
La gerencia ha decidido desarrollar una API REST que sirva como backbone del futuro
sistema de logística. La API debe permitir gestionar el ciclo completo de un envío: desde
la creación y asignación a un vehículo/conductor, hasta el seguimiento de estados y la
generación de métricas de eficiencia.
Importante: Esta es una prueba de habilidades técnicas, no un producto comercial. Se
valora la calidad del diseño, la toma de decisiones arquitectónicas y la comprensión de
principios de ingeniería de software.
Requerimientos Funcionales
RF-01: Creación de Envío
El sistema debe permitir registrar un nuevo envío con la siguiente información:
Remitente: nombre, teléfono, dirección de recogida
Destinatario: nombre, teléfono, dirección de entrega

Paquete: peso (kg), dimensiones (largo x ancho x alto en cm), tipo (documento,
paquete, frágil, perecedero)
Tipo de servicio: estándar (3-5 días), express (1-2 días), mismo día
Origen y destino: ciudades válidas del sistema (ver Datos de Referencia)
Al crear un envío, el sistema debe asignarle un código de rastreo único (formato: CM-
XXXXXXXX , 8 dígitos) y estado inicial CREADO .
RF-02: Seguimiento de Estados
Un envío debe poder transitar por los siguientes estados:
CREADO → ASIGNADO → EN_TRANSITO → ENTREGADO
↓
CANCELADO ← (desde cualquier estado, excepto ENTREGADO)
Cada cambio de estado debe registrar:
Estado anterior y nuevo
Fecha/hora del cambio
Motivo del cambio (obligatorio para CANCELADO)
Quién realizó el cambio (ID de usuario/conductor)
RF-03: Asignación a Vehículo/Conductor
Un vehículo tiene una capacidad máxima en peso (kg) y volumen (m³)
Un conductor está asignado a un vehículo (relación 1:1)
Al asignar un envío a un conductor, el sistema debe verificar que el vehículo no
exceda su capacidad
Si excede capacidad, la asignación debe rechazarse con error claro
Un envío solo puede asignarse a un conductor activo
RF-04: Cálculo de Tarifas
El costo del envío se calcula con la siguiente lógica:
Tarifa base por tipo de servicio:
Estándar: $8,000

Express: $15,000
Mismo día: $25,000
Recargo por peso: $1,500 por kg adicional a los primeros 2 kg
Recargo por distancia: Se define entre pares de ciudades (ver Datos de
Referencia)
Recargo por tipo de paquete:
Frágil: +30%
Perecedero: +25%
Documento/Paquete: sin recargo
Ejemplo: Un paquete frágil de 5 kg, servicio express, entre Bogotá y Medellín:
Base express: $15,000
Peso extra: 3 kg × $1,500 = $4,500
Distancia Bogotá-Medellín: $12,000
Recargo frágil: ($15,000 + $4,500 + $12,000) × 30% = $9,450
Total: $40,950
RF-05: Alertas de Entrega Atrasada (SLA)
Cada tipo de servicio tiene un SLA en días hábiles:
Estándar: 5 días hábiles
Express: 2 días hábiles
Mismo día: 0 días hábiles (debe entregarse el mismo día)
Un envío se considera atrasado si han pasado más días hábiles desde su creación sin
llegar a ENTREGADO .
El sistema debe permitir consultar envíos atrasados por rango de fechas.
RF-06: Reporte de Métricas de Eficiencia
El sistema debe permitir generar un reporte por conductor con:
Total de envíos asignados
Total entregados vs. cancelados vs. en tránsito
Tiempo promedio de entrega (días desde asignación hasta entrega)

Porcentaje de entregas dentro del SLA
Peso total transportado
Reglas de Negocio
RN-01: Capacidad de Vehículo
Un vehículo NO puede exceder su capacidad máxima de peso ni de volumen
La capacidad se calcula en tiempo real al momento de asignar un envío
Si hay múltiples vehículos disponibles, seleccionar el que tenga menor carga actual
(balanceo de carga)
RN-02: Días Hábiles
No se cuentan sábados, domingos ni festivos colombianos
Festivos colombianos 2026: 1 Ene, 26 Ene, 30 Ene, 24 Mar, 1 May, 1 Jun, 29 Jun, 20
Jul, 17 Ago, 20 Oct, 9 Nov, 8 Dic
Si un envío se crea un viernes y tiene SLA de 1 día hábil, debe entregarse el lunes (no
cuenta sábado ni domingo)
RN-03: Cancelación
Solo se puede cancelar un envío si NO está en estado ENTREGADO
La cancelación requiere un motivo obligatorio (mínimo 5 caracteres)
Al cancelar, liberar la capacidad del vehículo si estaba asignado
RN-04: Validaciones de Datos
Teléfono: formato colombiano (10 dígitos, inicia con 3 o 6)
Peso: mínimo 0.1 kg, máximo 100 kg por envío
Dimensiones: mínimo 1 cm, máximo 200 cm por lado
Direcciones: no pueden estar vacías
Ciudades: solo se permiten ciudades de la lista de referencia

RN-05: Código de Rastreo Único
| El código  | CM-XXXXXXXX |  debe ser único en el sistema |     |     |
| ---------- | ----------- | ----------------------------- | --- | --- |
No se pueden crear dos envíos con el mismo código
Datos de Referencia
Ciudades y Distancias (km)
| Origen   | Destino      |     | Distancia (km) | Tarifa Distancia |
| -------- | ------------ | --- | -------------- | ---------------- |
| Bogotá   | Medellín     |     | 480            | $12,000          |
| Bogotá   | Cali         |     | 360            | $9,000           |
| Bogotá   | Barranquilla |     | 950            | $20,000          |
| Medellín | Cali         |     | 310            | $8,000           |
| Medellín | Barranquilla |     | 650            | $15,000          |
| Cali     | Barranquilla |     | 900            | $18,000          |
Vehículos Disponibles
|          |           |     | Capacidad Peso | Capacidad Volumen |
| -------- | --------- | --- | -------------- | ----------------- |
| ID Placa | Conductor |     |                |                   |
|          |           |     | (kg)           | (m³)              |
ABC-
| 1   | Juan Pérez |     | 500 | 10  |
| --- | ---------- | --- | --- | --- |
123
| DEF- | María |     |     |     |
| ---- | ----- | --- | --- | --- |
| 2    |       |     | 300 | 6   |
| 456  | López |     |     |     |
GHI-
| 3   | Carlos Ruiz |     | 800 | 15  |
| --- | ----------- | --- | --- | --- |
789

Requerimientos Técnicos
La arquitectura es de libre elección del candidato. Puede usar Entity
Framework Core, Dapper, o incluso almacenamiento en memoria. La decisión
arquitectónica se evaluará como indicador de capacidades de diseño.
Se requieren pruebas automatizadas (unitarias como mínimo).
Se recomienda usar principios de diseño (SOLID, DRY, KISS) y patrones
arquitectónicos apropiados.
La API debe exponer endpoints RESTful con respuestas HTTP apropiadas (200,
201, 400, 404, 409, 500).
Se valora el uso de validación de inputs, manejo centralizado de errores, y logging.
Entregables Obligatorios
1. Repositorio GitHub público con el código fuente completo
2. README.md con:
Instrucciones claras para ejecutar el proyecto localmente
Descripción de la arquitectura elegida y justificación
Tecnologías utilizadas
Ejemplos de llamadas a la API (curl o Postman collection)
3. Tests automatizados que validen los flujos de negocio principales
4. Código compilable siguiendo las instrucciones del README
Criterios de Evaluación
Se evaluará:
Cumplimiento de los requerimientos funcionales y reglas de negocio
Arquitectura y diseño de la solución
Calidad del código y principios de diseño aplicados
Manejo de errores y excepciones
Seguridad de la aplicación
Cobertura y calidad de las pruebas

Documentación
Notas Adicionales
Puedes usar herramientas de IA (GitHub Copilot, ChatGPT, etc.) como parte de tu
flujo de trabajo.
No se requiere frontend — una API funcional con Swagger/OpenAPI es suficiente.
Puedes usar cualquier proveedor de nube para despliegue opcional (AWS, Azure,
GCP).
Calibración por Plazo de Entrega
El alcance de la prueba DEBE ajustarse al plazo otorgado.
Nota interna (NO incluir en el enunciado del candidato): 1 día de plazo = ~4
horas de desarrollo efectivo. Esta equivalencia es para calibrar el alcance internamente.
En el enunciado solo se muestra "X días" sin detallar horas.
Se asume que el candidato usará herramientas de IA (Copilot, ChatGPT, etc.), por lo que
el rendimiento esperado es mayor al convencional — pero calibrado a bloques de ~4h
reales.

Horas
| Plazo |     | Alcance esperado (con IA) | Ajustes |
| ----- | --- | ------------------------- | ------- |
efectivas
2-3 requerimientos funcionales,
Un módulo/dominio
2 reglas de negocio, 1 flujo
bien hecho. README
| 1 día | ~4h | principal con 1 caso borde. Sin |     |
| ----- | --- | ------------------------------- | --- |
básico. Tests unitarios
despliegue. Enfoque en calidad
mínimos.
sobre cantidad.
|        |     | 3-5 requerimientos funcionales,   | Alcance estándar para la |
| ------ | --- | --------------------------------- | ------------------------ |
|        |     | 3-4 reglas de negocio, 1-2 flujos | mayoría de pruebas.      |
| 2 días | ~8h |                                   |                          |
|        |     | con 2 casos borde. Sin            | README con               |
|        |     | despliegue (bonus si ≥ Senior).   | arquitectura.            |
4-6 requerimientos funcionales,
|     |     | 4-5 reglas de negocio, 2 flujos | Mayor profundidad en |
| --- | --- | ------------------------------- | -------------------- |
3 días ~12h con 2-3 casos borde. Despliegue tests y documentación
|     |     | como bonus (≥ Middle) u | de decisiones. |
| --- | --- | ----------------------- | -------------- |
obligatorio (≥ Senior).
5-8 requerimientos funcionales,
|     |     | 5-7 reglas de negocio, 2-3 flujos | Prueba completa. |
| --- | --- | --------------------------------- | ---------------- |
4-5
|     | ~16-20h | con 3-4 casos borde. Despliegue | Múltiples módulos |
| --- | ------- | ------------------------------- | ----------------- |
días
|     |     | obligatorio (≥ Middle). | relacionados. |
| --- | --- | ----------------------- | ------------- |
Componente SQL si aplica.
7-10 requerimientos
|     |     | funcionales, integración entre | Solo para niveles |
| --- | --- | ------------------------------ | ----------------- |
6-7
|     | ~24-28h | módulos, componente SQL si | Senior/Lead. Diseño |
| --- | ------- | -------------------------- | ------------------- |
días
|     |     | aplica, documentación de | sistémico. |
| --- | --- | ------------------------ | ---------- |
decisiones técnicas detallada.
Principio rector: Si una persona del nivel solicitado + IA no puede completar la
prueba con calidad aceptable en las horas efectivas estimadas, el alcance es excesivo. Si
la completa en menos de la mitad de las horas, el alcance es insuficiente.