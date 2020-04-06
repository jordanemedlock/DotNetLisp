using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace JEM.Testing
{
    public class JsonFileDataAttribute : DataAttribute
    {
        private readonly string _filePath;

        private readonly string _propertyName;

        public JsonFileDataAttribute(string filePath)
        {
            _filePath = filePath;
        }

        public JsonFileDataAttribute(string filePath, string propertyName)
        {
            _filePath = filePath;
            _propertyName = propertyName;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException(nameof(testMethod));
            }

            var parameters = testMethod.GetParameters();
            // Get the absolute path to the JSON file
            var path = Path.IsPathRooted(_filePath)
                ? _filePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), _filePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }

            // Load the file
            var fileData = File.ReadAllText(_filePath);
            
            return GetData(fileData);
        }

        private IEnumerable<object[]> GetData(string jsonData)
        {
            Dictionary<string, Dictionary<string, object>> dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(jsonData);
            var objectList = new List<object[]>();
            if (string.IsNullOrEmpty(_propertyName))
            {
                foreach (var kvp in dict)
                {
                    objectList.Add(new object[] { kvp.Key, kvp.Value });
                }
            }
            else
            {
                objectList.Add(new object[] { _propertyName, dict[_propertyName] });
            }
            return objectList;
        }
    }
}