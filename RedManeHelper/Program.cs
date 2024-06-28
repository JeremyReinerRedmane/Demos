using mCASE_ADMIN.DataAccess.mCase;//DemoKatan.mCase;

var commandLineArgs = Environment.GetCommandLineArgs();

if (commandLineArgs.Length > 1)
{
    if (commandLineArgs.Length == 8)
    {
        //directly querying db for datalist id's
        var cmd = new SyncDlConfigs(commandLineArgs);

        var sqlQuery = cmd.DataAccess().Result;

        cmd.RemoteSync(sqlQuery);
    }

    if (commandLineArgs.Length == 7)
    {
        //direct access to csv data
        var cmd = new SyncDlConfigs(commandLineArgs);

        var data = cmd.DirectDataAccess();

        cmd.RemoteSync(data);
    }

}
else
{
    var cmd = new List<string>
    {
        "",
        "1250,726,744",
        //"data source=localhost;initial catalog=mCASE_ADMIN;integrated security=True;TrustServerCertificate=true;",
        //"SELECT [DataListID] FROM [mCASE_ADMIN].[dbo].[DataList]",
        "jeremy.reiner:Password123!",
        //"http://localhost:64762",
        "https://auusmc-arccwis-app-mcs-qa-r2.redmane-cloud.us",
        @"C:\Users\jreiner\source\repos\AR-mCase-CustomEvents\MCaseCustomEvents\FactoryEntities",
        @"C:\Users\jreiner\Desktop\Exceptions",
        "MCaseCustomEvents.FactoryEntities"
    }.ToArray();

    var local = new SyncDlConfigs(cmd);
    if (cmd.Length == 8)
    {
        //directly querying db
        var sqlQuery = await local.DataAccess();

        local.RemoteSync(sqlQuery);
    }

    if (cmd.Length == 7)
    {
        //direct access to csv data
        var data = local.DirectDataAccess();

        local.RemoteSync(data);
    }
}