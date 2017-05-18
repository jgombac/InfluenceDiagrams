using InfluenceDiagrams.Nodes;
using InfluenceDiagrams.Relations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace InfluenceDiagrams
{
    class SerializeData
    {
        Stream stream = null;
        BinaryFormatter bformatter = null;
        string filename = "";

        public SerializeData(string filename)
        {
            this.filename = filename;
            stream = File.Open(filename, FileMode.OpenOrCreate);
            bformatter = new BinaryFormatter();
        }

        public void SerializeObject(Object objectToSerialize)
        {
            bformatter.Serialize(stream, objectToSerialize);
        }

        public void DeserializeObjects()
        {
            Object objectToDeserialize = null;
            try
            {
                while (stream.CanSeek)
                {
                    objectToDeserialize = (Object)bformatter.Deserialize(stream);
                    if (objectToDeserialize is SerialNode)
                    {
                        SerialNode serialNode = (SerialNode)objectToDeserialize;
                        Node node = new Node(serialNode);
                    }
                    else if(objectToDeserialize is SerialRelation)
                    {
                        SerialRelation serialRelation = (SerialRelation)objectToDeserialize;
                        Relation rel = new Relation(serialRelation);
                    }
                }
            }
            catch (SerializationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("EndoOfFile");
            }

        }

        public void CloseStream()
        {
            stream.Close();
        }
    }
}
