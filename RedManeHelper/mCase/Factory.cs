using System.Reflection.Metadata;
using DemoKatan.mCase.Static;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml.Linq;
using Extensions = DemoKatan.mCase.Static.Extensions;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace DemoKatan.mCase
{
    public static class Factory
    {
        private static List<MCaseTypes> _stringCheck => new() { MCaseTypes.Date, MCaseTypes.Boolean, MCaseTypes.DateTime, MCaseTypes.EmailAddress, MCaseTypes.Number, MCaseTypes.Phone, MCaseTypes.Time, MCaseTypes.URL };

        public static StringBuilder ClassInitializer(JObject jObject, string className, string namespace_)
        {
            var sb = new StringBuilder();

            var id = jObject.ParseJson(ListTransferFields.Id.GetDescription());
            var sysName = jObject.ParseJson(ListTransferFields.SystemName.GetDescription());
            var relationships = jObject[ListTransferFields.Relationships.GetDescription()];//??
            var dtNow = DateTime.Now.ToString(Extensions.MCaseDateTimeStorageFormat);

            sb.AppendLine(//TODO continue to add usings, as more and more validations are made
                "using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing MCaseCustomEvents.ARFocus.DataAccess;\nusing MCaseEventsSDK;\nusing MCaseEventsSDK.Util;\nusing MCaseEventsSDK.Util.Data;\nusing System.ComponentModel;\nusing System.Reflection;");
            sb.AppendLine($"namespace {namespace_}"); //TODO: project specific namespace
            sb.AppendLine("{"); //open namespace

            #region Dl Info Class

            sb.AppendLine(0.Indent() + "/// <summary>");
            sb.AppendLine(0.Indent() + $"/// Synchronized data list [{id}][{sysName}] on {dtNow}");
            sb.AppendLine(0.Indent() + "/// </summary>");
            sb.AppendLine(0.Indent() + $"public class {className}DLInfo");
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.AppendLine(1.Indent() + "private AEventHelper _eventHelper;");
            sb.AppendLine(1.Indent() + $"public {className}DLInfo(AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(1.Indent() + "}"); //close constructor
            sb.AppendLine(1.Indent() + $"public string SystemName => \"{sysName}\";");
            sb.Append(GenerateRelationships(relationships));
            sb.AppendLine(0.Indent() + "}"); //close class

            #endregion
            #region Entity

            sb.AppendLine(0.Indent() + "/// <summary>");
            sb.AppendLine(0.Indent() + $"/// Synchronized data list [{id}][{sysName}] on {dtNow}");
            sb.AppendLine(0.Indent() + "/// </summary>");
            sb.AppendLine(0.Indent() + $"public class {className}Entity");
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.AppendLine(1.Indent() + "private RecordInstanceData _recordInsData;");
            sb.AppendLine(1.Indent() + "private AEventHelper _eventHelper;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Class for Updating Existing RecordInstanceData");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() +
                          $"public {className}Entity(RecordInstanceData recordInsData, AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor
            sb.AppendLine(2.Indent() + "_recordInsData = recordInsData;");
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(1.Indent() + "}"); //close constructor
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Class for Creating New RecordInstanceData");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() +
                          $"public {className}Entity(AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor #Create new Record
            sb.AppendLine(2.Indent() + "_recordInsData = new RecordInstanceData();");
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(1.Indent() + "}"); //close constructor
            sb.AppendLine(1.Indent() + "public long RecordInstanceId => _recordInsData.RecordInstanceID;");
            sb.AppendLine(1.Indent() + "public void SaveRecord()");
            sb.AppendLine(1.Indent() + "{"); //open Method
            sb.AppendLine(2.Indent() + "_eventHelper.SaveRecord(_recordInsData);");
            sb.AppendLine(1.Indent() + "}"); //close Method
            sb.AppendLine(1.Indent() + $"public string SystemName => \"{sysName}\";");
            sb.Append(GenerateRelationships(relationships));

            #endregion

            return sb;
        }

        private static string GenerateRelationships(JToken? relationships)
        {
            //TODO validate new extension does not throw errors
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "private int _dataListId = -1;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Data list identifier is -1 if not found");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "public int DataListId");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + "if(_dataListId != -1) return _dataListId;");
            sb.AppendLine(3.Indent() + "var id = _eventHelper.GetDataListID(SystemName);");
            sb.AppendLine(3.Indent() + "if(id.HasValue && id.Value > 0) _dataListId = id.Value;");
            sb.AppendLine(3.Indent() + "return _dataListId;");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(1.Indent() + "}"); //close Property

            if (relationships == null || !relationships.Any())
                return sb.ToString();

            var parentRelationships =
                relationships.ParseChildren(ListTransferFields.ParentSystemName.GetDescription());

            if (parentRelationships.Any())
            {
                var parentList = parentRelationships.Aggregate(
                    "public enum ParentRelationShips {", (current, child) => current + $"[Description(\"{child}\")] {child},");
                parentList += "};";
                sb.AppendLine(1.Indent() + parentList);
            }
            else
                sb.AppendLine(1.Indent() + "public enum ParentRelationShips {}");

            var childRelationships =
                relationships.ParseChildren(ListTransferFields.ChildSystemName.GetDescription());

            if (childRelationships.Any())
            {
                var childList = childRelationships.Aggregate(
                    "public enum ChildRelationShips {", (current, child) => current + $"[Description(\"{child}\")] {child},");
                childList += "};";
                sb.AppendLine(1.Indent() + childList);
            }
            else
                sb.AppendLine(1.Indent() + "public enum ChildRelationShips {}");

            return sb.ToString();
        }

        public static string EnumerableFactory(JToken jToken, MCaseTypes type, string propertyName, string sysName, string fieldType)
        {
            var sb = new StringBuilder();

            var fieldOptions = jToken.ParseToken(ListTransferFields.FieldOptions.GetDescription());
            var dynamicData = jToken.ParseDynamicData(ListTransferFields.DynamicData.GetDescription(),
                ListTransferFields.DynamicSourceSystemName.GetDescription());

            var privateName = $"_{propertyName.ToLower()}";
            var notAbleToSelectManyValues = fieldOptions.Contains("\"Able to Select Multiple values\"" + ":" + "\"No\"", StringComparison.OrdinalIgnoreCase);
            var multiSelect = notAbleToSelectManyValues ? "False" : "True";

            switch (type)
            {
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                    sb.Append(DropDownFactory(jToken, propertyName, sysName, fieldType, privateName, multiSelect, notAbleToSelectManyValues));
                    break;
                case MCaseTypes.CascadingDynamicDropDown:
                case MCaseTypes.DynamicDropDown:
                    sb.Append(DynamicDropDownFactory(propertyName, sysName, fieldType, privateName, multiSelect, dynamicData, notAbleToSelectManyValues));
                    break;
                default:
                    return string.Empty;
            }

            return sb.ToString();
        }

        private static string DynamicDropDownFactory(string propertyName, string sysName, string fieldType,
            string privateName, string multiSelect, string dynamicData, bool notAbleToSelectManyValues)
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + $"private List<RecordInstanceData> {privateName} = null;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {fieldType}]");
            sb.AppendLine(1.Indent() + $"/// [Multi Select: {multiSelect}]");
            sb.AppendLine(1.Indent() + $"/// [Dynamic Source: {dynamicData}]");
            sb.AppendLine(1.Indent() + "/// [Setting: Requires a RecordInstancesData]");
            sb.AppendLine(1.Indent() + "/// [Getting: Returns the list of RecordInstancesData's]");
            sb.AppendLine(1.Indent() + "/// [Updating: Requires use of either AddTo(), or RemoveFrom()]");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + $"public List<RecordInstanceData> {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if({privateName}!=null) return {privateName};");
            sb.AppendLine(3.Indent() +
                          $"{privateName}=_eventHelper.GetDynamicDropdownRecords(_recordInsData.RecordInstanceID, \"{sysName}\").ToList();");
            sb.AppendLine(3.Indent() + $"return {privateName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{"); //open Setter
            sb.AppendLine(3.Indent() +
                          $"if({privateName} == null || value == null || !value.Any()) {privateName} = new List<RecordInstanceData>();");
            sb.AppendLine(3.Indent() +
                          "if (value != null && value.Any(x => x == null)) value.RemoveAll(x => x == null);");
            sb.AppendLine(3.Indent() +
                          $"if(value == null || !value.Any()) {privateName} = new List<RecordInstanceData>();");
            if (notAbleToSelectManyValues)
                sb.AppendLine(3.Indent() +
                              $"if (value != null && value.Count > 1) throw new Exception(\"[Multi Select is Disabled] {sysName} only accepts a list length of 1.\");");
            sb.AppendLine(3.Indent() + $"else {privateName} = value;");
            sb.AppendLine(3.Indent() +
                          $"_recordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}.Select(x => x.RecordInstanceID)));");
            sb.AppendLine(2.Indent() + "}"); //close Setter
            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb.ToString();
        }

        private static string DropDownFactory(JToken jToken, string propertyName, string sysName, string fieldType,
            string privateName, string multiSelect, bool notAbleToSelectManyValues)
        {
            var sb = new StringBuilder();
            var defaultValues = jToken.ParseDefaultData(ListTransferFields.FieldValues.GetDescription());
            var enumName = $"{propertyName}Enum";
            if (defaultValues.Any())
            {
                sb.AppendLine(GeneratePropertyEnums(defaultValues, propertyName));
            }

            sb.AppendLine(1.Indent() + $"private List<string> {privateName} = null;");
            #region Summary
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {fieldType}]");
            sb.AppendLine(1.Indent() + $"/// [Multi Select: {multiSelect}]");

            if (defaultValues.Any())
            {
                sb.AppendLine(1.Indent() + $"/// [Default list values can be found in {enumName}]");
            }

            sb.AppendLine(1.Indent() +
                          "/// [Setting: Accepts a list of strings such as 'yes' or 'no' added to the original value]");
            sb.AppendLine(1.Indent() + "/// [Getting: Returns the list of field labels]");
            sb.AppendLine(1.Indent() + "/// [Updating: Requires use of either AddTo(), or RemoveFrom()]");
            sb.AppendLine(1.Indent() + "/// </summary>");

            if (defaultValues.Any())
            {
                sb.AppendLine(1.Indent() + $"/// <returns>If value not found in {enumName}. new value will return: [Not found in Default Values] + updated value</returns>");
            }
            #endregion
            sb.AppendLine(1.Indent() + $"public List<string> {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if({privateName} != null) return {privateName};");
            sb.AppendLine(3.Indent() +
                          $"{privateName} = _recordInsData.GetMultiSelectFieldValue(\"{sysName}\");");
            sb.AppendLine(3.Indent() + $"return {privateName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{"); //open Setter
            sb.AppendLine(3.Indent() +
                          $"if({privateName} == null) {privateName} = new List<string>();");
            sb.AppendLine(3.Indent() +
                          "if (value != null && value.Any(string.IsNullOrEmpty)) value.RemoveAll(string.IsNullOrEmpty);");
            sb.AppendLine(3.Indent() +
                          $"if(value == null || !value.Any()) {privateName} = new List<string>();");
            if (notAbleToSelectManyValues)
            {
                sb.AppendLine(3.Indent() +
                              $"if (value != null && value.Count > 1) {privateName} = new List<string>()" + "{\"[Multi Select is Disabled] "+ propertyName + " only accepts a list length of 1.\"};");
            }

            if (defaultValues.Any())
            {
                sb.AppendLine(3.Indent() + "if (value != null && value.Any())");
                sb.AppendLine(3.Indent() + "{// check that all values being set exist in default values enum");//open if
                sb.AppendLine(4.Indent() + $"var defaultOptions = ObjectExtensions.GetDescriptions<{enumName}>();//get all possible default options");
                sb.AppendLine(4.Indent() + "var filteredValues = value.Select(entry => defaultOptions.Contains(entry) ? entry : $\"[Not found in Default Values] {entry}\").ToList();");
                sb.AppendLine(4.Indent() + $"{privateName} = filteredValues;");
                sb.AppendLine(3.Indent() + "}");//close if
                sb.AppendLine(3.Indent() + "else//value could be null or empty. Set to new list");//open single else
                sb.AppendLine(4.Indent() + $"{privateName} = new List<string>();");
            }

            sb.AppendLine(3.Indent() +
                          $"_recordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}));");
            sb.AppendLine(2.Indent() + "}"); //close Setter
            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb.ToString();
        }

        public static string StringFactory(JToken jToken, string propertyName, string sysName, string type)
        {
            var sb = new StringBuilder();
            var enumType = type.GetEnumValue<MCaseTypes>();

            var privateSysName = $"_{propertyName.ToLower()}";
            var mirroredField = jToken.IsMirrorField();

            sb.AppendLine(1.Indent() + $"private string {privateSysName} = string.Empty;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {type}]");
            if (mirroredField)
                sb.AppendLine(1.Indent() + "/// This is a Mirrored field. No setting / updating allowed.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            if (_stringCheck.Contains(enumType) && !mirroredField)
                sb.AppendLine(1.Indent() + "/// <returns>\"-1 if string does not pass mCase data type validation.\"</returns>");
            sb.AppendLine(1.Indent() + $"public string {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(3.Indent() + $"{privateSysName} = _recordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter

            if (!mirroredField)
            {
                //string is not readonly, add setter
                sb.AppendLine(2.Indent() + "set");
                sb.AppendLine(2.Indent() + "{"); //open Setter
                if (_stringCheck.Contains(enumType))
                    sb.Append(AddStringValidations(enumType));
                sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
                sb.AppendLine(3.Indent() + $"_recordInsData.SetValue(\"{sysName}\", {privateSysName});");
                sb.AppendLine(2.Indent() + "}"); //close Setter
            }

            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb.ToString();
        }

        private static string AddStringValidations(MCaseTypes type)
        {
            var sb = new StringBuilder();

            switch (type)
            {
                case MCaseTypes.Date:
                    sb.AppendLine(3.Indent() + "var result = DateTime.TryParse(value, out var dt);");
                    sb.AppendLine(3.Indent() + "if(!result) value = \"-1\";");
                    sb.AppendLine(3.Indent() + "else value = dt.ToString(MCaseEventConstants.DateStorageFormat);");
                    break;
                case MCaseTypes.DateTime:
                    sb.AppendLine(3.Indent() + "var result = DateTime.TryParse(value, out var dt);");
                    sb.AppendLine(3.Indent() + "if(!result) value = \"-1\";");
                    sb.AppendLine(3.Indent() + "else value = dt.ToString(MCaseEventConstants.DateTimeStorageFormat);");
                    break;
                case MCaseTypes.Boolean:
                    sb.AppendLine(3.Indent() + "if(MCaseEventConstants.TrueValues.Contains(value?.Trim().ToLowerInvariant())) value = \"1\";");
                    sb.AppendLine(3.Indent() + "else value = \"0\";");
                    break;
                case MCaseTypes.EmailAddress:
                    sb.AppendLine(3.Indent() + "if(!value.Contains(\"@\")) value = \"-1\";");
                    break;
                case MCaseTypes.Number:
                    sb.AppendLine(3.Indent() + "var isNumeric = long.TryParse(value, out _) || int.TryParse(value, out _) || double.TryParse(value, out _);");
                    sb.AppendLine(3.Indent() + "if(!isNumeric) value = \"-1\";");
                    break;
                case MCaseTypes.Phone:
                    sb.AppendLine(3.Indent() + "var isNumeric = int.TryParse(value, out _);");
                    sb.AppendLine(3.Indent() + "if(!isNumeric) value = \"-1\";");
                    break;
                case MCaseTypes.Time:
                    sb.AppendLine(3.Indent() + "var result = DateTime.TryParse(value, out var dt);");
                    sb.AppendLine(3.Indent() + "if(!result) value = \"-1\";");
                    sb.AppendLine(3.Indent() + "else value = dt.ToString(\"HH:mm\");");
                    break;
                case MCaseTypes.URL:
                    sb.AppendLine(3.Indent() + "if(!value.Contains(\".com\") || !value.Contains(\"https://\")) value = \"-1\";");
                    break;
                default:
                    return string.Empty;
            }
            return sb.ToString();
        }

        public static string LongFactory(JToken jToken, string propertyName, string sysName, string type)
        {
            var sb = new StringBuilder();

            var privateSysName = $"_{propertyName.ToLower()}";

            sb.AppendLine(1.Indent() + $"private string {privateSysName} = string.Empty;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {type}]");
            sb.AppendLine(1.Indent() + "/// Gets value, and sets long value");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + $"public string {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(3.Indent() + $"{privateSysName} = _recordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{"); //open Setter
            sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
            sb.AppendLine(3.Indent() + $"_recordInsData.SetLongValue(\"{sysName}\", {privateSysName});");
            sb.AppendLine(2.Indent() + "}"); //close setter
            sb.AppendLine(1.Indent() + "}"); //close 

            return sb.ToString();
        }

        public static string GetEmbeddedOptions(List<string> data)
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// [mCase data type] Embedded List.");
            sb.AppendLine(1.Indent() + "/// Requires the Datalist Id from one of the following Embedded Data lists:");
            sb.AppendLine(1.Indent() + "/// " + string.Join(", ", data));
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"childDataListId\"> Data list Id</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Related children from selected data list</returns>)");
            sb.AppendLine(1.Indent() + "public List<RecordInstanceData> GetActiveChildRecords(int childDataListId)");
            sb.AppendLine(2.Indent() + "=> _eventHelper");
            sb.AppendLine(3.Indent() + ".GetActiveChildRecordsByParentRecId(_recordInsData.RecordInstanceID)");
            sb.AppendLine(3.Indent() + ".Where(x => x.DataListID == childDataListId)");
            sb.AppendLine(3.Indent() + ".ToList();");

            return sb.ToString();
        }

        /// <summary>
        /// Required because c# has getters and setters  but no updaters. If you add something to a list, it is required to update the internal state
        /// This method allows us to update the internal state property on updation. Essentially, creating an updater. Since we do not
        /// operate directly on the property itself, we aquire the property name using autogenerated enums. Using this we must fetch the class property using
        /// this.GetType() and GetProperty(). Verify that the property was succesfully returned and then we must verify that the return type of the property is the
        /// same as the type passed into the method. This is done by fetching the methodinfo using GetGetMethod().Return type and then comparing that to the incoming
        /// value. If both the property return type, and the argument type are the same we can begin checks for setting the value.
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string AddEnumerableExtensions(string className)
        {
            var sb = new StringBuilder();

            sb.Append(BuildAddMethods(className));

            sb.Append(BuildAddRangeMethods(className));

            sb.Append(BuildRemoveFromMethods(className));

            return sb.ToString();
        }

        private static string BuildAddMethods(string className)
        {
            var sb = new StringBuilder();

            #region Add RecordInstance

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Default null value</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. Null errors: -2. RecordInstance not Created: -5.</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int AddTo({className}Enum propertyEnum, RecordInstanceData value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if (value.RecordInstanceID == 0) return -5;");
            sb.AppendLine(2.Indent() + "var property = propertyEnum.GetEnumDescription();");
            sb.AppendLine(2.Indent() + "var objectType = this.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(this);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "getter.Add(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion
            #region Add String

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\"></param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. Null errors: -2.</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int AddTo({className}Enum propertyEnum, string value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "var property = propertyEnum.GetEnumDescription();");
            sb.AppendLine(2.Indent() + "var objectType = this.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(this);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "getter.Add(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            return sb.ToString();
        }

        private static string BuildAddRangeMethods(string className)
        {
            var sb = new StringBuilder();
            #region AddRange RecordInstance

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
            sb.AppendLine(1.Indent() + "/// List of RecordInstanceData's");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">list of Record Instance Data</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2. RecordInstance not Created: -5</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int AddTo({className}Enum propertyEnum, List<RecordInstanceData> value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if (!value.Any()) return 0;");
            sb.AppendLine(2.Indent() + "if (value.Any(x => x.RecordInstanceID == 0)) return -5;");
            sb.AppendLine(2.Indent() + "var property = propertyEnum.GetEnumDescription();");
            sb.AppendLine(2.Indent() + "var objectType = this.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(this);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "value.RemoveAll(x => x == null);");
            sb.AppendLine(2.Indent() + "if(value.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.AddRange(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(2.Indent() + "return getter.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion
            #region AddRange RecordInstance

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
            sb.AppendLine(1.Indent() + "/// 'List of strings'.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">List of strings</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2.</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int AddTo({className}Enum propertyEnum, List<string> value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if(!value.Any()) return 0;");
            sb.AppendLine(2.Indent() + "var property = propertyEnum.GetEnumDescription();");
            sb.AppendLine(2.Indent() + "var objectType = this.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(this);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "value.RemoveAll(x => x == null);");
            sb.AppendLine(2.Indent() + "if(value.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.AddRange(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(2.Indent() + "return value.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            return sb.ToString();
        }

        private static string BuildRemoveFromMethods(string className)
        {
            var sb = new StringBuilder();
            #region Remove RecordInstance

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Remove all data from enumerable class property that matches predicate");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\"></param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. null errors: -2</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int RemoveFrom({className}Enum propertyEnum, Func<RecordInstanceData, bool> predicate)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (predicate == null) return -2;");
            sb.AppendLine(2.Indent() + "var property = propertyEnum.GetEnumDescription();");
            sb.AppendLine(2.Indent() + "var objectType = this.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(this);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "var foundValues = getter.Where(predicate).ToList();");
            sb.AppendLine(2.Indent() + "if (foundValues.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.RemoveAll(x => foundValues.Contains(x));");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(2.Indent() + "return foundValues.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion
            #region Remove string

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Remove all data from enumerable class property that matches predicate");
            sb.AppendLine(1.Indent() + "/// Example predicate: x => string.IsNullOrEmpty(x)");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\"></param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. null errors: -2</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int RemoveFrom({className}Enum propertyEnum, Func<string, bool> predicate)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (predicate == null) return -2;");
            sb.AppendLine(2.Indent() + "var property = propertyEnum.GetEnumDescription();");
            sb.AppendLine(2.Indent() + "var objectType = this.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var propertyMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (propertyMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(this);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "var foundStrings = getter.Where(predicate).ToList();");
            sb.AppendLine(2.Indent() + "if (foundStrings.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(this, getter.RemoveAll(x => foundStrings.Contains(x)));");
            sb.AppendLine(2.Indent() + "return foundStrings.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            return sb.ToString();
        }

        public static string GeneratePropertyEnums(List<string> fieldSet, string className)
        {
            var sb = new StringBuilder();

            var distinct = fieldSet.Distinct().OrderBy(x => x).ToList();
            var distinctEnums = new List<string>();

            var property = $"public enum {className}Enum" + "{";//open

            for (var i = 0; i < distinct.Count; i++)
            {
                var field = distinct[i];

                if (field.Contains("\""))
                {
                    field = field.Replace("\"", "\\\"");
                }

                property += $"[Description(\"{field}\")] ";

                var enumName = field.GetPropertyNameFromSystemName();

                if (distinctEnums.Contains(enumName))
                {
                    property += enumName + $"_{i}_,";//generate enum duplicate
                }
                else
                {
                    property += enumName + ",";//generate enum 
                    distinctEnums.Add(enumName);
                }
            }

            property += "}";//close enum

            sb.AppendLine(1.Indent() + property);


            return sb.ToString();
        }

        public static string GenerateEnumExtensions()
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "public static List<string> GetDescriptions<TEnum>() where TEnum : Enum =>");
            sb.AppendLine(2.Indent() + "Enum.GetValues(typeof(TEnum))");
            sb.AppendLine(3.Indent() + ".Cast<TEnum>()");
            sb.AppendLine(3.Indent() + ".Select(e => e.GetEnumDescription())");
            sb.AppendLine(3.Indent() + ".ToList();");
            return sb.ToString();
        }

        public static string GenerateStaticFile(string namespace_)
        {

            var sb = new StringBuilder();

            sb.AppendLine(//TODO continue to add usings, as more and more validations are made
                "using System;\nusing System.Collections.Generic;\nusing System.ComponentModel;\nusing System.Linq;\nusing MCaseEventsSDK.Util;\n");
            sb.AppendLine($"namespace {namespace_}"); //TODO: project specific namespace
            sb.AppendLine("{"); //Open class
            sb.AppendLine(0.Indent() + "public static class ObjectExtensions"); //static class
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.Append(GenerateEnumExtensions());
            sb.AppendLine(0.Indent() + "}"); //close class
            sb.AppendLine("}"); //close  class

            return sb.ToString();

        }
    }
}
