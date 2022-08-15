using System.Xml;
using Microsoft.Extensions.Logging;

namespace DependencyViewerConsole;

public class CsProjParser
{
    private readonly ILogger<CsProjParser> log;

    public CsProjParser(ILogger<CsProjParser> log)
    {
        this.log = log;
    }

    public IEnumerable<Dependency> GetDependencies(string filename)
    {
        if (IsNewCsprojFile(filename))
        {
            return GetDependenciesNew(filename);
        }
        else
        {
            return GetDependenciesOld(filename);
        }
    }

    private IEnumerable<Dependency> GetDependenciesNew(string filename)
    {
        var fi = new FileInfo(filename);
        var dllName = fi.Name.Replace(fi.Extension, "");
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.Load(filename);
        var references = doc.SelectNodes($"/Project/ItemGroup/PackageReference[starts-with(@Include, 'Bfe')]");
        if (references != null)
        {
            foreach (XmlNode reference in references)
            {
                yield return new Dependency(dllName, reference.Attributes?["Include"]?.Value ?? "unknown");
            }
        }
    }

    private IEnumerable<Dependency> GetDependenciesOld(string filename)
    {
        var fi = new FileInfo(filename);
        var dllName = fi.Name.Replace(fi.Extension, "");
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.Load(filename);
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
        var msXmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
        namespaceManager.AddNamespace("ms", msXmlns);

        // Finde alle Include Zeilen mit dieser PackageId
        var references = doc.SelectNodes($"/ms:Project/ms:ItemGroup/ms:Reference[starts-with(@Include, 'Bfe')]",
            namespaceManager);
        if (references != null)
        {
            foreach (XmlNode reference in references)
            {
                var toDllName = reference.Attributes?["Include"]?.Value ?? "unknown";
                toDllName = toDllName.Remove(toDllName.IndexOf(','));
                yield return new Dependency(dllName, toDllName);
            }
        }
    }

    public bool IsNewCsprojFile(string filename)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.Load(filename);
        var projectNode = doc.SelectSingleNode("Project");
        if (projectNode == null)
        {
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            var msXmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            namespaceManager.AddNamespace("ms", msXmlns);
            projectNode = doc.SelectSingleNode("ms:Project", namespaceManager);
        }
        if (projectNode != null)
        {
            if (projectNode?.Attributes?["Sdk"] == null)
            {
                log.LogInformation("Filename {FileName} old", filename);
            }
            else
            {
                log.LogInformation("Filename {FileName} new", filename);
                return true;
            }
        }
        return false;
    }
}