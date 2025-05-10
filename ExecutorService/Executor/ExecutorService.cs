using System.Diagnostics;
using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer.AstAnalyzer;
using ExecutorService.Executor._ExecutorUtils;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Win32.SafeHandles;

namespace ExecutorService.Executor;

public interface IExecutorService
{
    public Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto);
    public Task<ExecuteResultDto> DryExecute(ExecuteRequestDto executeRequestDto);
}



//TODO could clean up (meaning cut down on) some of the file handling, a bit too much of it going on
public class ExecutorService(IExecutorRepository executorRepository, IExecutorConfig executorConfig) : IExecutorService
{
    private const string JavaImport = "import com.google.gson.Gson;\n\n"; //TODO this is temporary, not the gson but the way it's imported

    private IAnalyzer? _analyzer;
    private CodeAnalysisResult? _codeAnalysisResult;

    public async Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto)
    {
        Language lang;
        try
        {
            lang = CheckLanguageSupported(executeRequestDto.Lang);
        }
        catch (InvalidOperationException e)
        {
            return new ExecuteResultDto("", "Err: unsupported language");
        }
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, executeRequestDto.ExerciseId);

        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString());
        _codeAnalysisResult = _analyzer.AnalyzeUserCode();
        
        if (!_codeAnalysisResult.PassedValidation)
        {
            return new ExecuteResultDto("", "critical function signature modified. Exiting.");
        }
        
        if (_codeAnalysisResult.MainMethodIndices is not null)
        {
            await InsertTestCases(fileData, _codeAnalysisResult.MainMethodIndices.MethodFileEndIndex);
        }
        else
        {
            //TODO temporary solution I'd like to insert a main if it's not found to test either way
            return new ExecuteResultDto("", "no main found. Exiting");
        }

        // return new ExecuteResultDto($"{await File.ReadAllTextAsync(fileData.FilePath)}", ""); /*[DEBUG]*/
        return await Exec(fileData);
    }

    public async Task<ExecuteResultDto> DryExecute(ExecuteRequestDto executeRequestDto)
    {
        Language lang;
        try
        {
            lang = CheckLanguageSupported(executeRequestDto.Lang);
        }
        catch (InvalidOperationException)
        {
            return new ExecuteResultDto("", "Err: unsupported language");
        }
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, executeRequestDto.ExerciseId);
        
        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString());
        _codeAnalysisResult = _analyzer.AnalyzeUserCode();

        return await Exec(fileData);
    }

    private async Task<ExecuteResultDto> Exec(SrcFileData srcFileData)
    {
        byte[] codeBytes = Encoding.UTF8.GetBytes(srcFileData.FileContents.ToString());
        string codeB64 = Convert.ToBase64String(codeBytes);
        
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/build-copy.sh {_codeAnalysisResult!.MainClassName} {codeB64} {srcFileData.Guid}", 
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
        
        buildProcess.Start();
        await buildProcess.WaitForExitAsync();
        // TODO add success or failure check

        var execProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/java-exec.sh {srcFileData.Guid}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };

        execProcess.Start();
        await execProcess.WaitForExitAsync();
        
        var output = await execProcess.StandardOutput.ReadToEndAsync();
        var error = await execProcess.StandardError.ReadToEndAsync();
        
        return new ExecuteResultDto(output, error);
    }

    private async Task<SrcFileData> PrepareFile(string codeB64, Language lang, string exerciseId) //TODO Make the import not constant
    {
        byte[] codeBytes = Convert.FromBase64String(codeB64);
        string codeString = Encoding.UTF8.GetString(codeBytes);

        StringBuilder code = new StringBuilder(codeString);
        
        var fileData = new SrcFileData(Guid.NewGuid(), lang, await executorRepository.GetFuncName(), code);

        code.Insert(0, JavaImport);
        
        return fileData;
    }

    private async Task InsertTestCases(SrcFileData srcFileData, int writeOffset)
    {
        TestCase[] testCases = await executorRepository.GetTestCasesAsync();
        
        StringBuilder testCaseInsertBuilder = new StringBuilder();
        testCaseInsertBuilder.Append("Gson gson = new Gson();\n");

        foreach (var testCase in testCases)
        {
            string comparingStatement = $"System.out.println(gson.toJson({testCase.ExpectedOutput}).equals(gson.toJson(sortIntArr({testCase.TestInput}))));\n";
            testCaseInsertBuilder.Append(comparingStatement);
        }
        
        srcFileData.FileContents.Insert(writeOffset, testCaseInsertBuilder);
    }

    private Language CheckLanguageSupported(string lang)
    {
        return executorConfig.GetSupportedLanguages().FirstOrDefault(l => l.Name.Equals(lang)) ?? throw new InvalidOperationException(); // TODO make this a custom languageException
    }
}