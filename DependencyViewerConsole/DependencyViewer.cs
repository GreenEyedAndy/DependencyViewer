using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DependencyViewerConsole;

public class DependencyViewer : IDependencyViewer
{
    private readonly ILogger<DependencyViewer>  log;
    private readonly IConfiguration config;
    private readonly CsProjParser parser;

    public DependencyViewer(ILogger<DependencyViewer> log, IConfiguration config, CsProjParser parser)
    {
        this.log = log;
        this.config = config;
        this.parser = parser;
    }
    
    public string Walk()
    {
        var output = "graph RL" + Environment.NewLine;
        var allCsproj = Directory.GetFiles(config.GetValue<string>("SearchDir"), @"*.csproj", SearchOption.AllDirectories).Select(s => new FileInfo(s));

        foreach (var fi in allCsproj)
        {
            if (fi.Name.StartsWith("Bfe.")  && !fi.Name.Contains(" - "))
            {
                var deps = parser.GetDependencies(fi.FullName);
                foreach (var dep in deps)
                {
                    output += $"{dep.From} --> {dep.To}" + Environment.NewLine;
                }
            }
        }
     
        return output;
    }
}