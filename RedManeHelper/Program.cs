using mCASE_ADMIN.DataAccess.mCase;//DemoKatan.mCase;

var cmd = Environment.GetCommandLineArgs();

if (cmd.Length <= 1)
{
    cmd = new List<string>
    {
        "",
        //"csv",
        "connectionString",
        "sql query",
        "",
        "https://auusmc-arccwis-app-mcs-qa-r2.redmane-cloud.us/",
        @"C:\Users\jreiner\source\repos\AR-mCase-CustomEvents\MCaseCustomEvents\FactoryEntities",
        @"C:\Users\jreiner\Desktop\Exceptions",
        "MCaseCustomEvents.ARFocus.FactoryEntities",
        "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Numerics;\r\nusing MCase.Core.Event;\r\nusing MCaseEventsSDK;\r\nusing MCaseEventsSDK.Util;\r\nusing MCaseEventsSDK.Util.Data;",
        "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing MCaseEventsSDK.Util.Data;\r\nusing System.Text.RegularExpressions;\r\nusing MCase.Core.Utility;\r\nusing MCaseEventsSDK;"
    }.ToArray();
}

var local = new SyncDlConfigs(cmd);

local.RemoteSync();
