using System.Xml;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Modules.DocumentReaders
{
    public class PackagesConfigReader
    {
        public List<(string name, NuGetVersion version, Dictionary<string, string> metadata)> GetPackages(string file)
        {
            var document = new XmlDocument();
            document.LoadXml(file);

            var packagesNode = document.SelectSingleNode("packages");
            if (packagesNode == null) return new List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>();

            return (
                from XmlNode packageNode in packagesNode.ChildNodes 
                let packageId = packageNode.Attributes?["id"]?.InnerText 
                let packageVersion = packageNode.Attributes?["version"]?.InnerText 
                where packageId != null && packageVersion != null 
                select (packageId, new NuGetVersion(packageVersion), new Dictionary<string, string>())
                ).ToList();
        }
    }
}
