using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Modules.Orders.Domain.Orders.Entities;

namespace Modules.Orders.Infrastructure.Database.EventSourcing
{
    internal sealed class OrderJsonConverter : JsonConverter<Order>
    {
        private static readonly Func<Order> Construct = BuildConstructor();
        private static readonly OrderMember[] Members = BuildMembers();

        public override Order? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected StartObject token while reading {nameof(Order)}.");

            var order = Construct();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return order;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException($"Expected PropertyName token while reading {nameof(Order)}.");

                var propertyName = reader.GetString();
                reader.Read();

                var member = Array.Find(Members, m => m.JsonName == propertyName);
                if (member is null)
                {
                    reader.Skip();
                    continue;
                }

                var value = JsonSerializer.Deserialize(ref reader, member.Type, options);
                member.Set(order, value);
            }

            throw new JsonException($"Unexpected end of JSON while reading {nameof(Order)}.");
        }

        public override void Write(Utf8JsonWriter writer, Order value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var member in Members)
            {
                writer.WritePropertyName(member.JsonName);
                JsonSerializer.Serialize(writer, member.Get(value), member.Type, options);
            }

            writer.WriteEndObject();
        }

        private static Func<Order> BuildConstructor()
        {
            var constructor = typeof(Order).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null)
                ?? throw new InvalidOperationException($"{nameof(Order)} has no parameterless constructor.");

            return Expression.Lambda<Func<Order>>(Expression.New(constructor)).Compile();
        }

        private static OrderMember[] BuildMembers()
        {
            string[] names =
            [
                "Id", "CreatedOn", "CustomerId", "LaunchId", "SellerId", "ProductId", "Quantity",
                "UnitPrice", "TotalAmount", "OrderCode", "Status", "ExpiresAt", "ConfirmedAt", "Reason"
            ];

            return names.Select(name =>
            {
                var property = typeof(Order).GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    ?? throw new InvalidOperationException($"{nameof(Order)} has no property named '{name}'.");

                return new OrderMember(name, property.PropertyType, BuildGetter(property), BuildSetter(property));
            }).ToArray();
        }

        private static Func<Order, object?> BuildGetter(PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(Order), "instance");
            var access = Expression.Property(instance, property);
            var convert = Expression.Convert(access, typeof(object));

            return Expression.Lambda<Func<Order, object?>>(convert, instance).Compile();
        }

        private static Action<Order, object?> BuildSetter(PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(Order), "instance");
            var value = Expression.Parameter(typeof(object), "value");
            var convert = Expression.Convert(value, property.PropertyType);
            var access = Expression.Property(instance, property);
            var assign = Expression.Assign(access, convert);

            return Expression.Lambda<Action<Order, object?>>(assign, instance, value).Compile();
        }

        private sealed record OrderMember(string JsonName, Type Type, Func<Order, object?> Get, Action<Order, object?> Set);
    }
}