using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace dummy_serializer
{
    class Program
    {
        const int RANDOM_STRING_LENGHT = 8;

        static void Main(string[] args)
        {
            Init();
            var mc = new MongoClient();
            var db = mc.GetDatabase("mydb");
            var dummies = db.GetCollection<Dummy>("Dummy");

            var d = new Dummy(GenerateRandomString(RANDOM_STRING_LENGHT), GenerateRandomString(RANDOM_STRING_LENGHT));
            dummies.InsertOne(d);
            Console.WriteLine("Inserted");
            Console.WriteLine("Fetching all inputs from collection Dummy");
            var entries = dummies.Find<Dummy>(new BsonDocument()).ToList();

            PrintEntries(entries);
            Console.WriteLine("succes");
        }

        private static void PrintEntries(IEnumerable<Dummy> dummies)
        {
            foreach (var d in dummies)
            {
                Console.WriteLine(d.Prop1);
                Console.WriteLine(d.Prop2);
                Console.WriteLine();
            }
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void Init()
        {
            BsonClassMap.RegisterClassMap<Dummy>(cm =>
            {
                cm.MapProperty(d => d.Prop1);
                cm.MapProperty(d => d.Prop2);
                cm.MapCreator(d => new Dummy(d.Prop1, d.Prop2));
            });

            BsonSerializer.RegisterSerializer(typeof(Dummy), new DummySerializer());
        }
    }

    //Or decorate
    //[BsonSerializer(typeof(DummySerializer))]
    public readonly struct Dummy
    {
        public string Prop1 { get; }
        public string Prop2 { get; }

        [BsonConstructor]
        public Dummy(string p1, string p2)
        {
            Prop1 = p1;
            Prop2 = p2;
        }
    }

    public class DummySerializer : SerializerBase<Dummy>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Dummy value)
        {
            context.Writer.WriteStartDocument();
            context.Writer.WriteName("_id");
            context.Writer.WriteObjectId(ObjectId.GenerateNewId());

            context.Writer.WriteName("prop1");
            context.Writer.WriteString(value.Prop1);

            context.Writer.WriteName("prop2");
            context.Writer.WriteString(value.Prop2);

            context.Writer.WriteEndDocument();
        }

        public override Dummy Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartDocument();
            //not used, but can be used if wanted
            ObjectId id = context.Reader.ReadObjectId();

            var p1 = context.Reader.ReadString();
            var p2 = context.Reader.ReadString();
            context.Reader.ReadEndDocument();

            return new Dummy(p1, p2);
        }
    }
}