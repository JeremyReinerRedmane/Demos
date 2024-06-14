using DemoKatan.mCase.Static;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions = DemoKatan.mCase.Static.Extensions;

namespace DemoKatan.mCase
{
    public static class Factory
    {
        public static string EnumerableFactory(JToken jToken, MCaseTypes type)
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
                    sb.AppendLine(1.Indent() + $"private List<string> {privateName} = null;");
                    sb.AppendLine(1.Indent() + "/// <summary>");
                    sb.AppendLine(1.Indent() + $"/// [mCase data type: {fieldType}]");
                    sb.AppendLine(1.Indent() + $"/// [Multi Select: {multiSelect}]");
                    sb.AppendLine(1.Indent() +
                                  "/// [Setting: Accepts a list of strings such as 'yes' or 'no' added to the original value]");
                    sb.AppendLine(1.Indent() + "/// [Getting: Returns the list of field labels]");
                    sb.AppendLine(1.Indent() + "/// </summary>");
                    sb.AppendLine(1.Indent() + $"public List<string> {sysName}");
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
                    if (notAbleToSelectManyValues)
                        sb.AppendLine(3.Indent() +
                                      $"if (value != null && value.Count > 1) throw new Exception(\"[Multi Select is Disabled] {sysName} only accepts a list length of 1.\");");
                    sb.AppendLine(3.Indent() +
                                  $"_recordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, value));");
                    sb.AppendLine(2.Indent() + "}"); //close Setter
                    sb.AppendLine(1.Indent() + "}"); //close Property
                    break;
                case MCaseTypes.CascadingDynamicDropDown:
                case MCaseTypes.DynamicDropDown:
                    sb.AppendLine(1.Indent() + $"private List<RecordInstanceData> {privateName} = null;");
                    sb.AppendLine(1.Indent() + "/// <summary>");
                    sb.AppendLine(1.Indent() + $"/// [mCase data type: {fieldType}]");
                    sb.AppendLine(1.Indent() + $"/// [Multi Select: {multiSelect}]");
                    sb.AppendLine(1.Indent() + $"/// [Dynamic Source: {dynamicData}]");
                    sb.AppendLine(1.Indent() + "/// [Setting: Requires the RecordInstanceID]");
                    sb.AppendLine(1.Indent() + "/// [Getting: Returns the list of RecordInstancesData's]");
                    sb.AppendLine(1.Indent() + "/// </summary>");
                    sb.AppendLine(1.Indent() + $"public List<RecordInstanceData> {sysName}");
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
                    break;
                default:
                    return string.Empty;
            }

            return sb.ToString();
        }

        public static string StringFactory(JToken jToken)
        {
            var sb = new StringBuilder();
            var sysName = jToken.ParseToken(ListTransferFields.SystemName.GetDescription()); //title case
            var type = jToken.ParseToken(ListTransferFields.Type.GetDescription()); //title case
            var privateSysName = $"_{sysName.ToLower()}";

            var readonlyOrMirrored = jToken.IsStringReadonlyOrMirrored();

            sb.AppendLine(1.Indent() + $"private string {privateSysName} = string.Empty;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {type}]");
            if (readonlyOrMirrored)
                sb.AppendLine(1.Indent() + "/// This is a Readonly / Mirrored field. No setting / updating allowed.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + $"public string {sysName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(3.Indent() + $"{privateSysName} = _recordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter

            if (!readonlyOrMirrored)
            {
                //string is not readonly, add setter
                sb.AppendLine(2.Indent() + "set");
                sb.AppendLine(2.Indent() + "{"); //open Setter
                sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
                sb.AppendLine(3.Indent() + $"_recordInsData.SetValue(\"{sysName}\", {privateSysName});");
                sb.AppendLine(2.Indent() + "}"); //close Setter
            }

            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb.ToString();
        }

        public static StringBuilder ClassInitializer(JObject jObject, string className)
        {
            var sb = new StringBuilder();

            var id = jObject.ParseJson(ListTransferFields.Id.GetDescription());
            var sysName = jObject.ParseJson(ListTransferFields.SystemName.GetDescription());
            var relationships = jObject[ListTransferFields.Relationships.GetDescription()];//??
            var dtNow = DateTime.Now.ToString(Extensions.MCaseDateTimeStorageFormat);
            
            sb.AppendLine(
                "using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing MCaseCustomEvents.ARFocus.DataAccess;\nusing MCaseEventsSDK;\nusing MCaseEventsSDK.Util;\nusing MCaseEventsSDK.Util.Data;\nusing System.ComponentModel;\nusing System.Reflection;");
            sb.AppendLine("namespace MCaseCustomEvents.ARFocus.FactoryEntities"); //TODO: project specific namespace
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
            sb.AppendLine(1.Indent() + "private int _dataListId = -1;");
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
            if (relationships != null && relationships.Any())
            {
                var parentRelationships =
                    relationships.ParseChildren(ListTransferFields.ParentSystemName.GetDescription());

                if (parentRelationships.Any())
                {
                    var parentList = parentRelationships.Aggregate(
                        "public List<string> ParentRelationShips => new List<string>() {", (current, child) => current + $"\"{child}\",");
                    parentList += "};";
                    sb.AppendLine(1.Indent() + parentList);
                }

                var childRelationships =
                    relationships.ParseChildren(ListTransferFields.ChildSystemName.GetDescription());

                if (childRelationships.Any())
                {
                    var childList = childRelationships.Aggregate(
                        "public List<string> ChildRelationShips => new List<string>() {",
                        (current, child) => current + $"\"{child}\",");
                    childList += "};";
                    sb.AppendLine(1.Indent() + childList);
                }
            }

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
            sb.AppendLine(1.Indent() +
                          $"public {className}Entity(RecordInstanceData recordInsData, AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor
            sb.AppendLine(2.Indent() + "_recordInsData = recordInsData;");
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(1.Indent() + "}"); //close constructor
            sb.AppendLine(1.Indent() + $"public string SystemName => \"{sysName}\";");
            sb.AppendLine(1.Indent() + "public long RecordInstanceId => _recordInsData.RecordInstanceID;");
            sb.AppendLine(1.Indent() + "public void SaveRecord()");
            sb.AppendLine(1.Indent() + "{"); //open Method
            sb.AppendLine(2.Indent() + "_eventHelper.SaveRecord(_recordInsData);");
            sb.AppendLine(1.Indent() + "}"); //close Method
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
            sb.AppendLine(2.Indent() + "}"); //close Setter
            sb.AppendLine(1.Indent() + "}"); //close Property

            #endregion

            return sb;
        }

        public static string LongFactory(JToken jToken)
        {
            var sb = new StringBuilder();

            var sysName = jToken.ParseToken(ListTransferFields.SystemName.GetDescription());
            var type = jToken.ParseToken(ListTransferFields.Type.GetDescription());

            var privateSysName = $"_{sysName.ToLower()}";

            sb.AppendLine(1.Indent() + $"private string {privateSysName} = string.Empty;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + $"/// [mCase data type: {type}]");
            sb.AppendLine(1.Indent() + "/// Gets value, and sets long value");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + $"public string {sysName}");
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

        public static string GeneratePropertyEnums(HashSet<string> fieldSet, string className)
        {
            var sb = new StringBuilder();

            sb.AppendLine(0.Indent() + $"public enum {className}Enum");
            sb.AppendLine(0.Indent() + "{");

            foreach (var field in fieldSet)
            {
                sb.AppendLine(1.Indent() + $"[Description(\"{field.CleanString()}\")]");
                sb.AppendLine(1.Indent() + $"{field},");

            }

            sb.AppendLine(0.Indent() + "}");

            return sb.ToString();
        }

        /// <summary>
        /// Handles processing for Embedded lists
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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


        public static string AddEnumerableExtensions(string className)
        {
            var sb = new StringBuilder();

            #region Add

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() +
                          $"/// Use {className}Enum to find Property to update, and Either string value or recordInstance value.");
            sb.AppendLine(1.Indent() + "/// If both are entered no update will occur. returning value of 0.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + $"/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Default null value</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"stringValue\">Default null value</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values added</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int AddTo({className}Enum propertyEnum, RecordInstanceData value = null, string stringValue = null)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "var propertyInfo = GetPropertyInfo(propertyEnum, value, stringValue);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return 0;");
            sb.AppendLine(2.Indent() + "if (value != null)");
            sb.AppendLine(2.Indent() + "{"); //open if
            sb.AppendLine(3.Indent() + "if (value.RecordInstanceID == 0) return 0;");
            sb.AppendLine(3.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(this);");
            sb.AppendLine(3.Indent() + "if(getter == null) return 0;");
            sb.AppendLine(3.Indent() + "if(getter.Any(r => r.RecordInstanceID == value.RecordInstanceID)) return 0;");
            sb.AppendLine(3.Indent() + "getter.Add(value);");
            sb.AppendLine(3.Indent() + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(3.Indent() + "return 1;");
            sb.AppendLine(2.Indent() + "}"); //close if
            sb.AppendLine(2.Indent() + "if(string.IsNullOrEmpty(stringValue)) return 0;");
            sb.AppendLine(2.Indent() + "var stringGetter = (List<string>)propertyInfo.GetValue(this);");
            sb.AppendLine(2.Indent() + "if (stringGetter == null) return 0;");
            sb.AppendLine(2.Indent() + "if(stringGetter.Any(r => string.Equals(r, stringValue, StringComparison.OrdinalIgnoreCase))) return 0;");
            sb.AppendLine(2.Indent() + "stringGetter.Add(stringValue);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(this, stringGetter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Remove

            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() +
                          $"/// Use {className}Enum to find Property to update, and Either string value or recordInstance value.");
            sb.AppendLine(1.Indent() + "/// If both are entered no update will occur. returning value of 0.");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + $"/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Default null value</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"stringValue\">Default null value</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int RemoveFrom({className}Enum propertyEnum, RecordInstanceData value = null, string stringValue = null)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "var propertyInfo = GetPropertyInfo(propertyEnum, value, stringValue);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return 0;");
            sb.AppendLine(2.Indent() + "if (value != null)");
            sb.AppendLine(2.Indent() + "{"); //open if
            sb.AppendLine(3.Indent() + "if (value.RecordInstanceID == 0) return 0;");
            sb.AppendLine(3.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(this);");
            sb.AppendLine(3.Indent() + "if (getter == null) return 0;");
            sb.AppendLine(3.Indent() + "var count = getter.Count(x => x.RecordInstanceID == value.RecordInstanceID);");
            sb.AppendLine(3.Indent() + "if (count < 1) return 0;");
            sb.AppendLine(3.Indent() + "getter.RemoveAll(x => x.RecordInstanceID == value.RecordInstanceID);");
            sb.AppendLine(3.Indent() + "propertyInfo.SetValue(this, getter);");
            sb.AppendLine(3.Indent() + "return count;");
            sb.AppendLine(2.Indent() + "}"); //close if
            sb.AppendLine(2.Indent() + "if(string.IsNullOrEmpty(stringValue)) return 0;");
            sb.AppendLine(2.Indent() + "var stringGetter = (List<string>)propertyInfo.GetValue(this);");
            sb.AppendLine(2.Indent() + "if(stringGetter == null) return 0;");
            sb.AppendLine(2.Indent() +
                          "var stringCount = stringGetter.Count(x => string.Equals(x, stringValue, StringComparison.OrdinalIgnoreCase));");
            sb.AppendLine(2.Indent() + "if (stringCount < 1) return 0;");
            sb.AppendLine(2.Indent() +
                          "propertyInfo.SetValue(this, stringGetter.RemoveAll(x => string.Equals(x, stringValue, StringComparison.OrdinalIgnoreCase)));");
            sb.AppendLine(2.Indent() + "return stringCount;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion
            #region GetProperty

            sb.AppendLine(1.Indent() + "private PropertyInfo GetPropertyInfo(ArchiveintakesEnum propertyEnum, RecordInstanceData value = null, string stringValue = null)");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "if (value != null && !string.IsNullOrEmpty(stringValue)) return null;");
            sb.AppendLine(2.Indent() + "var property = propertyEnum.GetEnumDescription();");
            sb.AppendLine(2.Indent() + "var objectType = this.GetType();");
            sb.AppendLine(2.Indent() + "return objectType.GetProperty(property);");
            sb.AppendLine(1.Indent() + "}");//close method

            #endregion

            return sb.ToString();
        }
    }
}
