namespace WebApplication17.Executor;

public class ExecuteRequestDto(string code, string lang, string exerciseId)
{
    public string Code => code;

    public string Lang => lang;
    
    public string ExerciseId => exerciseId;
}