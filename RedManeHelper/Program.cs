
using DemoKatan.mCase;

string[] commandLineArgs = Environment.GetCommandLineArgs();

var config = new SyncDlConfigs(commandLineArgs);

await config.BeginSync();
