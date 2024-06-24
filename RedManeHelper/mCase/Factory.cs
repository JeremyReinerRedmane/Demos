using DemoKatan.mCase.Static;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Extensions = DemoKatan.mCase.Static.Extensions;

namespace DemoKatan.mCase
{
    public static class Factory
    {
        private static List<MCaseTypes> _stringCheck => new()
        {
            MCaseTypes.EmailAddress, MCaseTypes.Number,
            MCaseTypes.Phone, MCaseTypes.Time, MCaseTypes.URL
        };

        public static StringBuilder ClassInitializer(JObject jObject, string className, string namespace_)
        {
            var sb = new StringBuilder();

            var id = jObject.ParseJson(ListTransferFields.Id.GetDescription());
            var sysName = jObject.ParseJson(ListTransferFields.SystemName.GetDescription());
            var dtNow = DateTime.Now.ToString(Extensions.MCaseDateTimeStorageFormat);

            sb.AppendLine( //TODO continue to add usings, as more and more validations are made
                "using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing MCaseCustomEvents.ARFocus.DataAccess;\nusing MCaseEventsSDK;\nusing MCaseEventsSDK.Util;\nusing MCaseEventsSDK.Util.Data;\nusing System.ComponentModel;\nusing System.Reflection;");
            sb.AppendLine($"namespace {namespace_}"); //TODO: Always update project specific namespace
            sb.AppendLine("{"); //open namespace

            #region Dl Info Class

            sb.AppendLine(0.Indent() + "/// <summary>");
            sb.AppendLine(0.Indent() + $"/// Synchronized data list [{id}][{sysName}] on {dtNow}");
            sb.AppendLine(0.Indent() + "/// </summary>");
            sb.AppendLine(0.Indent() + $"public class {className}DlInfo");
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.AppendLine(1.Indent() + "private AEventHelper _eventHelper;");
            sb.AppendLine(1.Indent() + $"public {className}DlInfo(AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(1.Indent() + "}"); //close constructor
            sb.AppendLine(1.Indent() + $"public string SystemName => \"{sysName}\";");
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
            sb.AppendLine(0.Indent() + "}"); //close class

            #endregion

            #region Entity

            sb.AppendLine(0.Indent() + "/// <summary>");
            sb.AppendLine(0.Indent() + $"/// Synchronized data list [{id}][{sysName}] on {dtNow}");
            sb.AppendLine(0.Indent() + "/// </summary>");
            sb.AppendLine(0.Indent() + $"public class {className}Entity");
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.AppendLine(1.Indent() + "public RecordInstanceData RecordInsData;");
            sb.AppendLine(1.Indent() + "private readonly AEventHelper _eventHelper;");

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Class for Updating Existing RecordInstanceData. To create a new REcordInstance data, initialize with the appropriate datalist ID.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() +
                          $"public {className}Entity(RecordInstanceData recordInsData, AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor #Existing Record Instance Data
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(2.Indent() + $"if (recordInsData.DataListID != DataListId) throw new Exception(\"RecordInstance is not of type {className}Entity\");");
            sb.AppendLine(2.Indent() + "RecordInsData = recordInsData;");
            sb.AppendLine(1.Indent() + "}"); //close constructor

            sb.AppendLine(1.Indent() + $"public string SystemName => \"{sysName}\";");
            sb.AppendLine(1.Indent() + "private int _dataListId = -1;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Data list identifier is -1 if not found");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "public int DataListId");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + "if(_dataListId>0) return _dataListId;");
            sb.AppendLine(3.Indent() + "var id=_eventHelper.GetDataListID(SystemName);");
            sb.AppendLine(3.Indent() + "if(id.HasValue&&id.Value>0)_dataListId=id.Value;");
            sb.AppendLine(3.Indent() + "return _dataListId;");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(1.Indent() + "}"); //close Property
            sb.AppendLine(1.Indent() + "public void SaveRecord() =>_eventHelper.SaveRecord(RecordInsData);");
            sb.AppendLine(1.Indent() + $"public void LogDebug(string log) => _eventHelper.AddDebugLog($\"[{className}Entity Record][{{RecordInsData.RecordInstanceID}}]: {{log}}\");");
            sb.AppendLine(1.Indent() + $"public void LogInfo(string log) => _eventHelper.AddInfoLog($\"[{className}Entity Record][{{RecordInsData.RecordInstanceID}}]: {{log}}\");");
            sb.AppendLine(1.Indent() + $"public void LogWarning(string log) => _eventHelper.AddWarningLog($\"[{className}Entity Record][{{RecordInsData.RecordInstanceID}}]: {{log}}\");");
            sb.AppendLine(1.Indent() + $"public void LogError(string log) => _eventHelper.AddErrorLog($\"[{className}Entity Record][{{RecordInsData.RecordInstanceID}}]: {{log}}\");");


            #endregion

            return sb;
        }

        #region Factories

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
                sb.AppendLine(1.Indent() +
                              "/// <returns>\"-1 if string does not pass mCase data type validation.\"</returns>");
            sb.AppendLine(1.Indent() + $"public string {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(3.Indent() + $"{privateSysName} = RecordInsData.GetFieldValue(\"{sysName}\");");
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
                sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{sysName}\", {privateSysName});");
                sb.AppendLine(2.Indent() + "}"); //close Setter
            }

            sb.AppendLine(1.Indent() + "}"); //close Property

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
            sb.AppendLine(3.Indent() + $"{privateSysName} = RecordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{"); //open Setter
            sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
            sb.AppendLine(3.Indent() + $"RecordInsData.SetLongValue(\"{sysName}\", {privateSysName});");
            sb.AppendLine(2.Indent() + "}"); //close setter
            sb.AppendLine(1.Indent() + "}"); //close 

            return sb.ToString();
        }

        public static string DateFactory(JToken jToken, string propertyName, string sysName, string type)
        {
            var sb = new StringBuilder();
            var enumType = type.GetEnumValue<MCaseTypes>();

            var privateSysName = $"_{propertyName.ToLower()}";
            var mirroredField = jToken.IsMirrorField();
            sb.AppendLine(1.Indent() + $"private DateTime {privateSysName} = DateTime.MinValue;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {type}]");
            if (mirroredField)
                sb.AppendLine(1.Indent() + "/// This is a Mirrored field. No setting / updating allowed.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            if (_stringCheck.Contains(enumType) && !mirroredField)
                sb.AppendLine(1.Indent() +
                              "/// <returns>\"-1 if Datetime does not pass mCase data type validation.\"</returns>");
            sb.AppendLine(1.Indent() + $"public DateTime {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if({privateSysName} != DateTime.MinValue) return {privateSysName};");
            sb.AppendLine(3.Indent() + $"{privateSysName} = DateTime.TryParse(RecordInsData.GetFieldValue(\""+ sysName +"\"), out var dt) ? dt : DateTime.MinValue;");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter

            if (!mirroredField)
            {
                var setter = enumType == MCaseTypes.Date //only gets here if Date or DateTime
                    ? ".ToString(MCaseEventConstants.DateStorageFormat)"
                    : ".ToString(MCaseEventConstants.DateTimeStorageFormat)";

                //string is not readonly, add setter
                sb.AppendLine(2.Indent() + "set");
                sb.AppendLine(2.Indent() + "{"); //open Setter
                sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
                sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{sysName}\", {privateSysName}{setter});");
                sb.AppendLine(2.Indent() + "}"); //close Setter
            }

            sb.AppendLine(1.Indent() + "}"); //close Property

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
                          $"{privateName}=_eventHelper.GetDynamicDropdownRecords(RecordInsData.RecordInstanceID, \"{sysName}\").ToList();");
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
                          $"RecordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}.Select(x => x.RecordInstanceID)));");
            sb.AppendLine(2.Indent() + "}"); //close Setter
            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb.ToString();
        }

        private static Tuple<string, StringBuilder> DropDownFactory(JToken jToken, string propertyName, string sysName,
            string fieldType, string privateName, string multiSelect, bool notAbleToSelectManyValues, string className)
        {
            var defaultValues = jToken.ParseDefaultData(ListTransferFields.FieldValues.GetDescription());
            var staticName = $"{className}Static.DefaultValuesEnum";
            var defaultMap = $"{className}Static.DefaultValuesMap";
            var defaultOptionsName = $"{privateName}Values";
            var enumValues = new StringBuilder();
            var returnValue = new StringBuilder();

            if (defaultValues.Any())
            {
                returnValue = GenerateDDWithDefaultValues(propertyName, sysName, fieldType, privateName, multiSelect, notAbleToSelectManyValues, defaultValues, staticName, defaultOptionsName, defaultMap);

                var enums = new StringBuilder();
                enums.Append(string.Join("$~*@*~$", defaultValues));
                enumValues = enums;
            }
            else
            {
                returnValue = NoDefaultValues(propertyName, sysName, privateName, fieldType, multiSelect);
            }


            return new Tuple<string, StringBuilder>(returnValue.ToString(), enumValues);
        }

        /// <summary>
        /// All enum values are required to be pushed to a enum static extension file
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="fieldType"></param>
        /// <returns>Property, Enum</returns>
        public static Tuple<string, StringBuilder> EnumerableFactory(JToken jToken, MCaseTypes type,
            string propertyName, string sysName, string fieldType, string className)
        {
            var fieldOptions = jToken.ParseToken(ListTransferFields.FieldOptions.GetDescription());
            var dynamicData = jToken.ParseDynamicData(ListTransferFields.DynamicData.GetDescription());

            var privateName = $"_{propertyName.ToLower()}";
            var notAbleToSelectManyValues = fieldOptions.Contains("\"Able to Select Multiple values\"" + ":" + "\"No\"",
                StringComparison.OrdinalIgnoreCase);
            var multiSelect = notAbleToSelectManyValues ? "False" : "True";

            switch (type)
            {
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                    var dropDownValues = DropDownFactory(jToken, propertyName, sysName, fieldType, privateName,
                        multiSelect, notAbleToSelectManyValues, className);// has default values. Values are strings

                    //Item 1 = property, Item 2 = Enum
                    return new Tuple<string, StringBuilder>(dropDownValues.Item1, dropDownValues.Item2);

                case MCaseTypes.CascadingDynamicDropDown:
                case MCaseTypes.DynamicDropDown:
                    var dynamicValues = DynamicDropDownFactory(propertyName, sysName, fieldType, privateName,
                        multiSelect, dynamicData, notAbleToSelectManyValues);//does not have default values. Values are RecordInstances (pointers)
                    return new Tuple<string, StringBuilder>(dynamicValues, new StringBuilder());

                default:
                    return new Tuple<string, StringBuilder>(string.Empty, new StringBuilder());
            }
        }

        #endregion
        #region Additional Factory Helpers

        private static string AddStringValidations(MCaseTypes type)
        {
            var sb = new StringBuilder();

            switch (type)
            {
                case MCaseTypes.EmailAddress:
                    sb.AppendLine(3.Indent() + "if(!value.Contains(\"@\")) value = \"-1\";");
                    break;
                case MCaseTypes.Number:
                    sb.AppendLine(3.Indent() +
                                  "var isNumeric = long.TryParse(value, out _) || int.TryParse(value, out _) || double.TryParse(value, out _);");
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
                    sb.AppendLine(3.Indent() +
                                  "if(!value.Contains(\".com\") || !value.Contains(\"https://\")) value = \"-1\";");
                    break;
                default:
                    return string.Empty;
            }

            return sb.ToString();
        }

        private static StringBuilder NoDefaultValues(string propertyName, string systemName, string privateName, string fieldType, string multiSelect)
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + $"private List<string> {privateName}= null;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {fieldType}]");
            sb.AppendLine(1.Indent() + $"/// [Multi Select: {multiSelect}]");
            sb.AppendLine(1.Indent() + "/// [Getting: Returns the list of field labels]");
            sb.AppendLine(1.Indent() + "/// [Updating: Requires use of either AddTo(), AddRangeTo() or RemoveFrom()]");
            sb.AppendLine(1.Indent() + "/// </summary>");

            if (string.Equals(multiSelect, "True"))
            {
                sb.AppendLine(1.Indent() + "/// <returns> If multi select is false and more than one item found in list, then the new value will return: Multi Select: False</returns>");
            }
            sb.AppendLine(1.Indent() + $"public List<string> {propertyName}");
            sb.AppendLine(1.Indent() + "{");//open property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{");//open getter
            sb.AppendLine(3.Indent() + $"if({privateName} != null) return {privateName};");
            sb.AppendLine(3.Indent() + $"{privateName} = RecordInsData.GetMultiSelectFieldValue(\"{systemName}\");");
            sb.AppendLine(3.Indent() + $"return {privateName};");
            sb.AppendLine(2.Indent() + "}");//close getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{");//open setter
            sb.AppendLine(3.Indent() + $"if({privateName} == null) {privateName} = new List<string>();");
            sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{systemName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}));");
            sb.AppendLine(2.Indent() + "}");//close setter
            sb.AppendLine(1.Indent() + "}");//close property

            return sb;
        }

        private static StringBuilder GenerateDDWithDefaultValues(string propertyName, string sysName, string fieldType,
            string privateName, string multiSelect, bool notAbleToSelectManyValues, List<string> defaultValues
            , string staticName, string defaultOptionsName, string map)
        {
            var sb = new StringBuilder();

            #region private property

            var defaultAggregate = defaultValues.Aggregate(
                $"private List<{staticName}> {defaultOptionsName} = new List<{staticName}>()" + "{", (current, child) => current + $"{staticName}.{child.GetPropertyNameFromSystemName()},");

            defaultAggregate += "};";

            sb.AppendLine(1.Indent() + defaultAggregate);

            sb.AppendLine(1.Indent() + $"private List<{staticName}> {privateName} = null;");

            #endregion

            #region Summary

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {fieldType}]");
            sb.AppendLine(1.Indent() + $"/// [Multi Select: {multiSelect}]");


            sb.AppendLine(1.Indent() + $"/// [Default field values can be found in {staticName}]");
            sb.AppendLine(1.Indent() + $"/// [Available options can be found in {defaultOptionsName}]");

            sb.AppendLine(1.Indent() + "/// [Getting: Returns the list of field labels]");
            sb.AppendLine(1.Indent() + "/// [Updating: Requires AddTo(), RemoveFrom(), MapTo()]");
            sb.AppendLine(1.Indent() + "/// </summary>");

            if (string.Equals(multiSelect, "True"))
            {
                sb.AppendLine(1.Indent() + "/// <returns>");
                sb.AppendLine(1.Indent() + $"/// If value not found in {defaultOptionsName}. New value will return: #~Invalid Selection~#");
                sb.AppendLine(1.Indent() + "/// If multi select is false and more than one item found in list, then the new value will return: Multi Select: False");
                sb.AppendLine(1.Indent() + "/// </returns>");
            }

            #endregion

            #region Public property

            sb.AppendLine(1.Indent() + $"public List<{staticName}> {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if({privateName} != null) return {privateName};");
            sb.AppendLine(3.Indent() + $"{privateName} = GetMultiSelectValue(\"" + sysName + "\");");
            sb.AppendLine(3.Indent() + $"return {privateName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{"); //open Setter
            sb.AppendLine(3.Indent() + $"if({privateName} == null) {privateName} = new List<{staticName}>();");

            bool raised = false; //if list does not have default values, we still wnt to raise the notAbleToSelectManyValues flage


            sb.AppendLine(3.Indent() + "if (value != null && value.Any())");
            sb.AppendLine(3.Indent() + "{// check that all values being set exist in default values enum"); //open if
            sb.AppendLine(4.Indent() + $"if (value.Any(x => !{defaultOptionsName}.Contains(x))) value = new List<{staticName}>() " + "{" + $"{staticName}.Invalidselection" + "};");
            if (notAbleToSelectManyValues)
            {
                raised = true;
                sb.AppendLine(4.Indent() + $"if (value.Count > 1) value = new List<{staticName}>() " + "{" + $"{staticName}.Multiselectfalse" + "};");
            }
            sb.AppendLine(4.Indent() + $"{privateName} = value;");
            sb.AppendLine(3.Indent() + "}"); //close if
            sb.AppendLine(3.Indent() + "else//value could be null or empty. Set to new list"); //open single else
            sb.AppendLine(4.Indent() + $"{privateName} = new List<{staticName}>();");

            if (!raised)
            {
                if (notAbleToSelectManyValues)
                {
                    sb.AppendLine(3.Indent() + $"if (value.Count > 1) value = new List<{staticName}>() " + "{" + $"{staticName}.Multiselectfalse" + "};");
                }
            }

            sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}.Select(x => {map}[x])));");
            sb.AppendLine(2.Indent() + "}"); //close Setter
            sb.AppendLine(1.Indent() + "}"); //close Property

            #endregion

            return sb;
        }

        #endregion
        #region Static File Extensions

        public static string GenerateStaticFile(string namespace_)
        {
            var sb = new StringBuilder();

            sb.AppendLine( //TODO continue to add usings, as more and more validations are made
                "using System;\nusing System.Collections.Generic;\nusing System.ComponentModel;\nusing System.Linq;\nusing MCaseEventsSDK.Util;\nusing MCaseEventsSDK.Util.Data;\nusing System.Text.RegularExpressions;");
            sb.AppendLine($"namespace {namespace_}"); //TODO: project specific namespace
            sb.AppendLine("{"); //Open class
            sb.AppendLine(0.Indent() + "public static class FactoryExtensions"); //static class
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.Append(BuildEnumExtensions());
            sb.Append(AddStaticEnumerableExtensions());

            sb.AppendLine(0.Indent() + "}"); //close class
            sb.AppendLine("}"); //close  class
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
        private static string AddStaticEnumerableExtensions()
        {
            var sb = new StringBuilder();

            sb.Append(BuildAddMethods());

            sb.Append(BuildAddRangeMethods());

            sb.Append(BuildRemoveFromMethods());

            sb.Append(BuildClearMethods());

            return sb.ToString();
        }

        private static StringBuilder BuildClearMethods()
        {
            var sb = new StringBuilder();

            #region Clear

            sb.AppendLine(1.Indent() + "public static int Clear<T, TEnum>(this T classObject, string property) where TEnum : Enum");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if(getMethod == null) return -2;");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType == typeof(List<string>)) propertyInfo.SetValue(classObject, new List<string>());");
            sb.AppendLine(2.Indent() + "else if (getMethod.ReturnType == typeof(List<TEnum>)) propertyInfo.SetValue(classObject, new List<TEnum>());");
            sb.AppendLine(2.Indent() + "else if (getMethod.ReturnType == typeof(List<RecordInstanceData>)) propertyInfo.SetValue(classObject, new List<RecordInstanceData>());");
            sb.AppendLine(2.Indent() + "else return -1;");
            sb.AppendLine(2.Indent() + "return 0;");
            sb.AppendLine(1.Indent() + "}"); //close method


            #endregion

            return sb;
        }

        private static StringBuilder BuildAddMethods()
        {
            var sb = new StringBuilder();

            #region Add RecordInstance

            sb.AppendLine(1.Indent() + "public static int AddTo<T>(this T classObject, string property, RecordInstanceData value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if (value.RecordInstanceID == 0) return -5;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "getter.Add(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Add String

            sb.AppendLine(1.Indent() + "public static int AddTo<T>(this T classObject, string property, string value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "getter.Add(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Add DefaultValue

            sb.AppendLine(1.Indent() +
                          $"public static int AddTo<T, TEnum>(this T classObject,string property, TEnum value) where TEnum : Enum");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() +
                          "if (getMethod.ReturnType != typeof(List<TEnum>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<TEnum>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "getter.Add(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method


            #endregion

            return sb;
        }

        private static StringBuilder BuildAddRangeMethods()
        {
            var sb = new StringBuilder();

            #region AddRange RecordInstance

            sb.AppendLine(1.Indent() +
                          $"public static int AddRangeTo<T>(this T classObject, string property, List<RecordInstanceData> value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if (!value.Any()) return 0;");
            sb.AppendLine(2.Indent() + "if (value.Any(x => x.RecordInstanceID == 0)) return -5;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() +
                          "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "value.RemoveAll(x => x == null);");
            sb.AppendLine(2.Indent() + "if(value.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.AddRange(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return getter.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region AddRange RecordInstance

            sb.AppendLine(1.Indent() +
                          $"public static int AddRangeTo<T>(this T classObject, string property, List<string> value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if(!value.Any()) return 0;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() +
                          "if (getMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "value.RemoveAll(x => x == null);");
            sb.AppendLine(2.Indent() + "if(value.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.AddRange(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return value.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region AddRange Default Values

            sb.AppendLine(1.Indent() + "public static int AddRangeTo<T, TEnum>(this T classObject, string property, List<TEnum> value) where TEnum : Enum");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if(!value.Any()) return 0;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<TEnum>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<TEnum>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "value.RemoveAll(x => x == null);");
            sb.AppendLine(2.Indent() + "if(value.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.AddRange(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return value.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            return sb;
        }

        private static StringBuilder BuildRemoveFromMethods()
        {
            var sb = new StringBuilder();

            #region Remove RecordInstance

            sb.AppendLine(1.Indent() + "public static int RemoveFrom<T>(this T classObject, string property, Func<RecordInstanceData, bool> predicate)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (predicate == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "var foundValues = getter.Where(predicate).ToList();");
            sb.AppendLine(2.Indent() + "if (foundValues.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.RemoveAll(x => foundValues.Contains(x));");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return foundValues.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Remove string
            sb.AppendLine(1.Indent() +
                          "public static int RemoveFrom<T>(this T classObject, string property, Func<string, bool> predicate)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (predicate == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var propertyMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (propertyMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "var foundStrings = getter.Where(predicate).ToList();");
            sb.AppendLine(2.Indent() + "if (foundStrings.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter.RemoveAll(x => foundStrings.Contains(x)));");
            sb.AppendLine(2.Indent() + "return foundStrings.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Remove enum

            sb.AppendLine(1.Indent() +
                          "public static int RemoveFrom<T, TEnum>(this T classObject, string property, Func<TEnum, bool> predicate) where TEnum : Enum");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (predicate == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var propertyMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (propertyMethod.ReturnType != typeof(List<TEnum>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<TEnum>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "var foundStrings = getter.Where(predicate).ToList();");
            sb.AppendLine(2.Indent() + "if (foundStrings.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter.RemoveAll(x => foundStrings.Contains(x)));");
            sb.AppendLine(2.Indent() + "return foundStrings.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            return sb;
        }

        public static StringBuilder BuildEnumExtensions()
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "public static TEnum TryGetValue<TEnum>(this string value) where TEnum : struct => Enum.TryParse<TEnum>(value.GetEnumName(), out var converted) ? converted : default;");
            sb.AppendLine(1.Indent() + "public static List<TEnum> MapTo<TEnum>(this IEnumerable<string> values) where TEnum : struct => values.Select(value => value.TryGetValue<TEnum>()).ToList();");

            sb.AppendLine(1.Indent() + "private static string GetEnumName(this string input)");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "input = Regex.Replace(input, @\"[^\\w]\", \"\");");
            sb.AppendLine(2.Indent() + "if (int.TryParse(input, out _)) return \"F_\" + input;");
            sb.AppendLine(2.Indent() + "if (int.TryParse(input[0].ToString(), out _)) input = \"F_\" + input;");
            sb.AppendLine(2.Indent() + "char.TryParse(input[0].ToString().ToUpperInvariant(), out var cap);");
            sb.AppendLine(2.Indent() + "var lowerCase = input.Substring(1).ToLower();");
            sb.AppendLine(2.Indent() + "return cap + lowerCase;");
            sb.AppendLine(1.Indent() + "}");// close method


            return sb;
        }

        #endregion

        public static string GetEmbeddedOptions(string className)
        {
            var sb = new StringBuilder();
            var embeddedEnum = $"{className}Static.EmbeddedOptionsEnum";
            var embeddedMap = $"{className}Static.EmbeddedOptionsMap";

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// [mCase data type] Embedded List.");
            sb.AppendLine(1.Indent() + "/// Requires the Datalist Id from one of the following Embedded Data lists:");
            sb.AppendLine(1.Indent() + $"/// Refer to {embeddedEnum} for embedded options");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"embeddedEnum\"> Enum built for this specific method</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Related children from selected data list</returns>");
            sb.AppendLine(1.Indent() +
                          $"public List<RecordInstanceData> GetActiveChildRecords({embeddedEnum} embeddedEnum)"); // property name is added back with enum name appended 
            sb.AppendLine(1.Indent() + "{"); //open class
            sb.AppendLine(2.Indent() + $"var sysName = {embeddedMap}[embeddedEnum];");
            sb.AppendLine(2.Indent() + "if(string.IsNullOrEmpty(sysName)) return new  List<RecordInstanceData>();");
            sb.AppendLine(2.Indent() + "var childDataListId = _eventHelper.GetDataListID(sysName);");
            sb.AppendLine(2.Indent() + "return _eventHelper.GetActiveChildRecordsByParentRecId(RecordInsData.RecordInstanceID).Where(x => x.DataListID == childDataListId).ToList();");
            sb.AppendLine(1.Indent() + "}"); //close class

            return sb.ToString();
        }

        public static string AddEnumerableExtensions(string className, bool addMap)
        {
            var sb = new StringBuilder();

            var entity = $"{className}Entity";
            var staticProperties = $"{className}Static.Properties_Enum";
            var defaultValues = $"{className}Static.DefaultValuesEnum";
            var propertyMap = $"{className}Static.Properties_Map";
            var defaultMap = $"{className}Static.DefaultValuesMap";

            if (addMap)
            {
                // Map To Enum
                sb.AppendLine(1.Indent() + "/// <summary>");
                sb.AppendLine(1.Indent() + "/// Maps a list of enums, to the next enum type");
                sb.AppendLine(1.Indent() + "/// </summary>");
                sb.AppendLine(1.Indent() + "/// <typeparam name=\"TEnum\">The enum you are mapping to</typeparam>");
                sb.AppendLine(1.Indent() + "/// <param name=\"values\">Enum Values to convert</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Returns the converted list of enums, or default values if not found</returns>");
                sb.AppendLine(1.Indent() + $"public List<TEnum> MapToEnum<TEnum>(List<{className}Static.DefaultValuesEnum> values) where TEnum : struct => values.Select(x => {defaultMap}[x]).MapTo<TEnum>();");

                //remove by enum predicate 
                sb.AppendLine(1.Indent() + "/// <summary>");
                sb.AppendLine(1.Indent() + "/// Remove all data from enumerable class property that matches predicate");
                sb.AppendLine(1.Indent() + "/// </summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">Default value to remove</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. null errors: -2</returns>");
                sb.AppendLine(1.Indent() + $"public int RemoveFrom({staticProperties} propertyEnum, Func<{defaultValues}, bool> predicate) => this.RemoveFrom({propertyMap}[propertyEnum], predicate);");
            }

            #region Remove by predicate Predicate
            // remove value by Record instance predicate
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Remove all data from enumerable class property that matches predicate");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">Record instance value to remove</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. null errors: -2</returns>");
            sb.AppendLine(1.Indent() + $"public int RemoveFrom({staticProperties} propertyEnum, Func<RecordInstanceData, bool> predicate) => this.RemoveFrom({propertyMap}[propertyEnum], predicate);");

            //remove by string predicate
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Remove all data from enumerable class property that matches predicate");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">String value to remove</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. null errors: -2</returns>");
            sb.AppendLine(1.Indent() + $"public int RemoveFrom({staticProperties} propertyEnum, Func<string, bool> predicate) => this.RemoveFrom({propertyMap}[propertyEnum], predicate);");

            #endregion

            #region Add To

            // add single
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
            sb.AppendLine(1.Indent() +
                          "/// <returns>Amount of values removed. Type errors: -1. Null errors: -2.</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int AddTo({staticProperties} propertyEnum, string param) => this.AddTo({propertyMap}[propertyEnum], param);");

            //add single Record Instance
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. Null errors: -2. RecordInstance not Created: -5.</returns>");
            sb.AppendLine(1.Indent() + $"public int AddTo({staticProperties} propertyEnum, RecordInstanceData param) => this.AddTo({propertyMap}[propertyEnum], param);");

            if (addMap)
            {
                //add default value (Enum)
                sb.AppendLine(1.Indent() + "/// <summary>");
                sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
                sb.AppendLine(1.Indent() + "/// </summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2.</returns>");
                sb.AppendLine(1.Indent() + $"public int AddTo({staticProperties} propertyEnum, {defaultValues} param) => this.AddTo({propertyMap}[propertyEnum], param);");
            }

            #endregion

            #region Add Range

            //add range string
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
            sb.AppendLine(1.Indent() + "/// List of string's");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2.</returns>");
            sb.AppendLine(1.Indent() + $"public int AddRangeTo({staticProperties} propertyEnum, List<string> param) => this.AddRangeTo({propertyMap}[propertyEnum], param);");

            //add range Record Instance
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
            sb.AppendLine(1.Indent() + "/// List of RecordInstanceData's");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2. RecordInstance not Created: -5</returns>");
            sb.AppendLine(1.Indent() + $"public int AddRangeTo({staticProperties} propertyEnum, List<RecordInstanceData> param) => this.AddRangeTo({propertyMap}[propertyEnum], param);");


            if (addMap)
            {
                //add default value range (Enum)
                sb.AppendLine(1.Indent() + "/// <summary>");
                sb.AppendLine(1.Indent() + "/// Add data onto enumerable class property.");
                sb.AppendLine(1.Indent() + $"/// List of {defaultValues}'s");
                sb.AppendLine(1.Indent() + "/// </summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2.</returns>");
                sb.AppendLine(1.Indent() + $"public int AddRangeTo({staticProperties} propertyEnum, List<{defaultValues}> param) => this.AddRangeTo({propertyMap}[propertyEnum], param);");

            }

            #endregion
            #region Clear

            //clear
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Clears all existing values from list.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Cleared list = 0. Type errors: -1. null errors: -2.</returns>");
            sb.AppendLine(1.Indent() + $"public int Clear({staticProperties} propertyEnum) => this.Clear<{entity}, {staticProperties}>({propertyMap}[propertyEnum]);");

            #endregion

            #region private method extractions

            if (addMap)
            {
                //add MultiSelectValue
                sb.AppendLine(1.Indent() + $"private List<{defaultValues}> GetMultiSelectValue(string sysName)");
                sb.AppendLine(1.Indent() + "{");
                sb.AppendLine(2.Indent() + "var storedValue = RecordInsData.GetMultiSelectFieldValue(sysName);");
                sb.AppendLine(2.Indent() + $"return !storedValue.Any() ? new List<{defaultValues}>() : storedValue.Select(x => x.TryGetValue<{defaultValues}>()).ToList();");
                sb.AppendLine(1.Indent() + "}");
            }

            #endregion
            return sb.ToString();
        }

        public static StringBuilder GenerateEnums(List<string> fieldSet, string className, bool titleCase)
        {
            var sb = new StringBuilder();

            var distinct = fieldSet.Distinct().OrderBy(x => x).ToList();//order enums by name

            sb.AppendLine(BuildEnums(distinct, className).ToString());

            sb.AppendLine(BuildEnumMapper(distinct, className, titleCase).ToString());

            return sb;
        }

        private static StringBuilder BuildEnums(List<string> fieldSet, string className)
        {
            var sb = new StringBuilder();

            var distinct = fieldSet.Distinct().OrderBy(x => x).ToList();//order enums by name
            var distinctEnums = new List<string>();

            if (distinct.Count == 0)
                return sb;

            sb.Append(1.Indent() + $"public enum {className}Enum" + "{ Invalidselection,"); //open

            for (var i = 0; i < distinct.Count; i++)
            {
                var field = distinct[i];

                if (field.Contains("\""))
                {
                    field = field.Replace("\"", "\\\"");
                }

                var enumName = field.GetPropertyNameFromSystemName();

                if (distinctEnums.Contains(enumName))
                {
                    sb.Append(enumName + $"_{i}_,"); //generate enum duplicate
                }
                else
                {
                    sb.Append(enumName + ","); //generate enum 
                    distinctEnums.Add(enumName);
                }
            }

            sb.Append("}"); //close enum

            return sb;
        }

        public static StringBuilder BuildEnumMapper(List<string> fieldSet, string className, bool titleCase)
        {
            var sb = new StringBuilder();

            var distinct = fieldSet.Distinct().OrderBy(x => x).ToList();//order enums by name
            var distinctEnums = new List<string>();

            if (distinct.Count == 0)
                return sb;

            var staticClass = $"{className}Enum";

            sb.Append(1.Indent() + $"public static Dictionary<{staticClass}, string> {className}Map => new Dictionary<{staticClass}, string>()" + "{ {"+$"{staticClass}.Invalidselection, \"#~Invalid Selection~#\"" + "},"); //open

            for (var i = 0; i < distinct.Count; i++)
            {
                var field = distinct[i];

                if (field.Contains("\""))
                {
                    field = field.Replace("\"", "\\\"");
                }

                var value = titleCase ? $"\"{field.GetPropertyNameFromSystemName()}\"" : $"\"{field}\""; //get value

                var enumName = field.GetPropertyNameFromSystemName();

                if (distinctEnums.Contains(enumName))
                {
                    distinctEnums.Add(enumName);
                    enumName += $"_{i}_";
                }
                var key = enumName;

                sb.Append("{" + $"{staticClass}.{key}, {value}" + "},");
            }

            sb.Append("};"); //close dict

            return sb;
        }

        public static StringBuilder GenerateRelationships(JToken? relationships)
        {
            var sb = new StringBuilder();

            if (relationships == null || !relationships.Any())
                return sb;

            var parentRelationships =
                relationships.ParseChildren(ListTransferFields.ParentSystemName.GetDescription());

            if (parentRelationships.Any())
            {
                var distinctParentRelationships = parentRelationships.Distinct();
                sb.AppendLine(GenerateEnums(distinctParentRelationships.ToList(), "ParentRelationships", false).ToString());
            }

            var childRelationships =
                relationships.ParseChildren(ListTransferFields.ChildSystemName.GetDescription());

            if (!childRelationships.Any()) return sb;

            var distinctChildRelationships = childRelationships.Distinct();
            sb.AppendLine(GenerateEnums(distinctChildRelationships.ToList(), "ChildRelationships", false).ToString());

            return sb;
        }
    }
}

//extract duplicate getters / setters to methods.