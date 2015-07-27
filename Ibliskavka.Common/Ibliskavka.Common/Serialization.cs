using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Ibliskavka.Common
{
    /// <summary>
    /// Some useful methods for dealing XML serialization
    /// </summary>
    public static class Serialization
    {
        /// <summary> 
        /// Serializes an object to a text file. 
        /// </summary> 
        /// <param name="dataToSerialize"></param> 
        /// <param name="fileName"></param> 
        /// <param name="omitXmlDeclaration">Exclude XML declaration element at the top of the document.</param> 
        public static void SerializeToFile<T>(T dataToSerialize, string fileName, bool omitXmlDeclaration)
        {
            using (var fileWriter = File.Open(fileName, FileMode.Create))
            {
                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.GetEncoding(1252),
                    OmitXmlDeclaration = omitXmlDeclaration,
                };

                using (var writer = XmlWriter.Create(fileWriter, settings))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, dataToSerialize);
                }
            }
        }

        /// <summary> 
        /// Deserializes and object from a text file. 
        /// </summary> 
        /// <typeparam name="T"></typeparam> 
        /// <param name="fileName"></param> 
        /// <returns></returns> 
        public static T DeserializeFromFile<T>(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T serializedData;

            using (var stream = File.Open(fileName, FileMode.Open))
            {
                serializedData = (T)serializer.Deserialize(stream);
            }

            return serializedData;
        }

        /// <summary>
        /// Serializes the data in the object to to a string.
        /// </summary>
        /// <typeparam name="T">Type of Object to serialize</typeparam>
        /// <param name="dataToSerialize">Object to serialize</param>
        public static string SerializeToString<T>(T dataToSerialize)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dataToSerialize);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Deserializes the xml string into an object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize</typeparam>
        /// <param name="serializedObject">String containing object XML</param>
        /// <returns>Object containing deserialized data</returns>
        public static T Deserialize<T>(string serializedObject)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T serializedData;

            using (var stream = new StringReader(serializedObject))
            {
                serializedData = (T)serializer.Deserialize(stream);
            }

            return serializedData;
        }
    }
}
