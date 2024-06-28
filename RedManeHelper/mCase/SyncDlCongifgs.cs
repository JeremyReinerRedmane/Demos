using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using mCASE_ADMIN.DataAccess.mCase.Static;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Extensions = mCASE_ADMIN.DataAccess.mCase.Static.Extensions;

namespace mCASE_ADMIN.DataAccess.mCase
{
    public class SyncDlConfigs
    {
        private readonly string _csvData = string.Empty;
        private readonly string _connectionString;
        private readonly string _sqlCommand;
        private readonly string _outputDirectory;
        private readonly string _exceptionDirectory;
        private readonly string _credentials;
        private readonly string _mCaseUrl;
        private readonly string _namespace;
        private List<StringBuilder> _stringBuilders;

        public SyncDlConfigs(string[] commandLineArgs)
        {
            if (commandLineArgs.Length != 8 && commandLineArgs.Length != 7)//requires query, or csv
            {
                Console.WriteLine("Invalid Params. There are only two constructors.. Direct Sql query = 8 params. Or Direct Id Requests in CSV Format = 7 params");

                throw new Exception();
            }

            if (commandLineArgs.Length == 8) //Recieving all SQL Id's from specified Db
            {
                Console.WriteLine("System operation: Requesting Data from Db. Setting Parameters");
                // [0] bin dir containing dll and exe files
                _connectionString = commandLineArgs[1];//1
                Console.WriteLine("Connection string: " + _connectionString);

                _sqlCommand = commandLineArgs[2];//2
                Console.WriteLine("Sql Command: " + _sqlCommand);

                _credentials = commandLineArgs[3];//3
                Console.WriteLine("Credentials: " + _credentials);

                _mCaseUrl = commandLineArgs[4] + "/Resource/Export/DataList/Configuration/";//4
                Console.WriteLine("Mcase Url: " + _mCaseUrl);

                _outputDirectory = commandLineArgs[5];//5
                Console.WriteLine("Output Dir: " + _outputDirectory);
                if (!Directory.Exists(_outputDirectory))
                    Directory.CreateDirectory(_outputDirectory);

                _exceptionDirectory = commandLineArgs[6];//6
                Console.WriteLine("Exception Dir: " + _exceptionDirectory);
                if (!Directory.Exists(_exceptionDirectory))
                    Directory.CreateDirectory(_exceptionDirectory);

                _namespace = commandLineArgs[7];//7
                Console.WriteLine("Namespace: " + _namespace);
            }

            if (commandLineArgs.Length == 7)
            {
                Console.WriteLine("System operation: CSV Data. Setting Parameters");
                // [0] bin dir containing dll and exe files
                _csvData = commandLineArgs[1];
                Console.WriteLine("CSV data:" + _csvData);

                _credentials = commandLineArgs[2];
                Console.WriteLine("Credentials: " + _credentials);

                _mCaseUrl = commandLineArgs[3] + "/Resource/Export/DataList/Configuration/";
                Console.WriteLine("Mcase Url: " + _mCaseUrl);

                _outputDirectory = commandLineArgs[4];
                Console.WriteLine("Output Dir: " + _outputDirectory);
                if (!Directory.Exists(_outputDirectory))
                    Directory.CreateDirectory(_outputDirectory);

                _exceptionDirectory = commandLineArgs[5];
                Console.WriteLine("Exception Dir: " + _exceptionDirectory);
                if (!Directory.Exists(_exceptionDirectory))
                    Directory.CreateDirectory(_exceptionDirectory);

                _namespace = commandLineArgs[6];
                Console.WriteLine("Namespace: " + _namespace);
            }

            Console.WriteLine("All parameters have been completed. Moving to next step");
        }

        public void RemoteSync(List<int> sqlResult)
        {
            if (!sqlResult.Any()) return;

            var byteArray = Encoding.ASCII.GetBytes(_credentials);

            var base64String = Convert.ToBase64String(byteArray);

            var authorizationHeader = new AuthenticationHeaderValue("Basic", base64String);

            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = authorizationHeader;

            foreach (var id in sqlResult)
            {
                var url = _mCaseUrl + id;

                Console.WriteLine($"Attempting to reach url: {url}");
                try
                {
                    var clientResult = client.GetAsync(new Uri(url)).Result;

                    var content = clientResult.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(content))
                    {
                        var path = Sync(content);

                        if (!string.IsNullOrEmpty(path))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Received data [{id}]. Sent to location: " + path);
                        }
                    }

                    //TODO: Generate file for dictionaries. Mappers for enum to string, string to enum
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Unaccessable API Endpoint: " + url);
                    var path = Path.Combine(_exceptionDirectory, $"Endpoint-{id}-{DateTime.Now.ToString(Extensions.TimeFormat)}.cs");
                    File.WriteAllText(path, ex.ToString());
                }
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            if (string.IsNullOrEmpty(_csvData))
            {
                var staticFileData = Factory.GenerateStaticFile(_namespace);

                var staticPath = Path.Combine(_outputDirectory, "FactoryExtensions.cs");

                File.WriteAllText(staticPath, staticFileData);
            }
        }

        /// <summary>
        /// required for bat script pipeline app. 
        /// </summary>
        /// <returns></returns>
        public async Task<List<int>> DataAccess()
        {
            if (Directory.Exists(_outputDirectory))
                DeleteAllFiles();

            var ids = new List<int>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Execute the query and process results
                    var command = new SqlCommand(_sqlCommand, connection);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var values = new object[1];

                            // Access data from each row
                            reader.GetValues(values);

                            var value = int.TryParse(values[0].ToString(), out var listId);

                            if (value && listId > 0)
                                ids.Add(listId);
                        }
                    }

                    Console.WriteLine($"Processing {ids.Count} Data lists");
                    return ids.OrderBy(x => x).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Sql Exception: " + ex);
                var path = Path.Combine(_exceptionDirectory, $"Sql Exception {DateTime.Now.ToString(Extensions.TimeFormat)}.cs");
                File.WriteAllText(path, ex.ToString());
                Console.ForegroundColor = ConsoleColor.DarkGray;
                return new List<int>();
            }
        }

        private void DeleteAllFiles()
        {
            var dir = new DirectoryInfo(_outputDirectory);

            if (dir.GetFiles().Length == 0) return;

            var files = dir.GetFiles();

            foreach (var file in files)
            {
                file.Delete();
            }
        }

        public List<int> DirectDataAccess()
        {
            if (string.IsNullOrEmpty(_csvData)) return new List<int>();

            var data = _csvData.Split(',').ToList();

            var nums = new List<int>();
            foreach (var d in data)
            {
                if (int.TryParse(d, out var num))
                    nums.Add(num);
            }

            return nums;
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

            if (string.IsNullOrEmpty(result))
                return string.Empty; // Currently Two dls are empty. Returning from mCase "[]. 1799, & 1805"

            var jsonObject = JObject.Parse(result);

            var className = jsonObject.ParseJson(ListTransferFields.SystemName.GetDescription()).GetPropertyNameFromSystemName();

            if (string.IsNullOrEmpty(className)) return string.Empty;

            try
            {
                var sb = GenerateFileData(jsonObject, className);

                var path = Path.Combine(_outputDirectory, className + ".cs");

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
            var fields = jsonObject[ListTransferFields.Fields.GetDescription()];

            if (fields == null)
                return new StringBuilder();

            var requiresEnumeration = false;
            var hasDateTimes = false;
            var fieldSet = new HashSet<string>(); //used to track properties being set
            var enumerableFieldSet = new HashSet<string>(); //used to enum properties being set
            var embeddedRelatedFields = new HashSet<string>();
            _stringBuilders = new List<StringBuilder>();//used for containing all the enum property values in the class
            var requiredFields = new List<Tuple<string, string>>();
            var requiresEnumerationValues = new List<string>
            {
                MCaseTypes.EmbeddedList.GetDescription(),
                MCaseTypes.DropDownList.GetDescription(),
                MCaseTypes.CascadingDropDown.GetDescription(),
                MCaseTypes.DynamicDropDown.GetDescription(),
                MCaseTypes.CascadingDynamicDropDown.GetDescription(),
            };

            var sb = Factory.ClassInitializer(jsonObject, className, _namespace); // 1: Open namespace / class

            sb.AppendLine(1.Indent() + "#region Fields");
            
            for (var i=0;i<fields.Count();i++)
            {
                var field = fields[i];

                var type = field.ParseToken(ListTransferFields.Type.GetDescription());

                if (string.IsNullOrEmpty(type)) continue;

                var systemName = field.ParseToken(ListTransferFields.SystemName.GetDescription());//All caps

                if (string.IsNullOrEmpty(systemName) || fieldSet.Contains(systemName))
                    continue; //if property is already in field list then continue, no need to duplicate

                if (string.Equals(systemName, className, StringComparison.OrdinalIgnoreCase))
                {
                    systemName = "F_" + systemName + $"_{i}";
                }

                var requiredString = field.ParseToken(ListTransferFields.Required.GetDescription());

                var required = false;

                if (string.Equals("true", requiredString, StringComparison.OrdinalIgnoreCase))
                {
                    required = true;

                    requiredFields.Add(new Tuple<string, string>(type, systemName));
                }

                if (requiresEnumerationValues.Any(x => string.Equals(x, type, StringComparison.OrdinalIgnoreCase)))
                {
                    enumerableFieldSet.Add(systemName);

                    if (string.Equals(requiresEnumerationValues[0], type, StringComparison.OrdinalIgnoreCase))
                    {
                        var fieldName = field.ParseDynamicData(ListTransferFields.DynamicData.GetDescription());

                        if (string.IsNullOrEmpty(fieldName))
                            continue;//weird but true. DL: TDM-Signatures Field: TDMSIGNATURES

                        var entityName = fieldName.GetPropertyNameFromSystemName();

                        embeddedRelatedFields.Add(entityName);

                        fieldSet.Add(systemName);

                        continue;
                    }
                }

                if (requiresEnumerationValues.Contains(type))
                    requiresEnumeration = true;

                var property = AddProperties(field, type, systemName, className, required);// 2: Add properties. Magic happens here

                if (string.IsNullOrEmpty(property))
                    continue;

                sb.AppendLine(property);

                fieldSet.Add(systemName);//completed field, add to list
            }

            sb.AppendLine(1.Indent() + "#endregion Fields");
            if (embeddedRelatedFields.Count > 0)
            {
                sb.AppendLine(Factory.GetEmbeddedOptions(className, embeddedRelatedFields)); // 3: Add class methods
                embeddedRelatedFields.Clear(); // Dispose: no longer required
            }

            if (requiresEnumeration)
                sb.AppendLine(Factory.AddEnumerableExtensions(className, _stringBuilders.Any(), requiredFields)); // 3: Add class methods

            sb.AppendLine(0.Indent() + "}"); // 4: close class

            #region 5: Static File Backing

            sb.AppendLine(0.Indent() + $"public static class {className}Static");

            sb.AppendLine(0.Indent() + "{");//open static class

            if (enumerableFieldSet.Any())//generate our property values
            {
                sb.AppendLine(Factory.GenerateEnums(enumerableFieldSet.ToList(), "Properties_", true).ToString());// All class property names
            }

            sb.AppendLine(Factory.GenerateEnums(fieldSet.ToList(), "SystemNames", false).ToString());// All class property names

            var allDefaultValues = new List<string>() { "Multi Select: False" };

            foreach (var sbs in _stringBuilders)
            {
                var csv = sbs.ToString();

                if (string.IsNullOrEmpty(csv))
                    continue;

                var data = csv.Split(new[] { "$~*@*~$" }, StringSplitOptions.None).Where(x => !string.IsNullOrEmpty(x));

                allDefaultValues.AddRange(data);

                allDefaultValues = allDefaultValues.Distinct().ToList();
            }

            var relationships = jsonObject[ListTransferFields.Relationships.GetDescription()];

            var relationshipEnums = Factory.GenerateRelationships(relationships);

            if (!string.IsNullOrEmpty(relationshipEnums.ToString()))
                sb.Append(relationshipEnums.ToString());

            if (allDefaultValues.Count > 1)
            {
                var distinctData = allDefaultValues.Distinct();

                sb.AppendLine(Factory.GenerateEnums(distinctData.ToList(), "DefaultValues", false).ToString()); //enum adds Enum to name at end
            }

            sb.AppendLine(0.Indent() + "}"); //close static class

            #endregion

            #region 6: Info Class

            sb.Append(Factory.BuildInfoClass(jsonObject, className, requiresEnumeration));

            #endregion

            sb.AppendLine("}"); // 7: close namespace
            return sb;
        }

        private string AddProperties(JToken jToken, string type, string sysName, string className, bool required)//sysname is uppercase
        {
            var typeEnum = type.GetEnumValue<MCaseTypes>();

            var propertyName = sysName.GetPropertyNameFromSystemName();// title case

            switch (typeEnum)
            {
                //case mCaseTypes.EmbeddedList: Processed after loop completion
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                case MCaseTypes.DynamicDropDown:
                case MCaseTypes.CascadingDynamicDropDown:
                    //Item 1 = property, Item 2 = Enum
                    var values = Factory.EnumerableFactory(jToken, typeEnum, propertyName, sysName, type, className, required); //multiselect?
                    
                    if (!string.IsNullOrEmpty(values.Item2.ToString())) //if there are enum values
                        _stringBuilders.Add(values.Item2);
                    
                    return values.Item1;
                case MCaseTypes.String:
                case MCaseTypes.LongString:
                case MCaseTypes.EmailAddress:
                case MCaseTypes.Phone:
                case MCaseTypes.URL:
                case MCaseTypes.Number:
                case MCaseTypes.Money:
                case MCaseTypes.Time:
                case MCaseTypes.Boolean:
                case MCaseTypes.ReadonlyField:
                case MCaseTypes.User:
                case MCaseTypes.Address:
                    return Factory.StringFactory(jToken, propertyName, sysName, type, required);
                case MCaseTypes.Attachment:
                    return Factory.LongFactory(propertyName, sysName, type, required);
                case MCaseTypes.Date:
                case MCaseTypes.DateTime:
                    return Factory.DateFactory(jToken, propertyName, sysName, type, required);
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
    }
}
