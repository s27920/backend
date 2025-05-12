using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer.AstAnalyzer;
using ExecutorService.Executor._ExecutorUtils;

namespace ExecutorService.Executor;

public interface ICodeExecutorService
{
    public Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto);
    public Task<ExecuteResultDto> DryExecute(ExecuteRequestDto executeRequestDto);
}


public class CodeExecutorService(IExecutorRepository executorRepository, IExecutorConfig executorConfig) : ICodeExecutorService
{
    private const string JavaImport = "import com.google.gson.Gson;\n\n"; //TODO this is temporary, not the gson but the way it's imported

    private IAnalyzer? _analyzer;
    private CodeAnalysisResult? _codeAnalysisResult;
    private const string CompilerServiceUrl = "http://172.21.40.155:5137/compile";

    public async Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto)
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

    
    //TODO add better error handling here
    private async Task<ExecuteResultDto> Exec(UserSolutionData userSolutionData)
    {
        byte[] codeBytes = Encoding.UTF8.GetBytes(userSolutionData.FileContents.ToString());
        string codeB64 = Convert.ToBase64String(codeBytes);

        // TODO Make this reuse http clients
        HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Console.WriteLine(codeB64);
        Console.WriteLine(_codeAnalysisResult!.MainClassName);
        var requestDto = new CompileRequestDto(codeB64, _codeAnalysisResult!.MainClassName);
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestDto),
            Encoding.UTF8,
            "application/json"
        );
        // TODO add polly fallback policy

        var response = await client.PostAsync(CompilerServiceUrl, jsonContent);
        var responseDto = await response.Content.ReadFromJsonAsync<CompileResponseDto>();
        
        if (responseDto is null)
        {
            throw new JavaSyntaxException("Probably compilation failed ig");
        }
        
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/build-copy.sh \"{_codeAnalysisResult.MainClassName}\" \"{responseDto.CompileResponseB64}\" \"{userSolutionData.ExecutionId}\" \"{userSolutionData.SigningKey}\"", 
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
                Arguments = $"/app/fc-scripts/java-exec.sh {userSolutionData.ExecutionId} {userSolutionData.SigningKey}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };

        execProcess.Start();
        await execProcess.WaitForExitAsync();
        
        try
        {
            var output = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-OUT-LOG.log");
            var answ = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-ANSW-LOG.log");
            Console.WriteLine($"output: {output}");
            Console.WriteLine($"testing: {answ}");
            return new ExecuteResultDto(output, answ);
        }
        catch ( FileNotFoundException ex)
        {
            
        }
        return new ExecuteResultDto("No File Found", "");
    }

    private async Task<UserSolutionData> PrepareFile(string codeB64, Language lang, string exerciseId) //TODO Make the import not constant
    {
        byte[] codeBytes = Convert.FromBase64String(codeB64);
        string codeString = Encoding.UTF8.GetString(codeBytes);

        StringBuilder code = new StringBuilder(codeString);
        
        var fileData = new UserSolutionData(Guid.NewGuid(), Guid.NewGuid().ToString(), lang, await executorRepository.GetFuncName(), code, exerciseId);
        code.Insert(0, JavaImport);
        
        return fileData;
    }

    private async Task InsertTestCases(UserSolutionData userSolutionData, int writeOffset)
    {
        TestCase[] testCases = await executorRepository.GetTestCasesAsync();
        
        StringBuilder testCaseInsertBuilder = new StringBuilder();
        testCaseInsertBuilder.Append("Gson gson = new Gson();\n");

        foreach (var testCase in testCases)
        {
            string comparingStatement = $"System.out.println(\"ctr-[{userSolutionData.SigningKey}]-ans: \" + gson.toJson({testCase.ExpectedOutput}).equals(gson.toJson(sortIntArr({testCase.TestInput}))));\n";
            testCaseInsertBuilder.Append(comparingStatement);
        }
        
        userSolutionData.FileContents.Insert(writeOffset, testCaseInsertBuilder);
    }

    private Language CheckLanguageSupported(string lang)
    {
        return executorConfig.GetSupportedLanguages().FirstOrDefault(l => l.Name.Equals(lang)) ?? throw new InvalidOperationException(); // TODO make this a custom languageException
    }
}
