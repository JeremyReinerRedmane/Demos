using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using DemoKatan.mCase.Static;
using Newtonsoft.Json.Linq;
using Extensions = DemoKatan.mCase.Static.Extensions;

namespace DemoKatan.mCase
{
    public class SyncDlConfigs
    {
        private string _connectionString = string.Empty;
        private string _sqlCommand = string.Empty;
        private string _outputDirectory = string.Empty;
        private string _exceptionDirectory = string.Empty;
        private string _credentials = string.Empty;
        private string _mCaseUrl = string.Empty;

        public SyncDlConfigs()
        {
            _connectionString = "data source=localhost;initial catalog=mCASE_ADMIN;integrated security=True";
            _sqlCommand = "SELECT [DataListID] FROM [mCASE_ADMIN].[dbo].[DataList]";
            _outputDirectory = @"C:\Users\jreiner\Desktop\FactoryEntities";
            _exceptionDirectory = @"C:\Users\jreiner\Desktop\FactoryEntities\Exceptions";
            _credentials = "admin:Password123!";
            //_mCaseUrl = "http://auusmc-arccwis-app-mcs-qa-r2.redmane-cloud.us/Resource/Export/DataList/Configuration/";
            _mCaseUrl = "http://localhost:64762/Resource/Export/DataList/Configuration/";
        }
        public SyncDlConfigs(string[] commandLineArgs)
        {
            if (commandLineArgs.Length != 8)
            {
                Console.WriteLine($"Entered params = {commandLineArgs.Length}");
                throw new ArgumentException("Invalid params. 1: Connection string, 2: Sql command 3: Credentials, 4: MCase Url 5: Output directory, 6: Exception directory");
            }
            // [0] dll exe path [1] run program.cs
            _connectionString = commandLineArgs[2];//1
            Console.WriteLine("Connection string: " + _connectionString);

            _sqlCommand = commandLineArgs[3];//2
            Console.WriteLine("Sql Command: " + _sqlCommand);

            _credentials = commandLineArgs[4];//3
            Console.WriteLine("Credentials: " + _credentials);

            _mCaseUrl = commandLineArgs[5];//4
            Console.WriteLine("Mcase Url: " + _mCaseUrl);

            _outputDirectory = commandLineArgs[6];//5
            Console.WriteLine("Output Dir: " + _outputDirectory);

            _exceptionDirectory = commandLineArgs[7];//6
            Console.WriteLine("Exception Dir: " + _exceptionDirectory);
        }

        public async Task LocalSync()
        {
            var dirInfo = new DirectoryInfo(_connectionString);

            var files = dirInfo.GetFiles();

            if (!Directory.Exists(_outputDirectory))
                Directory.CreateDirectory(_outputDirectory);

            foreach (var file in files)
            {
                //points to file for data
                var data = await File.ReadAllTextAsync(file.FullName);

                var path = Sync(data);

                if (!string.IsNullOrEmpty(path))
                    Console.WriteLine(path);
            }
        }

        public async Task RemoteSync()
        {
            // 1: query db with connection string and retrieve list of Datalist Id's

            var sqlResult = DataAccess();//return from sql

            // 2: foreach over each id, and use http client to retrieve json using endpoint 'Export/DataList/Configuration/{listIDsString}'

            // Convert the string to byte array using UTF-8 encoding
            var byteArray = Encoding.ASCII.GetBytes(_credentials);

            // Base64 encode the byte array
            var base64String = Convert.ToBase64String(byteArray);

            // Create the Authorization header with Basic scheme
            var authorizationHeader = new AuthenticationHeaderValue("Basic", base64String);

            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = authorizationHeader;

            foreach (var id in sqlResult)
            {
                if (id == "545") 
                    continue;
                var url = _mCaseUrl + id;

                try
                {
                    var clientResult = await client.GetAsync(new Uri(url));

                    var content = await clientResult.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(content))
                    {
                        var path = Sync(content);

                        if (!string.IsNullOrEmpty(path))
                            Console.WriteLine(path);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                }
            }
        }

        private IEnumerable<string> DataAccess()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Execute the query and process results
                var command = new SqlCommand(_sqlCommand, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var values = new object[1];

                        // Access data from each row
                        reader.GetValues(values);

                        var value = int.TryParse(values[0].ToString(), out var listId);

                        if (value && listId > 0)
                            yield return listId.ToString();
                    }
                }
            }
        }


        /// <summary>
        /// Using List transfer we can catch the structure of our DL's from the db, and reconstruct a C# object. used for custom events
        /// </summary>
        /// <param name="data"></param>
        private string Sync(string data)
        {
            var closing = data.LastIndexOf(']');

            var cleanedClose = data.Remove(closing);

            var result = cleanedClose.Remove(0, 1);

            var jsonObject = JObject.Parse(result);

            var className = jsonObject.ParseJson(ListTransferFields.SystemName.GetDescription()).CleanString();

            try
            {
                var sb = GenerateFileData(jsonObject, className);

                var path = Path.Combine(_outputDirectory, $"{className}Entity.cs");

                File.WriteAllText(path, sb.ToString());

                return path;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{className}: " + ex);
                var path = Path.Combine(_exceptionDirectory, $"{className}Entity.cs");
                File.WriteAllText(path, ex.ToString());
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            return string.Empty;
        }

        private StringBuilder GenerateFileData(JObject jsonObject, string className)
        {
            var fieldSet = new HashSet<string>(); //used to track properties being set

            var fields = jsonObject[ListTransferFields.Fields.GetDescription()];

            var sb = ClassInitializer(jsonObject, className);

            var requiresEnumeration = false;

            var requiresEnumerationValues = new List<string>
            {
                MCaseTypes.EmbeddedList.GetDescription(),
                MCaseTypes.DropDownList.GetDescription(),
                MCaseTypes.CascadingDropDown.GetDescription(),
                MCaseTypes.DynamicDropDown.GetDescription(),
                MCaseTypes.CascadingDynamicDropDown.GetDescription()
            };

            var embeddedRelatedFields = new HashSet<string>();

            foreach (var field in fields)
            {
                var type = field.ParseToken(ListTransferFields.Type.GetDescription());

                //Types.Add(type);
                if (string.IsNullOrEmpty(type)) continue;

                var systemName = field.ParseToken(ListTransferFields.SystemName.GetDescription());

                if (string.IsNullOrEmpty(systemName) || fieldSet.Contains(systemName))
                    continue; //if property is already in field list then continue, no need to duplicate

                if (string.Equals(requiresEnumerationValues[0], type, StringComparison.OrdinalIgnoreCase))
                {
                    embeddedRelatedFields.Add(systemName);
                    fieldSet.Add(systemName);
                    continue;
                }

                if (requiresEnumerationValues.Contains(field.Type.GetDescription()))
                    requiresEnumeration = true;

                var property = AddProperties(field);

                if (string.IsNullOrEmpty(property))
                    continue;

                sb.AppendLine(property);

                fieldSet.Add(systemName);
            }

            if (embeddedRelatedFields.Any())
                sb.AppendLine(GetEmbeddedOptions(embeddedRelatedFields.ToList()));

            if (requiresEnumeration)
                sb.AppendLine(AddEnumerableExtensions(className));

            sb.AppendLine(Indent(0) + "}"); //close class

            sb.AppendLine(GeneratePropertyEnums(fieldSet, className));

            sb.AppendLine("}"); //close namespace
            return sb;
        }

        private string GeneratePropertyEnums(HashSet<string> fieldSet, string className)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Indent(0) + $"public enum {className}Enum");
            sb.AppendLine(Indent(0) + "{");

            foreach (var field in fieldSet)
            {
                sb.AppendLine(Indent(1) + $"[Description(\"{field.CleanString()}\")]");
                sb.AppendLine(Indent(1) + $"{field},");

            }

            sb.AppendLine(Indent(0) + "}");

            return sb.ToString();

        }
        private string AddProperties(JToken jToken)
        {
            var type = jToken.ParseToken(ListTransferFields.Type.GetDescription());

            var typeEnum = MCaseStringExtensions.GetEnumValue<MCaseTypes>(type);

            switch (typeEnum)
            {
                //case mCaseTypes.EmbeddedList: Processed after loop completion
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                case MCaseTypes.DynamicDropDown:
                case MCaseTypes.CascadingDynamicDropDown:
                    return EnumerableFactory(jToken, typeEnum); //multiselect?
                case MCaseTypes.String:
                case MCaseTypes.LongString:
                case MCaseTypes.EmailAddress:
                case MCaseTypes.Phone:
                case MCaseTypes.URL:
                case MCaseTypes.Number:
                case MCaseTypes.Money:
                case MCaseTypes.Date:
                case MCaseTypes.DateTime:
                case MCaseTypes.Time:
                case MCaseTypes.Boolean:
                case MCaseTypes.ReadonlyField:
                case MCaseTypes.User:
                case MCaseTypes.Address:
                    return StringFactory(jToken);
                case MCaseTypes.Attachment:
                    return LongFactory(jToken);
                case MCaseTypes.Section: //need in ce's?
                case MCaseTypes.Narrative: //need in ce's?
                case MCaseTypes.Header: //need in ce's?

                case MCaseTypes.UserRoleSecurityRestrict: //not required in CE's
                case MCaseTypes.DynamicCalculatedField: //not required in CE's
                case MCaseTypes.CalculatedField: // not required in CE's 
                case MCaseTypes.UniqueIdentifier: //not required in CE's
                case MCaseTypes.EmbeddedDocument: // blob?
                case MCaseTypes.HiddenField: //not required in CE's
                case MCaseTypes.LineBreak: //not required in CE's
                case MCaseTypes.Position0: //not required in CE's
                case MCaseTypes.Score1: //not required in CE's
                case MCaseTypes.Score2: //not required in CE's
                case MCaseTypes.Score3: //not required in CE's
                case MCaseTypes.Score4: //not required in CE's
                case MCaseTypes.Score5: //not required in CE's
                case MCaseTypes.Score6: //not required in CE's
                default:
                    return string.Empty;
            }
        }

        private static string Indent(int level)
        {
            var dict = new Dictionary<int, string>()
            {
                { 0, "   " },
                { 1, "       " },
                { 2, "           " },
                { 3, "               " },
            };
            return dict[level];
        }

        private static bool IsStringReadonlyOrMirrored(JToken jToken)
        {
            var defaultValue = jToken.ParseToken(ListTransferFields.DefaultValue.GetDescription());

            var isCoalesce = !string.IsNullOrEmpty(defaultValue) && defaultValue.Contains("COALESCE", StringComparison.OrdinalIgnoreCase);

            return isCoalesce;
        }

        private static StringBuilder ClassInitializer(JObject jObject, string className)
        {
            var id = jObject.ParseJson(ListTransferFields.Id.GetDescription());

            var sysName = jObject.ParseJson(ListTransferFields.SystemName.GetDescription());

            var relationships = jObject[ListTransferFields.Relationships.GetDescription()];//??

            var sb = new StringBuilder();

            var dtNow = DateTime.Now.ToString(Extensions.MCaseDateTimeStorageFormat);
            sb.AppendLine(
                "using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing MCaseCustomEvents.ARFocus.DataAccess;\nusing MCaseEventsSDK;\nusing MCaseEventsSDK.Util;\nusing MCaseEventsSDK.Util.Data;\nusing System.ComponentModel;\n");
            sb.AppendLine("namespace MCaseCustomEvents.ARFocus.FactoryEntities"); //TODO: project specific namespace
            sb.AppendLine("{"); //open namespace
            #region Dl Info Class
            sb.AppendLine(Indent(0) + "/// <summary>");
            sb.AppendLine(Indent(0) + $"/// Synchronized data list [{id}][{sysName}] on {dtNow}");
            sb.AppendLine(Indent(0) + "/// </summary>");
            sb.AppendLine(Indent(0) + $"public class {className}DLInfo");
            sb.AppendLine(Indent(0) + "{"); //open class
            sb.AppendLine(Indent(1) + "private AEventHelper _eventHelper;");
            sb.AppendLine(Indent(1) + $"public {className}DLInfo(AEventHelper eventHelper)");
            sb.AppendLine(Indent(1) + "{"); //open constructor
            sb.AppendLine(Indent(2) + "_eventHelper = eventHelper;");
            sb.AppendLine(Indent(1) + "}"); //close constructor
            sb.AppendLine(Indent(1) + $"public string SystemName => \"{sysName}\";");
            sb.AppendLine(Indent(1) + "private int _dataListId = -1;");
            sb.AppendLine(Indent(1) + "public int DataListId");
            sb.AppendLine(Indent(1) + "{"); //open Property
            sb.AppendLine(Indent(2) + "get");
            sb.AppendLine(Indent(2) + "{"); //open Getter
            sb.AppendLine(Indent(3) + "if(_dataListId != -1) return _dataListId;");
            sb.AppendLine(Indent(3) + "var id = _eventHelper.GetDataListID(SystemName);");
            sb.AppendLine(Indent(3) + "if(id.HasValue && id.Value > 0) _dataListId = id.Value;");
            sb.AppendLine(Indent(3) + "return _dataListId;");
            sb.AppendLine(Indent(2) + "}"); //close Getter
            sb.AppendLine(Indent(1) + "}"); //close Property
            if (relationships != null && relationships.Any())
            {
                //var parentRelationships = transfer.Relationships.Where(x => !string.IsNullOrEmpty(x.ParentSystemName)).Select(x => x.ParentSystemName).ToList();
                var parentRelationships =
                    relationships.ParseChildren(ListTransferFields.ParentSystemName.GetDescription());

                if (parentRelationships.Any())
                {
                    var parentList = parentRelationships.Aggregate(
                        "public List<string> ParentRelationShips => new List<string>() {", (current, child) => current + $"\"{child}\",");
                    parentList += "};";
                    sb.AppendLine(Indent(1) + parentList);
                }

                var childRelationships =
                    relationships.ParseChildren(ListTransferFields.ChildSystemName.GetDescription());

                if (childRelationships.Any())
                {
                    var childList = childRelationships.Aggregate(
                        "public List<string> ChildRelationShips => new List<string>() {",
                        (current, child) => current + $"\"{child}\",");
                    childList += "};";
                    sb.AppendLine(Indent(1) + childList);
                }
            }

            sb.AppendLine(Indent(0) + "}"); //close class

            #endregion

            #region Entity

            sb.AppendLine(Indent(0) + "/// <summary>");
            sb.AppendLine(Indent(0) + $"/// Synchronized data list [{id}][{sysName}] on {dtNow}");
            sb.AppendLine(Indent(0) + "/// </summary>");
            sb.AppendLine(Indent(0) + $"public class {className}Entity");
            sb.AppendLine(Indent(0) + "{"); //open class
            sb.AppendLine(Indent(1) + "private RecordInstanceData _recordInsData;");
            sb.AppendLine(Indent(1) + "private AEventHelper _eventHelper;");
            sb.AppendLine(Indent(1) +
                          $"public {className}Entity(RecordInstanceData recordInsData, AEventHelper eventHelper)");
            sb.AppendLine(Indent(1) + "{"); //open constructor
            sb.AppendLine(Indent(2) + "_recordInsData = recordInsData;");
            sb.AppendLine(Indent(2) + "_eventHelper = eventHelper;");
            sb.AppendLine(Indent(1) + "}"); //close constructor
            sb.AppendLine(Indent(1) + $"public string SystemName => \"{sysName}\";");
            sb.AppendLine(Indent(1) + "public long RecordInstanceId => _recordInsData.RecordInstanceID;");
            sb.AppendLine(Indent(1) + "public void SaveRecord()");
            sb.AppendLine(Indent(1) + "{"); //open Method
            sb.AppendLine(Indent(2) + "_eventHelper.SaveRecord(_recordInsData);");
            sb.AppendLine(Indent(1) + "}"); //close Method
            sb.AppendLine(Indent(1) + "private int _dataListId = -1;");
            sb.AppendLine(Indent(1) + "/// <summary>");
            sb.AppendLine(Indent(1) + "/// Data list identifier is -1 if not found");
            sb.AppendLine(Indent(1) + "/// </summary>");
            sb.AppendLine(Indent(1) + "public int DataListId");
            sb.AppendLine(Indent(1) + "{"); //open Property
            sb.AppendLine(Indent(2) + "get");
            sb.AppendLine(Indent(2) + "{"); //open Getter
            sb.AppendLine(Indent(3) + "if(_dataListId != -1) return _dataListId;");
            sb.AppendLine(Indent(3) + "var id = _eventHelper.GetDataListID(SystemName);");
            sb.AppendLine(Indent(3) + "if(id.HasValue && id.Value > 0) _dataListId = id.Value;");
            sb.AppendLine(Indent(3) + "return _dataListId;");
            sb.AppendLine(Indent(2) + "}"); //close Setter
            sb.AppendLine(Indent(1) + "}"); //close Property

            #endregion

            return sb;
        }

        private static string LongFactory(JToken jToken)
        {
            var sb = new StringBuilder();

            var sysName = jToken.ParseToken(ListTransferFields.SystemName.GetDescription());
            var type = jToken.ParseToken(ListTransferFields.Type.GetDescription());

            var privateSysName = $"_{sysName.ToLower()}";

            sb.AppendLine(Indent(1) + $"private string {privateSysName} = string.Empty;");
            sb.AppendLine(Indent(1) + "/// <summary>");
            sb.AppendLine(Indent(1) + $"/// [mCase data type: {type}]");
            sb.AppendLine(Indent(1) + "/// Gets value, and sets long value");
            sb.AppendLine(Indent(1) + "/// </summary>");
            sb.AppendLine(Indent(1) + $"public string {sysName}");
            sb.AppendLine(Indent(1) + "{"); //open Property
            sb.AppendLine(Indent(2) + "get");
            sb.AppendLine(Indent(2) + "{"); //open Getter
            sb.AppendLine(Indent(3) + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(Indent(3) + $"{privateSysName} = _recordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(Indent(3) + $"return {privateSysName};");
            sb.AppendLine(Indent(2) + "}"); //close Getter
            sb.AppendLine(Indent(2) + "set");
            sb.AppendLine(Indent(2) + "{"); //open Setter
            sb.AppendLine(Indent(3) + $"{privateSysName} = value;");
            sb.AppendLine(Indent(3) + $"_recordInsData.SetLongValue(\"{sysName}\", {privateSysName});");
            sb.AppendLine(Indent(2) + "}"); //close setter
            sb.AppendLine(Indent(1) + "}"); //close 

            return sb.ToString();
        }

        /// <summary>
        /// Generate factory for entering into mCase db. This process should feel as simple as updating an object. The getters and setters
        /// Use mCase OOTB generations for CRUD operations. Find the similarities for CRUD and update the properties here
        /// </summary>
        /// <returns></returns>
        private string StringFactory(JToken jToken)
        {
            var sb = new StringBuilder();
            var sysName = jToken.ParseToken(ListTransferFields.SystemName.GetDescription()); //title case
            var type = jToken.ParseToken(ListTransferFields.Type.GetDescription()); //title case
            var privateSysName = $"_{sysName.ToLower()}";

            var readonlyOrMirrored = IsStringReadonlyOrMirrored(jToken);

            sb.AppendLine(Indent(1) + $"private string {privateSysName} = string.Empty;");
            sb.AppendLine(Indent(1) + "/// <summary>");
            sb.AppendLine(Indent(1) + $"/// [mCase data type: {type}]");
            if (readonlyOrMirrored)
                sb.AppendLine(Indent(1) + "/// This is a Readonly / Mirrored field. No setting / updating allowed.");
            sb.AppendLine(Indent(1) + "/// </summary>");
            sb.AppendLine(Indent(1) + $"public string {sysName}");
            sb.AppendLine(Indent(1) + "{"); //open Property
            sb.AppendLine(Indent(2) + "get");
            sb.AppendLine(Indent(2) + "{"); //open Getter
            sb.AppendLine(Indent(3) + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(Indent(3) + $"{privateSysName} = _recordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(Indent(3) + $"return {privateSysName};");
            sb.AppendLine(Indent(2) + "}"); //close Getter

            if (!readonlyOrMirrored)
            {
                //string is not readonly, add setter
                sb.AppendLine(Indent(2) + "set");
                sb.AppendLine(Indent(2) + "{"); //open Setter
                sb.AppendLine(Indent(3) + $"{privateSysName} = value;");
                sb.AppendLine(Indent(3) + $"_recordInsData.SetValue(\"{sysName}\", {privateSysName});");
                sb.AppendLine(Indent(2) + "}"); //close Setter
            }

            sb.AppendLine(Indent(1) + "}"); //close Property

            return sb.ToString();
        }

        /// <summary>
        /// Factory generator for enumerable mCase data structures.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string EnumerableFactory(JToken jToken, MCaseTypes type)
        {
            var sb = new StringBuilder();

            var sysName = jToken.ParseToken(ListTransferFields.SystemName.GetDescription());
            var fieldOptions = jToken.ParseToken(ListTransferFields.FieldOptions.GetDescription());
            var fieldType = jToken.ParseToken(ListTransferFields.Type.GetDescription());
            var dynamicData = jToken.ParseDynamicData(ListTransferFields.DynamicData.GetDescription(),
                ListTransferFields.DynamicSourceSystemName.GetDescription());

            var privateName = $"_{sysName.ToLower()}";
            var notAbleToSelectManyValues = fieldOptions.Contains("\"Able to Select Multiple values\"" + ":" + "\"No\"", StringComparison.OrdinalIgnoreCase);

            var multiSelect = notAbleToSelectManyValues ? "False" : "True";
            switch (type)
            {
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                    sb.AppendLine(Indent(1) + $"private List<string> {privateName} = null;");
                    sb.AppendLine(Indent(1) + "/// <summary>");
                    sb.AppendLine(Indent(1) + $"/// [mCase data type: {fieldType}]");
                    sb.AppendLine(Indent(1) + $"/// [Multi Select: {multiSelect}]");
                    sb.AppendLine(Indent(1) +
                                  "/// [Setting: Accepts a list of strings such as 'yes' or 'no' added to the original value]");
                    sb.AppendLine(Indent(1) + "/// [Getting: Returns the list of field labels]");
                    sb.AppendLine(Indent(1) + "/// </summary>");
                    sb.AppendLine(Indent(1) + $"public List<string> {sysName}");
                    sb.AppendLine(Indent(1) + "{"); //open Property
                    sb.AppendLine(Indent(2) + "get");
                    sb.AppendLine(Indent(2) + "{"); //open Getter
                    sb.AppendLine(Indent(3) + $"if({privateName} != null) return {privateName};");
                    sb.AppendLine(Indent(3) +
                                  $"{privateName} = _recordInsData.GetMultiSelectFieldValue(\"{sysName}\");");
                    sb.AppendLine(Indent(3) + $"return {privateName};");
                    sb.AppendLine(Indent(2) + "}"); //close Getter
                    sb.AppendLine(Indent(2) + "set");
                    sb.AppendLine(Indent(2) + "{"); //open Setter
                    if (notAbleToSelectManyValues)
                        sb.AppendLine(Indent(3) +
                                      $"if (value != null && value.Count > 1) throw new Exception(\"[Multi Select is Disabled] {sysName} only accepts a list length of 1.\");");
                    sb.AppendLine(Indent(3) +
                                  $"_recordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, value));");
                    sb.AppendLine(Indent(2) + "}"); //close Setter
                    sb.AppendLine(Indent(1) + "}"); //close Property
                    break;
                case MCaseTypes.CascadingDynamicDropDown:
                case MCaseTypes.DynamicDropDown:
                    sb.AppendLine(Indent(1) + $"private List<RecordInstanceData> {privateName} = null;");
                    sb.AppendLine(Indent(1) + "/// <summary>");
                    sb.AppendLine(Indent(1) + $"/// [mCase data type: {fieldType}]");
                    sb.AppendLine(Indent(1) + $"/// [Multi Select: {multiSelect}]");
                    sb.AppendLine(Indent(1) + $"/// [Dynamic Source: {dynamicData}]");
                    sb.AppendLine(Indent(1) + "/// [Setting: Requires the RecordInstanceID]");
                    sb.AppendLine(Indent(1) + "/// [Getting: Returns the list of RecordInstancesData's]");
                    sb.AppendLine(Indent(1) + "/// </summary>");
                    sb.AppendLine(Indent(1) + $"public List<RecordInstanceData> {sysName}");
                    sb.AppendLine(Indent(1) + "{"); //open Property
                    sb.AppendLine(Indent(2) + "get");
                    sb.AppendLine(Indent(2) + "{"); //open Getter
                    sb.AppendLine(Indent(3) + $"if({privateName}!=null) return {privateName};");
                    sb.AppendLine(Indent(3) +
                                  $"{privateName}=_eventHelper.GetDynamicDropdownRecords(_recordInsData.RecordInstanceID, \"{sysName}\").ToList();");
                    sb.AppendLine(Indent(3) + $"return {privateName};");
                    sb.AppendLine(Indent(2) + "}"); //close Getter
                    sb.AppendLine(Indent(2) + "set");
                    sb.AppendLine(Indent(2) + "{"); //open Setter
                    sb.AppendLine(Indent(3) +
                                  $"if({privateName} == null || value == null || !value.Any()) {privateName} = new List<RecordInstanceData>();");
                    sb.AppendLine(Indent(3) +
                                  "if (value != null && value.Any(x => x == null)) value.RemoveAll(x => x == null);");
                    sb.AppendLine(Indent(3) +
                                  $"if(value == null || !value.Any()) {privateName} = new List<RecordInstanceData>();");
                    if (notAbleToSelectManyValues)
                        sb.AppendLine(Indent(3) +
                                      $"if (value != null && value.Count > 1) throw new Exception(\"[Multi Select is Disabled] {sysName} only accepts a list length of 1.\");");
                    sb.AppendLine(Indent(3) + $"else {privateName} = value;");
                    sb.AppendLine(Indent(3) +
                                  $"_recordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}.Select(x => x.RecordInstanceID)));");
                    sb.AppendLine(Indent(2) + "}"); //close Setter
                    sb.AppendLine(Indent(1) + "}"); //close Property
                    break;
                default:
                    return string.Empty;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Handles processing for Embedded lists
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string GetEmbeddedOptions(List<string> data)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Indent(1) + "/// <summary>");
            sb.AppendLine(Indent(1) + "/// [mCase data type] Embedded List.");
            sb.AppendLine(Indent(1) + "/// Requires the Datalist Id from one of the following Embedded Data lists:");
            sb.AppendLine(Indent(1) + "/// " + string.Join(", ", data));
            sb.AppendLine(Indent(1) + "/// </summary>");
            sb.AppendLine(Indent(1) + "/// <param name=\"childDataListId\"> Data list Id</param>");
            sb.AppendLine(Indent(1) + "/// <returns>Related children from selected data list</returns>)");
            sb.AppendLine(Indent(1) + "public List<RecordInstanceData> GetActiveChildRecords(int childDataListId)");
            sb.AppendLine(Indent(2) + "=> _eventHelper");
            sb.AppendLine(Indent(3) + ".GetActiveChildRecordsByParentRecId(_recordInsData.RecordInstanceID)");
            sb.AppendLine(Indent(3) + ".Where(x => x.DataListID == childDataListId)");
            sb.AppendLine(Indent(3) + ".ToList();");

            return sb.ToString();
        }

        private static string AddEnumerableExtensions(string className)
        {
            var sb = new StringBuilder();

            #region Add

            sb.AppendLine(Indent(1) + "/// <summary>");
            sb.AppendLine(Indent(1) +
                          $"/// Use {className}Enum to find Property to update, and Either string value or recordInstance value.");
            sb.AppendLine(Indent(1) + "/// If both are entered no update will occur. returning value of 0.");
            sb.AppendLine(Indent(1) + "/// </summary>");
            sb.AppendLine(Indent(1) + $"/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(Indent(1) + "/// <param name=\"value\">Default null value</param>");
            sb.AppendLine(Indent(1) + "/// <param name=\"stringValue\">Default null value</param>");
            sb.AppendLine(Indent(1) + "/// <returns>Amount of values added</returns>");
            sb.AppendLine(Indent(1) +
                          $"public int AddTo({className}Enum propertyEnum, RecordInstanceData value= null, string stringValue = null)");
            sb.AppendLine(Indent(1) + "{"); //open method
            sb.AppendLine(Indent(2) + "var property = propertyEnum.GetDescription();");
            sb.AppendLine(Indent(2) + "if (value != null && !string.IsNullOrEmpty(stringValue)) return 0;");
            sb.AppendLine(Indent(2) + "var objectType = this.GetType();");
            sb.AppendLine(Indent(2) + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(Indent(2) + "if (propertyInfo == null) return 0;");
            sb.AppendLine(Indent(2) + "if (value != null)");
            sb.AppendLine(Indent(2) + "{"); //open if
            sb.AppendLine(Indent(3) + "if (value.RecordInstanceID == 0) return 0;");
            sb.AppendLine(Indent(3) + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(this);");
            sb.AppendLine(Indent(3) +
                          "if (getter != null && getter.Any(r => r.RecordInstanceID != value.RecordInstanceID)) getter.Add(value);");
            sb.AppendLine(Indent(3) + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(Indent(3) + "return 1;");
            sb.AppendLine(Indent(2) + "}"); //close if
            sb.AppendLine(Indent(2) + "if(string.IsNullOrEmpty(stringValue)) return 0;");
            sb.AppendLine(Indent(2) + "var stringGetter = (List<string>)propertyInfo.GetValue(this);");
            sb.AppendLine(Indent(2) +
                          "if(stringGetter !=null && stringGetter.Any(r => string.Equals(r, stringValue, StringComparison.OrdinalIgnoreCase))) stringGetter.Add(stringValue);");
            sb.AppendLine(Indent(2) + "propertyInfo.SetValue(this, stringGetter);");
            sb.AppendLine(Indent(2) + "return 1;");
            sb.AppendLine(Indent(1) + "}"); //close method

            #endregion

            #region Remove

            sb.AppendLine(Indent(1) + "/// <summary>");
            sb.AppendLine(Indent(1) +
                          $"/// Use {className}Enum to find Property to update, and Either string value or recordInstance value.");
            sb.AppendLine(Indent(1) + "/// If both are entered no update will occur. returning value of 0.");
            sb.AppendLine(Indent(1) + "/// </summary>");
            sb.AppendLine(Indent(1) + $"/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(Indent(1) + "/// <param name=\"value\">Default null value</param>");
            sb.AppendLine(Indent(1) + "/// <param name=\"stringValue\">Default null value</param>");
            sb.AppendLine(Indent(1) + "/// <returns>Amount of values removed</returns>");
            sb.AppendLine(Indent(1) +
                          $"public int RemoveFrom({className}Enum propertyEnum, RecordInstanceData value= null, string stringValue = null)");
            sb.AppendLine(Indent(1) + "{"); //open method
            sb.AppendLine(Indent(2) + "var property = propertyEnum.GetDescription();");
            sb.AppendLine(Indent(2) + "if (value != null && !string.IsNullOrEmpty(stringValue)) return 0;");
            sb.AppendLine(Indent(2) + "var objectType = this.GetType();");
            sb.AppendLine(Indent(2) + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(Indent(2) + "if (propertyInfo == null) return 0;");
            sb.AppendLine(Indent(2) + "if (value != null)");
            sb.AppendLine(Indent(2) + "{"); //open if
            sb.AppendLine(Indent(3) + "if (value.RecordInstanceID == 0) return 0;");
            sb.AppendLine(Indent(3) + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(this);");
            sb.AppendLine(Indent(3) + "if (getter == null) return 0;");
            sb.AppendLine(Indent(3) + "var count = getter.Count(x => x.RecordInstanceID == value.RecordInstanceID);");
            sb.AppendLine(Indent(3) + "if (count < 1) return 0;");
            sb.AppendLine(Indent(3) + "getter.RemoveAll(x => x.RecordInstanceID == value.RecordInstanceID);");
            sb.AppendLine(Indent(3) + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(Indent(3) + "return count;");
            sb.AppendLine(Indent(2) + "}"); //close if
            sb.AppendLine(Indent(2) + "if(string.IsNullOrEmpty(stringValue)) return 0;");
            sb.AppendLine(Indent(2) + "var stringGetter = (List<string>)propertyInfo.GetValue(this);");
            sb.AppendLine(Indent(2) + "if(stringGetter == null) return 0;");
            sb.AppendLine(Indent(2) +
                          "var stringCount = stringGetter.Count(x => string.Equals(x, stringValue, StringComparison.OrdinalIgnoreCase));");
            sb.AppendLine(Indent(2) + "if (stringCount < 1) return 0;");
            sb.AppendLine(Indent(2) +
                          "propertyInfo.SetValue(this, stringGetter.RemoveAll(x => string.Equals(x, stringValue, StringComparison.OrdinalIgnoreCase)));");
            sb.AppendLine(Indent(2) + "return stringCount;");
            sb.AppendLine(Indent(1) + "}"); //close method

            #endregion

            return sb.ToString();

        }
    }

}
