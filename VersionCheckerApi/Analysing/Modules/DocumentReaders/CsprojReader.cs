using System.Xml;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Modules.DocumentReaders
{
    public class CsprojReader
    {
        private readonly XmlDocument _doc;
        private readonly XmlNamespaceManager _mgr;
        public CsprojReader(string file)
        {
            _doc = new XmlDocument();
            _doc.LoadXml(file);

            _mgr = new XmlNamespaceManager(_doc.NameTable);
            _mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");
        }

        public List<(string name, NuGetVersion version, Dictionary<string, string> metadata)> GetPackages()
        {
            var packageReferences = _doc.SelectNodes("//PackageReference");

            var list = new List<(string, NuGetVersion version, Dictionary<string, string> metadata)>();
            foreach (XmlElement packageRef in packageReferences!)
            {
                var include = packageRef.GetAttribute("Include");
                var version = packageRef.GetAttribute("Version");
                if (include == "") continue;

                var calculatedVersion = version == "" ? TryGetOldProjectVersion(out var v) ? v : new NuGetVersion("0.0.0") : new NuGetVersion(version);
                list.Add((include, calculatedVersion!, new Dictionary<string, string>()));
            }

            return list;
        }

        // if a package in an old csproj file has no specified version it should register as the projectversion
        // if a package is in a newer project and has no version info it will take the lowest???
        private bool TryGetOldProjectVersion(out NuGetVersion? version)
        {
            version = null;

            var node = _doc.SelectSingleNode("//x:TargetFrameworkVersion", _mgr);
            if (node == null) return false;

            version = new NuGetVersion(node.Value![1..]);
            return true;
        }

        public string? GetFrameworkVersion()
        {
            var oldNode = _doc.SelectSingleNode("//x:TargetFrameworkVersion", _mgr);
            if (oldNode != null)
            {
                return $".net Framework {oldNode.InnerText}";
            }

            var newNode = _doc.SelectSingleNode("//TargetFramework");
            return newNode?.InnerText;
        }

        public bool HasPackageReferences()
        {
            return _doc.SelectNodes("//PackageReference")?.Count > 0;
        }
    }
}