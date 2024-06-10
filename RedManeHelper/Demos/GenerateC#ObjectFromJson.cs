using System.Text;
using DemoKatan.Static;
using Newtonsoft.Json.Linq;

namespace DemoKatan.Demos
{
    public class GenerateC_ObjectFromJson
    {
        private string _exceptionsDir = @"C:\Users\jreiner\source\repos\RedManeHelper\RedManeHelper\FactoryExceptions";
        private string _factoryEntitiesDir = @"C:\Users\jreiner\source\repos\RedManeHelper\RedManeHelper\FactoryEntities";
        //private string _mCaseFactoryDir = @"C:\Users\jreiner\source\repos\AR-mCase-CustomEvents\MCaseCustomEvents\ARFocus\FactoryEntities";
        private HashSet<string> ClassSet = new();
        private HashSet<string> FieldSet = new();
        
        public void EntityFactory()
        {
            var dirPath = @"C:\Users\jreiner\source\repos\AR-mCase-Config\Datalists";

            var dirInfo = new DirectoryInfo(dirPath);

            var files = dirInfo.GetFiles();

            if (!Directory.Exists(_factoryEntitiesDir))
                Directory.CreateDirectory(_factoryEntitiesDir);

            foreach (var file in files)
            {
                var filePath = file.FullName;

                GenerateClass(filePath);
            }

            Console.WriteLine();
        }
        private void GenerateClass(string jsonPath)
        {
            ClassSet.Clear();

            var file = File.ReadAllText(jsonPath);

            var closing = file.LastIndexOf(']');

            var cleanedClose = file.Remove(closing);

            var result = cleanedClose.Remove(0, 1);

            var jsonObject = JObject.Parse(result);
            
            var fields = jsonObject["Fields"];

            var sb = new StringBuilder();
            var className = GetName(jsonObject) + "Entity";
            var sysName = jsonObject["SystemName"].Parent.FirstOrDefault().Value<string>();

            sb.AppendLine(
                "using System.Collections.Generic;\nusing System.Linq;\nusing MCaseCustomEvents.ARFocus.DataAccess;\nusing MCaseEventsSDK;\nusing MCaseEventsSDK.Util;\nusing MCaseEventsSDK.Util.Data;");
            sb.AppendLine("namespace ARFocus.FactoryEntities");
            sb.AppendLine("{"); //open namespace
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("     {"); //open class
            sb.AppendLine("        private RecordInstanceData _recordInsData;");
            sb.AppendLine("        private AEventHelper _eventHelper;");
            sb.AppendLine($"        public {className}(RecordInstanceData recordInsData, AEventHelper eventHelper)");
            sb.AppendLine("        {");
            sb.AppendLine("             _recordInsData = recordInsData;");
            sb.AppendLine("             _eventHelper = eventHelper;");
            sb.AppendLine("        }");
            sb.AppendLine($"        public string SystemName => \"{sysName}\";");
            sb.AppendLine($"        public void SaveRecord()");
            sb.AppendLine("        {");
            sb.AppendLine("             _eventHelper.SaveRecord(_recordInsData);");
            sb.AppendLine("        }");

            foreach (var field in fields)
            {
                var embeddedJsonstring = field.ToString();

                var jObject = JObject.Parse(embeddedJsonstring);

                var fieldSysName = GetSystemName(jObject);

                if (string.IsNullOrEmpty(fieldSysName) || FieldSet.Contains(fieldSysName))
                    continue;

                var property = AddProperties(jObject, fieldSysName);

                if (string.IsNullOrEmpty(property))
                    continue;

                sb.AppendLine(property);

                FieldSet.Add(fieldSysName);
            }

            sb.AppendLine("    }");//close class
            sb.AppendLine("}");//close namespace


            var filePath = Path.Combine(_factoryEntitiesDir, $"{className}.cs");

            File.WriteAllText(filePath, sb.ToString());
        }

        private string AddProperties(JObject jObject, string sysName)
        {
            var type = GetType(jObject);

            var typeEnum = type.GetEnumValue<mCaseTypes>();

            string property;

            switch (typeEnum)
            {
                //case mCaseTypes.EmbeddedList:
                //case mCaseTypes.CascadingDropDown:
                case mCaseTypes.DropDownList:
                case mCaseTypes.DynamicDropDown:
                case mCaseTypes.CascadingDynamicDropDown:
                    property = EnumerableFactory(sysName);
                    break;
                case mCaseTypes.Header:
                case mCaseTypes.String:
                case mCaseTypes.LongString:
                case mCaseTypes.EmailAddress:
                case mCaseTypes.Phone:
                case mCaseTypes.Narrative:
                case mCaseTypes.URL:
                case mCaseTypes.Section:
                case mCaseTypes.Address:
                case mCaseTypes.Number:
                case mCaseTypes.Money:
                case mCaseTypes.Date:
                case mCaseTypes.DateTime:
                case mCaseTypes.Time:
                case mCaseTypes.Boolean:
                    property = StringFactory(jObject, sysName);
                    break;
                case mCaseTypes.UserRoleSecurityRestrict:

                case mCaseTypes.DynamicCalculatedField:
                case mCaseTypes.CalculatedField:
                case mCaseTypes.UniqueIdentifier:
                case mCaseTypes.User:
                case mCaseTypes.Attachment:
                case mCaseTypes.EmbeddedDocument:
                case mCaseTypes.Score:
                case mCaseTypes.HiddenField:
                case mCaseTypes.ReadonlyField:
                case mCaseTypes.LineBreak:
                default:
                    property = string.Empty;
                    break;
            }

            if (string.IsNullOrEmpty(property) || ClassSet.Contains(property))
                return string.Empty;

            ClassSet.Add(property);

            return property;

        }

        private string GetName(JObject? jsonObject)
        {
            var classNameToken = jsonObject["Name"].Parent.FirstOrDefault();

            var propertyValue = classNameToken.Last["Value"].Value<string>();

            return string.IsNullOrEmpty(propertyValue)
                ? string.Empty
                : CleanString(propertyValue);
        }

        private string GetType(JObject? jsonObject)
        {
            var propertyValue = jsonObject["Type"].Parent.FirstOrDefault().Value<string>();

            return string.IsNullOrEmpty(propertyValue)
                ? string.Empty
                : CleanString(propertyValue);
        }

        private string GetSystemName(JObject? jsonObject)
        {
            var propertyValue = jsonObject["SystemName"].Parent.FirstOrDefault().Value<string>();

            return string.IsNullOrEmpty(propertyValue)
                ? string.Empty
                : CleanString(propertyValue);
        }

        private bool IsStringReadonly(JObject? jsonObject)
        {
            var propertyValue = jsonObject["DefaultValue"].Parent.FirstOrDefault().Value<string>();

            return propertyValue != null && propertyValue.Contains("COALESCE");
        }

        private string CleanString(string str)
        {
            // Define a set of allowed characters (A-Z, a-z, and 0-9)
            var allowedChars = new HashSet<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

            // Use LINQ to filter characters based on the allowed set
            var property = new string(str.Where(c => allowedChars.Contains(c)).ToArray());

            return property;
        }

        /// <summary>
        /// Factory generator for generating enumerable mCase data structures. Base data type are strings, so default generator 
        /// </summary>
        /// <param name="sysName"></param>
        /// <returns></returns>
        private string EnumerableFactory(string sysName)
        {
            var property =
                $"        private List<string> _{sysName} = new List<string>();\n" +
                $"        public List<string> {sysName}\n" +
                "        {\n" +
                "           get\n" +
                "           {\n" +
                $"              if(_{sysName}.Any())\n" +
                $"                  return _{sysName};\n" +
                $"              _{sysName} = _recordInsData.GetGenericDropDownValues(_eventHelper, \"{sysName}\");\n" +
                $"              return _{sysName};\n" +
                "           }\n" +
                "           set\n" +
                "           {\n" +
                $"              _{sysName} = value;\n" +
                $"              _recordInsData.SetValue(\"{sysName}\", _{sysName}.Last());\n" +
                "           }\n" +
                "        }\n";

            return property;
        }

        /// <summary>
        /// Generate factory for entering into mCase db. This process should feel as simple as updating an object. The getters and setters
        /// Use mCase OOTB genearations for CRUD operations. Find the similarities for CRUD and update the properties here
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        private string StringFactory(JObject jObject, string sysName)
        {
            string property;

            string privateSysName;

            if (IsStringReadonly(jObject))
            {//string is readonly, only use getter
                privateSysName = $"_{sysName}Readonly";

                property =
                    $"        private string {privateSysName} = string.Empty;\n" +
                    $"        public string {sysName}Readonly\n" +
                    "        {\n" +
                    "           get\n" +
                    "           {\n" +
                    $"              if(!string.IsNullOrEmpty({privateSysName}))\n" +
                    $"                  return {privateSysName};\n" +
                    $"              {privateSysName} = _recordInsData.GetFieldValue(\"{sysName}\");\n" +
                    $"              return {privateSysName};\n" +
                    "           }\n" +
                    "        }\n";
            }
            else
            {//string is not readonly, only use getter and setter
                privateSysName = $"_{sysName}";

                property =
                    $"        private string {privateSysName} = string.Empty;\n" +
                    $"        public string {sysName}\n" +
                    "        {\n" +
                    "           get\n" +
                    "           {\n" +
                    $"              if(!string.IsNullOrEmpty({privateSysName}))\n" +
                    $"                  return {privateSysName};\n" +
                    $"              {privateSysName} = _recordInsData.GetFieldValue(\"{sysName}\");\n" +
                    $"              return {privateSysName};\n" +
                    "           }\n" +
                    "           set\n" +
                    "           {\n" +
                    $"              {privateSysName} = value;\n" +
                    $"              _recordInsData.SetValue(\"{sysName}\", {privateSysName});\n" +
                    "           }\n" +
                    "        }\n";

            }

            return property;
        }
    }
}
