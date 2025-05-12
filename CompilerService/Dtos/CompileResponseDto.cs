namespace CompilerService.Dtos;

public class CompileResponseDto(string compileResponse)
{
    public string CompileResponseB64 => compileResponse; 
}