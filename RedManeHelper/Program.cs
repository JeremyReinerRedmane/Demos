using DemoKatan.mCase;

var commandLineArgs = Environment.GetCommandLineArgs();

if (commandLineArgs.Length > 1)
{
    if (commandLineArgs.Length == 8)
    {
        //directly querying db for datalist id's
        var cmd = new SyncDlConfigs(commandLineArgs);

        var sqlQuery = await cmd.DataAccess();

        await cmd.RemoteSync(sqlQuery);
    }

    if (commandLineArgs.Length == 7)
    {
        //direct access to csv data
        var cmd = new SyncDlConfigs(commandLineArgs);

        var data = cmd.DirectDataAccess();

        await cmd.RemoteSync(data);
    }
    
}
else
{

    var local = new SyncDlConfigs();

    var sqlQuery = await local.DataAccess();

    await local.RemoteSync(sqlQuery);
}

