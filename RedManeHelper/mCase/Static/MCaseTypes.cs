using System.ComponentModel;

namespace DemoKatan.mCase.Static
{
    public enum MCaseTypes
    {
        Position0,
        [Description("Dropdownlist")]//Drop-Down List
        DropDownList,
        [Description("Header")]
        Header,
        [Description("String")]
        String,
        [Description("Date")]
        Date,
        [Description("Longstring")]//Long String
        LongString,
        [Description("Emailaddress")]//Email Address
        EmailAddress,
        [Description("Phone")]
        Phone,
        [Description("Embeddedlist")]//Embedded List
        EmbeddedList,
        [Description("Userrolesecurityrestrict")]//User Role Security Restrict
        UserRoleSecurityRestrict,
        [Description("Section")]
        Section,
        [Description("Boolean")]
        Boolean,
        [Description("Dynamiccalculatedfield")]//Dynamic Calculated Field
        DynamicCalculatedField,
        [Description("Address")]
        Address,
        [Description("Number")]
        Number,
        [Description("Money")]
        Money,
        [Description("Narrative")]
        Narrative,
        [Description("Dynamicdropdown")]//Dynamic Drop-Down
        DynamicDropDown,
        [Description("Uniqueidentifier")]//Unique Identifier
        UniqueIdentifier,
        [Description("User")]
        User,
        [Description("DateTime")]
        DateTime,
        [Description("Cascadingdynamicdropdown")]//Cascading Dynamic Drop-Down
        CascadingDynamicDropDown,
        [Description("Calculatedfield")]//Calculated Field
        CalculatedField,
        [Description("Attachment")]
        Attachment,
        [Description("Embeddeddocument")]//Embedded Document
        EmbeddedDocument,
        [Description("Score1")]//Score 1
        Score1,
        [Description("Time")]
        Time,
        [Description("Cascadingdropdown")]//Cascading Drop-Down
        CascadingDropDown,
        [Description("Score2")]//Score 2
        Score2,
        [Description("URL")]
        URL,
        [Description("Readonlyfield")]//Read-only Field
        ReadonlyField,
        [Description("Score3")]//Score 3
        Score3,
        [Description("Score4")]//Score 4
        Score4,
        [Description("Score5")]//Score 5
        Score5,
        [Description("Score6")]//Score 6
        Score6,
        [Description("Linebreak")]//Line Break
        LineBreak,
        [Description("Hiddenfield")]//Hidden Field
        HiddenField
    }

    public enum ListTransferFields
    {
        [Description("Id")]
        Id,
        [Description("SystemName")]
        SystemName,
        [Description("Type")]
        Type,
        [Description("Fields")]
        Fields,
        [Description("DefaultValue")]
        DefaultValue,
        [Description("ChildSystemName")]
        ChildSystemName,
        [Description("ParentSystemName")]
        ParentSystemName,
        [Description("FieldOptions")]
        FieldOptions,
        [Description("DynamicData")]
        DynamicData,
        [Description("DynamicSourceSystemName")]
        DynamicSourceSystemName,
        [Description("Relationships")]
        Relationships,
        [Description("FieldValues")]
        FieldValues,
        [Description("Name")]
        Name,
        [Description("Value")]
        Value
    }
}