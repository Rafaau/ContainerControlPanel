using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Magic input model.
/// </summary>
public class MagicInput
{
    /// <summary>
    /// Gets or sets the ID of the magic input.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the magic input.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
