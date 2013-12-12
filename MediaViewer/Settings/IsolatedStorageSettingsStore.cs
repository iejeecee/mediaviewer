using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MediaViewer.Settings
{
    public class IsolatedStorageSettingsStore : JsonSettingsStoreBase
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const IsolatedStorageScope Scope = IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Roaming;

        protected override void WriteTextFile(string filename, string fileContents)
        {
            try
            {

                using (var isoStore = IsolatedStorageFile.GetStore(Scope, null, null))
                {
                    using (var stream = new IsolatedStorageFileStream(filename, FileMode.Create, isoStore))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.Write(fileContents);
                        writer.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Cannot write application settings to isolated storage file", e);
            }
        }

        protected override string ReadTextFile(string filename)
        {
            String result = null;

            try
            {

                using (var isoStore = IsolatedStorageFile.GetStore(Scope, null, null))
                {

                    if (isoStore.FileExists(filename))
                    {
                        using (var stream = new IsolatedStorageFileStream(filename, FileMode.Open, isoStore))
                        {
                            long length = stream.Length;

                            StreamReader reader = new StreamReader(stream);
                            result = reader.ReadToEnd();
                        }
                    }
                }

            }
            catch (Exception e)
            {
                log.Error("Cannot read application settings from isolated storage file", e);
            }

            return result;
        }
    }

    public abstract class JsonSettingsStoreBase : ISettingsStorage
    {
        public string SerializeList(List<string> listOfItems)
        {
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
            new DataContractJsonSerializer(typeof(List<string>)).WriteObject(ms, listOfItems);
            writer.Flush();
            var jsonString = Encoding.Default.GetString(ms.ToArray());

            return jsonString;
        }

        public List<string> DeserializeList(string serializedList)
        {
            return (List<string>)new DataContractJsonSerializer(typeof(List<string>))
                .ReadObject(new MemoryStream(Encoding.Default.GetBytes(serializedList)));
        }

        public void Save(string key, Dictionary<string, string> settings)
        {
            var filename = key + ".settings";

            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
            serializer.WriteObject(ms, settings);
            writer.Flush();
            var jsonString = Encoding.Default.GetString(ms.ToArray());
            WriteTextFile(filename, jsonString);
        }

        protected abstract void WriteTextFile(string filename, string fileContents);

        public Dictionary<string, string> Load(string key)
        {
            var filename = key + ".settings";

            var readTextFile = ReadTextFile(filename);
            if (!string.IsNullOrEmpty(readTextFile))
            {
                var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
                return (Dictionary<string, string>)serializer.ReadObject(new MemoryStream(Encoding.Default.GetBytes(readTextFile)));
            }

            return new Dictionary<string, string>();
        }

        protected abstract string ReadTextFile(string filename);
    }
}
