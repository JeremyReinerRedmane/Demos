using DemoKatan.mCase;

var commandLineArgs = Environment.GetCommandLineArgs();

if (commandLineArgs.Length > 1)
{
    var cmd = new SyncDlConfigs(commandLineArgs);

    await cmd.RemoteSync();
    
}
else
{
    var local = new SyncDlConfigs();

    await local.RemoteSync();
}

