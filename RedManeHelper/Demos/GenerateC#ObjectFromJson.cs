using System.Text;
using DemoKatan.Static;
using DemoKatan.Static.Ignore;
using Newtonsoft.Json.Linq;

namespace DemoKatan.Demos
{
    public class GenerateC_ObjectFromJson
    {
        private string _exceptionsDir = @"C:\Users\jreiner\source\repos\RedManeHelper\RedManeHelper\FactoryExceptions";
        private string _factoryEntitiesDir = @"C:\Users\jreiner\Desktop\FactoryEntities\" + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss");
        //private string _mCaseFactoryDir = @"C:\Users\jreiner\source\repos\AR-mCase-CustomEvents\MCaseCustomEvents\ARFocus\FactoryEntities";
        private HashSet<string> FieldSet = new();
        private HashSet<string> Types = new();

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

            Console.WriteLine(string.Join('\n', Types));
        }
        private void GenerateClass(string jsonPath)
        {
            FieldSet.Clear();

            var file = File.ReadAllText(jsonPath);

            var closing = file.LastIndexOf(']');

            var cleanedClose = file.Remove(closing);

            var result = cleanedClose.Remove(0, 1);

            var jsonObject = JObject.Parse(result);

            var fields = jsonObject["Fields"];

            var sb = new StringBuilder();

            var sysName = GetSystemName(jsonObject);

            var className = sysName + "Entity";

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
            sb.AppendLine("        public void SaveRecord()");
            sb.AppendLine("        {");
            sb.AppendLine("             _eventHelper.SaveRecord(_recordInsData);");
            sb.AppendLine("        }");


            foreach (var field in fields)
            {
                var embeddedJsonString = field.ToString();

                var jObject = JObject.Parse(embeddedJsonString);

                var fieldSysName = GetSystemName(jObject);

                if (string.Equals(fieldSysName, "role", StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine();

                if (string.IsNullOrEmpty(fieldSysName) || FieldSet.Contains(fieldSysName))
                    continue;//if property is already in field list then continue, no need to duplicate property in class

                var property = AddProperties(jObject, fieldSysName);

                if (string.IsNullOrEmpty(property))
                    continue;

                sb.AppendLine(property);

                FieldSet.Add(fieldSysName);
            }

            sb.AppendLine("    }");//close class
            sb.AppendLine("}");//close namespace

            var log = $"{sysName} class created with {FieldSet.Count} properties";

            if (FieldSet.Count >= 100)
                log.PrintHeavy();
            else if (100 > FieldSet.Count && FieldSet.Count >= 50)
                log.PrintMedium();
            else
                log.PrintLight();

            var filePath = Path.Combine(_factoryEntitiesDir, $"{className}.cs");

            File.WriteAllText(filePath, sb.ToString());
        }

        private string AddProperties(JObject jObject, string sysName)
        {
            var type = GetType(jObject);

            Types.Add(jObject["Type"].Parent.FirstOrDefault().Value<string>());

            var typeEnum = type.GetEnumValue<mCaseTypes>();


            string property;

            switch (typeEnum)
            {
                //case mCaseTypes.EmbeddedList:
                case mCaseTypes.CascadingDropDown:
                case mCaseTypes.DropDownList:
                case mCaseTypes.DynamicDropDown:
                case mCaseTypes.CascadingDynamicDropDown:
                    property = EnumerableFactory(sysName, typeEnum);
                    break;
                case mCaseTypes.Header:
                case mCaseTypes.String:
                case mCaseTypes.LongString:
                case mCaseTypes.EmailAddress:
                case mCaseTypes.Phone:
                case mCaseTypes.Narrative:
                case mCaseTypes.URL:
                case mCaseTypes.Section:
                case mCaseTypes.Number:
                case mCaseTypes.Money:
                case mCaseTypes.Date:
                case mCaseTypes.DateTime:
                case mCaseTypes.Time:
                case mCaseTypes.Boolean:
                case mCaseTypes.ReadonlyField:
                case mCaseTypes.User:
                    property = StringFactory(jObject, sysName, type);
                    break;
                case mCaseTypes.UserRoleSecurityRestrict://not required in CE's
                case mCaseTypes.DynamicCalculatedField://not required in CE's
                case mCaseTypes.CalculatedField:// not required in CE's calculated in front end
                case mCaseTypes.UniqueIdentifier://not required in CE's
                case mCaseTypes.Address://key value
                case mCaseTypes.Attachment://key value 
                case mCaseTypes.EmbeddedDocument:// Not required in ce? blob?
                case mCaseTypes.Score://not required in CE's
                case mCaseTypes.HiddenField://not required in CE's
                case mCaseTypes.LineBreak://not required in CE's
                default:
                    property = string.Empty;
                    break;
            }

            return string.IsNullOrEmpty(property)
                ? string.Empty
                : property;
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
            // Define a set of allowed characters (A-Z, a-z)
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
        private static string EnumerableFactory(string sysName, mCaseTypes type)
        {
            string cascading;
            switch (type)
            {
                case mCaseTypes.CascadingDropDown:
                case mCaseTypes.DropDownList:
                    cascading = type == mCaseTypes.CascadingDropDown
                        ? "Cascading "
                        : string.Empty;

                    return
                        $"        private List<string> _{sysName} = new List<string>();\n" +
                        $"        /// <summary>\n" +
                        $"        /// mCase data type: {cascading}Drop down list.\n" +
                        $"        /// It can take in named values such as 'no', 'yes' and any other strings and save it in the db\n" +
                        $"        /// </summary>\n" +
                        $"        public List<string> {sysName}\n" +
                        "        {\n" +
                        "           get\n" +
                        "           {\n" +
                        $"              if(_{sysName}.Any())\n" +
                        $"                  return _{sysName};\n" +
                        $"              _{sysName} = _recordInsData.GetMultiSelectFieldValue(\"{sysName}\");\n" +
                        $"              return _{sysName};\n" +
                        "           }\n" +
                        "           set\n" +
                        "           {\n" +
                        $"              _{sysName} = value;\n" +
                        $"              _recordInsData.SetValue(\"{sysName}\", _{sysName}.Last());\n" +
                        "           }\n" +
                        "        }\n";
                case mCaseTypes.CascadingDynamicDropDown:
                case mCaseTypes.DynamicDropDown:
                    {
                        cascading = type == mCaseTypes.CascadingDynamicDropDown
                            ? "Cascading "
                            : string.Empty;

                        return
                            $"        private List<string> _{sysName} = new List<string>();\n" +
                            $"        /// <summary>\n" +
                            $"        /// mCase data type: {cascading}Dynamic Drop down.\n" +
                            $"        /// Requires the Field ID as the parameter entered in the list.\n" +
                            $"        /// </summary>\n" +
                            $"        public List<string> {sysName}\n" +
                            "        {\n" +
                            "           get\n" +
                            "           {\n" +
                            $"              if(_{sysName}.Any())\n" +
                            $"                  return _{sysName};\n" +
                            $"              _{sysName} = _eventHelper.GetDynamicDropdownRecords(_recordInsData.RecordInstanceID, \"{sysName}\").Select(x => x.Label).ToList();\n" +
                            $"              return _{sysName};\n" +
                            "           }\n" +
                            "           set\n" +
                            "           {\n" +
                            $"              _{sysName} = value;\n" +
                            $"              _recordInsData.SetValue(\"{sysName}\", _{sysName}.Last());\n" +
                            "           }\n" +
                            "        }\n";
                    }
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Generate factory for entering into mCase db. This process should feel as simple as updating an object. The getters and setters
        /// Use mCase OOTB generations for CRUD operations. Find the similarities for CRUD and update the properties here
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        private string StringFactory(JObject jObject, string sysName, string dataType)
        {
            var privateSysName = $"_{sysName}";

            if (IsStringReadonly(jObject)) // Readonly = Mirrored
            {//string is readonly / a mirror field, only use getter
                privateSysName += "Readonly";

                return
                    $"        private string {privateSysName} = string.Empty;\n" +
                    "        /// <summary>\n" +
                    $"        /// mCase data type: {dataType}\n" +
                    "        /// This is a Readonly Mirrored field. No setting / updating allowed\n" +
                    "        /// </summary>\n" +
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

            //string is not readonly, use getter and setter

            return
                $"        private string {privateSysName} = string.Empty;\n" +
                "        /// <summary>\n" +
                $"        /// mCase data type: {dataType}\n" +
                "        /// </summary>\n" +
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
    }
}
