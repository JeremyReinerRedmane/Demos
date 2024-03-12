using System.Data;

namespace DemoKatan.Demos
{
    public class CompareSysNames
    {
        public void Compare(string exportedPath, string configRepoPath)
        {
            var exported = GetSysNames(exportedPath);

            var configRepo = GetSysNames(configRepoPath);

            DoNameComparison(exported, configRepo);
        }

        private IEnumerable<string> GetSysNames(string path)
        {
            var fileNames = ReturnFileNames(path).ToList();

            return from file
                    in fileNames
                   select file.Replace("ListConfigJSON_", "");
        }

        private IEnumerable<string> ReturnFileNames(string path)
        {

            if (!Directory.Exists(path))
                yield return "";

            var dirInfo = new DirectoryInfo(path);

            var files = dirInfo.GetFiles();

            foreach (var file in files)
            {
                yield return file.Name;
            }
        }

        private void DoNameComparison(IEnumerable<string> exported, IEnumerable<string> configRepo)
        {
            var one = exported.Order().ToList();

            var two = configRepo.Order().ToList();

            FindCommonSubstrings(one, two);
        }

        private void FindCommonSubstrings(List<string> exported, List<string> configRepo)
        {
            HashSet<string> sameInBoth = new HashSet<string>();
            // Check if any element in the longer list is a substring of elements in the shorter set

            var invalidCahrs = Path.GetInvalidFileNameChars();

            foreach (var x in configRepo.Select(item => item.Replace(".json", "")))
            {
                foreach (var config in exported)
                {
                    var y = config.Replace(".json", "");

                    var sysName = config.Select(list => new string(config.Where(c => !invalidCahrs
                                .Contains(c))
                            .ToArray()))
                        .FirstOrDefault() ?? string.Empty;

                    if (x != y)
                        continue;

                    sameInBoth.Add(x);
                    break;
                }
            }

            //var exportedSet = new HashSet<string>(exported);

            //var configSet = new HashSet<string>(configRepo);

            //var x = exportedSet.Intersect(configSet);

            //var y = exportedSet.Union(configSet);

            //var z = y.Except(x).ToList();

            FindLastUnknownFiles(exported, configRepo);
        }

        private void FindLastUnknownFiles(List<string> exported, List<string> configRepo)
        {

            var temp = Path.GetTempPath();

            var dir = Path.Combine(temp, Guid.NewGuid().ToString());

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Console.WriteLine("Temp Dir: {0}", dir);

            File.WriteAllText(Path.Combine(dir, "config.txt"), string.Join('\n', configRepo));
            File.WriteAllText(Path.Combine(dir, "Exported.txt"), string.Join('\n', exported));

            Console.ReadKey();
        }
    }
}
