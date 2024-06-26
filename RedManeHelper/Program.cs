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