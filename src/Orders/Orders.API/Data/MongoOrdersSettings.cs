namespace Orders.API.Data;

// Solo datos NO sensibles (la cadena de conexión real va aparte, en
// ConnectionStrings:Database, para poder inyectarla por variable de entorno
// sin que quede escrita en el repositorio).
public class MongoOrdersSettings
{
    public string DatabaseName { get; set; } = "OrdersDb";
    public string CollectionName { get; set; } = "orders";
}
