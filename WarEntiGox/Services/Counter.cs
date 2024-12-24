using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Counter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public string Name { get; set; } // Sayaç ismi, örneğin: "CompanyId"
    public int Value { get; set; }   // Sayaç değeri
}
