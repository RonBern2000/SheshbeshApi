using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SheshbeshApi.Models
{
    public class Message
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("sender_username"), BsonRepresentation(BsonType.String)]
        public string? SenderUsername { get; set; }

        [BsonElement("recipient_username"), BsonRepresentation(BsonType.String)]
        public string? RecipientUsername { get; set; }
        [BsonElement("message"), BsonRepresentation(BsonType.String)]
        public string? MessageContent { get; set; }
        [BsonElement("time_sent"), BsonRepresentation(BsonType.DateTime)]
        public DateTime Timestamp { get; set; }
    }
}
