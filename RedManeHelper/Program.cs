using mCASE_ADMIN.DataAccess.mCase;//DemoKatan.mCase;

var commandLineArgs = Environment.GetCommandLineArgs();

if (commandLineArgs.Length > 1)
{
    if (commandLineArgs.Length == 10)
    {
        //directly querying db for datalist id's
        var cmd = new SyncDlConfigs(commandLineArgs);

        var sqlQuery = cmd.SqlDataAccess();

        cmd.RemoteSync(sqlQuery);
    }

    if (commandLineArgs.Length == 9)
    {
        //direct access to csv data
        var cmd = new SyncDlConfigs(commandLineArgs);

        var data = cmd.CsvDataAccess();

        cmd.RemoteSync(data);
    }

}
else
{
    //var cmd = new List<string>
    //{
    //    "",
    //    //"1250,726,744",
    //    "data source",
    //    "sql query",
    //    "credentials",
    //    "api endpoint url",
    //    @"output directory",
    //    @"exception directory",
    //    "namespace",
    //    "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Numerics;\r\nusing MCase.Core.Event;\r\nusing MCaseEventsSDK;\r\nusing MCaseEventsSDK.Util;\r\nusing MCaseEventsSDK.Util.Data;",
    //    "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing MCaseEventsSDK.Util.Data;\r\nusing System.Text.RegularExpressions;\r\nusing MCase.Core.Utility;\r\nusing MCaseEventsSDK;"
    //}.ToArray();
    var cmd = new List<string>
    {

    }.ToArray();

    var local = new SyncDlConfigs(cmd);
    if (cmd.Length == 10)
    {
        //directly querying db
        var sqlQuery = local.SqlDataAccess();

        local.RemoteSync(sqlQuery);
    }

    if (cmd.Length == 9)
    {
        //direct access to csv data
        var data = local.CsvDataAccess();

        local.RemoteSync(data);
    }
}