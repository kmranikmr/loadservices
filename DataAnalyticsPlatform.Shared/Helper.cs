using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataAnalyticsPlatform.Shared
{
    public static class Helper
    {
        public static Dictionary<string, int> FileNametoId = new Dictionary<string, int>();

        public static int _runningFileNumber;
        public static int GetFileId(string fileName)
        {
            if (FileNametoId.ContainsKey(fileName))
            {
                return FileNametoId[fileName];
            }
            else
            {
                FileNametoId.Add(fileName, ++_runningFileNumber);
                return _runningFileNumber;
            }
        }
        public static string GetJson(object record)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(record);

            return json;
        }

        public static string GetJsonSchema(Type type)
        {
            JSchemaGenerator jsonSchemaGenerator = new JSchemaGenerator();
            JSchema schema = jsonSchemaGenerator.Generate(type);
            schema.Title = type.Name;

            return schema.ToString();
        }

        public static T DeserializeFromXmlString<T>(string xml) where T : new()
        {
            T xmlObject = new T();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(xml);
            xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return true;// cnt.Values.All(c => c == 0) ;
        }

        public static async Task<bool> IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                await Task.Run(() =>
                {
                    using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        if (inputStream.Length > 0)
                        {
                            inputStream.Close();
                            return true;
                        }
                        else
                            return false;
                    }

                });
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static async void WaitForFile(string filename)
        {
            //This will lock the execution until the file is ready
            //TODO: Add some logic to make it async and cancelable
            while (!await IsFileReady(filename)) { }
        }

        public static string CreateCsfromJson(string json)
        {
            string inputPath = @"temp" + Guid.NewGuid().ToString() + ".json";
            string outputPath = @"petstore.cs"; // path of the output csharp file

            try
            {
                File.WriteAllText(inputPath, json);
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = $"/C quicktype {inputPath} --top-level originalrecord -l cs --no-boolean-strings --no-enums --csharp-version 6 --array-type array --features just-types",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                File.Delete(inputPath);
                return output;
            }
            catch (Exception ex)
            {
                return "";
            }

        }

    }
}
