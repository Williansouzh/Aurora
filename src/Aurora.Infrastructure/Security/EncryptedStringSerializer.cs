using Aurora.Domain.ValueObjects;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Aurora.Infrastructure.Security;

public class EncryptedStringSerializer : SerializerBase<EncryptedString>
{
    public override EncryptedString Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) =>
        new(context.Reader.ReadString());

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        EncryptedString value) =>
        context.Writer.WriteString(value.CipherText);
}
