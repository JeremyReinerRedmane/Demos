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
            _connectionString = ";
            _sqlCommand = "";
            _outputDirectory = "";
            _exceptionDirectory = "";
            _credentials = "";//TODO add credentials username:password
            _mCaseUrl = "";
        }
        public SyncDlConfigs(string[] commandLineArgs)
        {
            if (commandLineArgs.Length != 8)
            {
                Console.WriteLine(commandLineArgs.Length > 8
                    ? $"Too many parameters [{commandLineArgs.Length - 8}]"
                    : $"Missing parameters [{8 - commandLineArgs.Length}]");

                throw new ArgumentException("Invalid params. 1: Connection string, 2: Sql command 3: Credentials, 4: Enviroment Url 5: Output directory, 6: Exception directory");
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

        public async Task RemoteSync()
        {
            var sqlResult = DataAccess();

            var byteArray = Encoding.ASCII.GetBytes(_credentials);

            var base64String = Convert.ToBase64String(byteArray);

            var authorizationHeader = new AuthenticationHeaderValue("Basic", base64String);

            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = authorizationHeader;

            foreach (var id in sqlResult)
            {
                if (id == "545")//Id for todo dl
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
            using (var connection = new SqlConnection(_connectionString))
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

            if (fields == null)
                return new StringBuilder();

            var sb = Factory.ClassInitializer(jsonObject, className);

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

                if (string.Equals(requiresEnumerationValues[0], type, StringComparison.OrdinalIgnoreCase))
                {
                    embeddedRelatedFields.Add(systemName);
                    fieldSet.Add(systemName);
                    continue;
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

            sb.AppendLine(Factory.GeneratePropertyEnums(fieldSet, className));

            sb.AppendLine("}"); //close namespace
            return sb;
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
