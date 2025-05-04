using System.Diagnostics;

namespace WebApplication17.Executor;

public interface IExecutorConfig
{
    public Language[] GetSupportedLanguages();
}

public class ExecutorConfig : IExecutorConfig
{
    private readonly Language[] _supportedLanguages;

    public Language[] GetSupportedLanguages()
    {
        return _supportedLanguages;
    }

    public ExecutorConfig()
    {
        IExecutorRepository executorRepository = new ExecutorRepositoryMock();
        _supportedLanguages = executorRepository.GetSupportedLangsAsync().Result; 
        BuildImages();
    }
    
    
    private void BuildImages()
    {
        var shBuildArgs = string.Join(" ", _supportedLanguages.Select(arg => $"\"{arg.Name}\""));
        
        var buildProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "/bin/sh",
                Arguments = $"\"./scripts/build-images.sh\" {shBuildArgs}",
            }
        };
        buildProcess.Start();
        buildProcess.WaitForExit();
    }
}
