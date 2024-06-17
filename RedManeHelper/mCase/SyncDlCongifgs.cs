using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Text;
using DemoKatan.mCase.Static;
using Newtonsoft.Json.Linq;
using Extensions = DemoKatan.mCase.Static.Extensions;

namespace DemoKatan.mCase
{
    public class SyncDlConfigs
    {
        private readonly string _connectionString;
        private readonly string _sqlCommand;
        private readonly string _outputDirectory;
        private readonly string _exceptionDirectory;
        private readonly string _credentials;
        private readonly string _mCaseUrl;

        public SyncDlConfigs()
        {
            _connectionString = "";
            _sqlCommand = "";
            _outputDirectory = @"";
            _exceptionDirectory = @"";
            _credentials = "";//TODO add credentials username:password
            _mCaseUrl = "";
        }

        public SyncDlConfigs(string[] commandLineArgs)
        {
            if (commandLineArgs.Length != 7)
            {
                Console.WriteLine(commandLineArgs.Length > 7
                    ? $"Too many arguments [{commandLineArgs.Length - 7}]"
                    : $"Missing arguments [{7 - commandLineArgs.Length}]");

                throw new ArgumentException("Invalid params. 1: Connection string, 2: Sql command 3: Credentials, 4: Enviroment Url 5: Output directory, 6: Exception directory");
            }
            // [0] bin dir containing dll and exe files
            _connectionString = commandLineArgs[1];//1
            Console.WriteLine("Connection string: " + _connectionString);

            _sqlCommand = commandLineArgs[2];//2
            Console.WriteLine("Sql Command: " + _sqlCommand);

            _credentials = commandLineArgs[3];//3
            Console.WriteLine("Credentials: " + _credentials);

            _mCaseUrl = commandLineArgs[4] +"/Resource/Export/DataList/Configuration/";//4
            Console.WriteLine("Mcase Url: " + _mCaseUrl);

            _outputDirectory = commandLineArgs[5];//5
            Console.WriteLine("Output Dir: " + _outputDirectory);

            _exceptionDirectory = commandLineArgs[6];//6
            Console.WriteLine("Exception Dir: " + _exceptionDirectory);
        }

        public async Task RemoteSync()
        {
            var sqlResult = await DataAccess();

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
                    var clientResult = await client.GetAsync(new Uri(url));

                    var content = await clientResult.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(content))
                    {
                        var path = await Sync(content);

                        if (!string.IsNullOrEmpty(path))
                            Console.WriteLine("Received data from path: " + path);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Unaccessable API Endpoint: " + url);
                    var path = Path.Combine(_exceptionDirectory, $"Endpoint-{id}-{DateTime.Now.ToString(Extensions.TimeFormat)}.cs");
                    await File.WriteAllTextAsync(path, ex.ToString());
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
            }
        }

        private async Task<List<string>> DataAccess()
        {
            var ids = new List<string>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Execute the query and process results
                    var command = new SqlCommand(_sqlCommand, connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var values = new object[1];

                            // Access data from each row
                            reader.GetValues(values);

                            var value = int.TryParse(values[0].ToString(), out var listId);

                            if (value && listId > 0)
                                ids.Add(listId.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Sql Exception: " + ex);
                var path = Path.Combine(_exceptionDirectory, $"Sql Exception {DateTime.Now.ToString(Extensions.TimeFormat)}.cs");
                await File.WriteAllTextAsync(path, ex.ToString());
                Console.ForegroundColor = ConsoleColor.DarkGray;
                return new List<string>();
            }

            return ids;
        }

        /// <summary>
        /// Using List transfer we can catch the structure of our DL's from the db, and reconstruct a C# object. used for custom events
        /// </summary>
        /// <param name="data"></param>
        private async Task<string> Sync(string data)
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
                await File.WriteAllTextAsync(path, ex.ToString());
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            return string.Empty;
        }

        private StringBuilder GenerateFileData(JObject jsonObject, string className)
        {
            var fieldSet = new HashSet<string>(); //used to track properties being set
            var enumerableFieldSet = new HashSet<string>(); //used to enum properties being set

            var fields = jsonObject[ListTransferFields.Fields.GetDescription()];

            if (fields == null)
                return new StringBuilder();

            var sb = Factory.ClassInitializer(jsonObject, className);//TODO Generate Partial class, Methods in one, Properties in the other

            var requiresEnumeration = false;

            var requiresEnumerationValues = new List<string>
            {
                MCaseTypes.EmbeddedList.GetDescription(),
                MCaseTypes.DropDownList.GetDescription(),
                MCaseTypes.CascadingDropDown.GetDescription(),
                MCaseTypes.DynamicDropDown.GetDescription(),
                MCaseTypes.CascadingDynamicDropDown.GetDescription(),
            };

            var embeddedRelatedFields = new HashSet<string>();

            foreach (var field in fields)
            {
                var type = field.ParseToken(ListTransferFields.Type.GetDescription());

                if (string.IsNullOrEmpty(type)) continue;

                var systemName = field.ParseToken(ListTransferFields.SystemName.GetDescription());

                if (string.IsNullOrEmpty(systemName) || fieldSet.Contains(systemName))
                    continue; //if property is already in field list then continue, no need to duplicate

                if (requiresEnumerationValues.Any(x => string.Equals(x, type, StringComparison.OrdinalIgnoreCase)))
                {
                    enumerableFieldSet.Add(systemName);

                    if (string.Equals(requiresEnumerationValues[0], type, StringComparison.OrdinalIgnoreCase))
                    {
                        embeddedRelatedFields.Add(systemName);
                        fieldSet.Add(systemName);
                        continue;
                    }
                }

                if (requiresEnumerationValues.Contains(type))
                    requiresEnumeration = true;

                var property = AddProperties(field);

                if (string.IsNullOrEmpty(property))
                    continue;

                sb.AppendLine(property);

                fieldSet.Add(systemName);
            }

            if (embeddedRelatedFields.Count > 0)
                sb.AppendLine(Factory.GetEmbeddedOptions(embeddedRelatedFields.ToList()));

            if (requiresEnumeration)
                sb.AppendLine(Factory.AddEnumerableExtensions(className));

            sb.AppendLine(0.Indent() + "}"); //close class

            sb.AppendLine(Factory.GeneratePropertyEnums(enumerableFieldSet, className));

            sb.AppendLine("}"); //close namespace
            return sb;
        }

        private string AddProperties(JToken jToken)
        {
            var type = jToken.ParseToken(ListTransferFields.Type.GetDescription());

            var typeEnum = type.GetEnumValue<MCaseTypes>();

            switch (typeEnum)
            {
                //case mCaseTypes.EmbeddedList: Processed after loop completion
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                case MCaseTypes.DynamicDropDown:
                case MCaseTypes.CascadingDynamicDropDown:
                    return Factory.EnumerableFactory(jToken, typeEnum); //multiselect?
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
                    return Factory.StringFactory(jToken);
                case MCaseTypes.Attachment:
                    return Factory.LongFactory(jToken);
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
