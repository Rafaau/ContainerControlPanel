using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ContainerControlPanel.Domain.Models;

public class MagicInput
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Content { get; set; } = string.Empty;
}
