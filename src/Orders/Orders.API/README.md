# Orders.API — Microservicio de Órdenes de Compra

Microservicio construido con **ASP.NET Core 9 Minimal API**, siguiendo el mismo patrón
CQRS + MediatR + Carter que ya usan `Catalog.API` y `Basket.API` en este proyecto,
pero persistiendo en **MongoDB Atlas** en vez de PostgreSQL.

## Arquitectura

```
Cliente (React)
   │
   │ 1. GET /basket/{userName}          → Basket.API (ya existente)
   │ 2. POST /api/orders                → Orders.API (este microservicio)
   ▼
Orders.API
   │  - Valida el carrito llamando por HTTP a Basket.API
   │  - Calcula subtotal, impuestos (IVA 16% configurable) y total
   │  - Persiste la orden en MongoDB Atlas
   ▼
MongoDB Atlas (colección "orders")
```

Orders.API **no** tiene acceso directo a la base de datos de Basket.API — respeta el
límite entre microservicios y solo consume su contrato HTTP público
(`GET /basket/{userName}`), tal como pide el punto 6 del examen ("conserva las
responsabilidades de cada servicio").

## Endpoints

| Método | Ruta                                  | Descripción                                    |
|--------|----------------------------------------|-------------------------------------------------|
| POST   | `/api/orders`                          | Genera una orden a partir de un carrito         |
| GET    | `/api/orders/{id}`                     | Consulta una orden por Id                       |
| GET    | `/api/orders/customer/{customerId}`    | Lista las órdenes de un cliente                 |
| PATCH  | `/api/orders/{id}/status`              | Cambia el estado de una orden                   |

Documentación interactiva completa (Swagger): **`/swagger`**

### POST /api/orders

```json
// Request
{
  "customerId": "erik",
  "basketId": "erik"
}
```

> `basketId` es el mismo `userName` que usa Basket.API para identificar el carrito.
> En este proyecto (sin login) `customerId` y `basketId` normalmente son el mismo
> valor — se dejan separados en el contrato porque así lo pide el examen y porque
> a futuro (con login real) podrían no coincidir.

Header opcional: `Idempotency-Key: <cualquier-string-único>`

```json
// Response 201 Created
{
  "id": "b2e1...",
  "customerId": "erik",
  "status": "Pending",
  "subtotal": 100.00,
  "tax": 16.00,
  "total": 116.00,
  "createdAt": "2026-08-12T20:00:00Z"
}
```

Errores:
- **400** si el carrito no existe o está vacío, o si trae cantidades/precios inválidos.
- **500** si falla la persistencia (mensaje genérico, sin exponer detalles internos).

### PATCH /api/orders/{id}/status

```json
{ "status": "Confirmed" }
```

Transiciones permitidas: `Pending → Confirmed`, `Pending → Cancelled`.
Cualquier otra combinación (incluyendo `Cancelled → Confirmed`) responde **400**.

## Idempotencia

Si el cliente reenvía `POST /api/orders` con el mismo header `Idempotency-Key`,
Orders.API **no crea una segunda orden**: devuelve la orden que ya se había
generado la primera vez. Esto se implementa con un índice único **parcial** en
MongoDB sobre el campo `IdempotencyKey` (solo aplica a documentos que sí tienen
ese campo, así que las órdenes sin la clave conviven sin problema entre sí).

## Variables de entorno (secretos fuera del código)

| Variable                          | Ejemplo                                                  |
|------------------------------------|-----------------------------------------------------------|
| `ConnectionStrings__Database`      | `mongodb+srv://usuario:password@cluster.mongodb.net/`     |
| `Services__BasketApiBaseUrl`       | `https://basket-production-bbb1.up.railway.app`           |
| `PORT`                             | `8080`                                                     |
| `ASPNETCORE_ENVIRONMENT`           | `Production`                                               |

`Mongo:DatabaseName` (`OrdersDb`) y `Mongo:CollectionName` (`orders`) no son
secretos, así que viven directamente en `appsettings.json`.

## Ejecución local

1. Ten Basket.API corriendo (local o apuntando a Railway) y ajusta
   `Services:BasketApiBaseUrl` en `appsettings.Development.json`.
2. Crea un archivo `appsettings.Development.json` local (o usa `dotnet user-secrets`)
   con tu cadena de conexión real de MongoDB Atlas en `ConnectionStrings:Database`.
3. `dotnet run` desde `src/Orders/Orders.API`.
4. Abre `https://localhost:<puerto>/swagger`.

## Decisiones de diseño documentadas

- **Transición de estado inválida → 400** (no 409), para mantener el mismo estilo
  de respuesta de error que ya usan Catalog.API y Basket.API en este proyecto
  (`BuildingBlocks.Exceptions.BadRequestException`).
- **Reenvío de la misma Idempotency-Key → 201 con la orden ya existente** (no se
  genera una segunda orden ni se marca error).
- El repositorio (`OrdersRepository`) se registra como **Singleton** porque
  `MongoClient` está diseñado para reutilizarse durante toda la vida de la
  aplicación (crear uno nuevo por request desperdicia conexiones).
