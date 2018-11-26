using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MongodbDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            QSet set = prepareData();

            BsonClassMap.RegisterClassMap<Option>();

            BsonClassMap.RegisterClassMap<ONode>(cm => 
            {
                cm.AutoMap();

                cm.MapMember(c => c.Options).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<int, Option>>(DictionaryRepresentation.ArrayOfArrays));
            });

            BsonClassMap.RegisterClassMap<QNode>(cm=> 
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Guid);
            });

            BsonClassMap.RegisterClassMap<QSet>(cm=> 
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Guid);
            });

            MongoClient client = new MongoClient("mongodb://localhost:27017");


            IMongoDatabase database = client.GetDatabase("AHabit");

            IMongoCollection<QSet> collection  = database.GetCollection<QSet>("QSets");

            collection.InsertOne(set);


            QSet result = collection.Find<QSet>(s => s.Subject == "xxx").First();


        }

        private static QSet prepareData()
        {
            Option option1_1 = new Option()
            {
                Order = 1,
                Text = "Happy",
                Image = "Happy.png"
            };

            Option option1_2 = new Option()
            {
                Order = 2,
                Text = "Sad",
                Image = "Sad.png"
            };

            ONode qNode1 = new ONode()
            {
                Guid = Guid.NewGuid().ToString(),
                Question = "Are you happy ?",
                Options = new Dictionary<int, Option>()
            };

            qNode1.Options.Add(1, option1_1);
            qNode1.Options.Add(2, option1_2);


            Option option2_1 = new Option()
            {
                Order = 1,
                Text = "Cloud",
                Image = "Cloud.png"
            };

            Option option2_2 = new Option()
            {
                Order = 2,
                Text = "Sunny",
                Image = "Sunny.png"
            };

            XNode qNode2 = new XNode()
            {
                Guid = Guid.NewGuid().ToString(),
                Dict = new Dictionary<string, string>()
            };

            qNode2.Dict.Add("1", "option2_1");
            qNode2.Dict.Add("2", "option2_2");


            QSet qSet = new QSet()
            {
                //Id = ObjectId.GenerateNewId(),
                Guid = Guid.NewGuid().ToString(),
                Subject = "xxx",
                QNodes = new Dictionary<string, QNode>()
            };

            qSet.QNodes.Add(qNode1.Guid, qNode1);
            qSet.QNodes.Add(qNode2.Guid, qNode2);

            return qSet;
        }
    }

    class QSet
    {
        //[BsonId] 
        public string Guid { get; set; }

        public string Subject { get; set; }

        //[BsonDictionaryOptions]
        public Dictionary<string, QNode> QNodes { get; set; }

        
    }

    class QNode
    {
        //[BsonId]
        public string Guid { get; set; }

        
    }

    class ONode : QNode
    {
        public string Question { get; set; }

        //[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, Option> Options { get; set; }

    }

    class XNode : QNode
    {
        public Dictionary<string, string> Dict { get; set; }
    }

    class Option
    {
        public int Order { get; set; }

        public string Text { get; set; }

        public string Image { get; set; }
    }
}
