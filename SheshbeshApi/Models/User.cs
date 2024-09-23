using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SheshbeshApi.Models
{
    public class User
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_name"), BsonRepresentation(BsonType.String)]
        public string? UserName { get; set; }

        [BsonElement("user_password"), BsonRepresentation(BsonType.String)]
        public string? PasswordHash { get; set; }

        [BsonElement("user_email"), BsonRepresentation(BsonType.String)]
        public string? Email { get; set; }
    }
}
