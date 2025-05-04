namespace WebApplication17.Executor;

public class SrcFileData(Guid guid, string lang, string funcName)
{
    public Guid Guid => guid;

    public string Lang => lang;

    public string FuncName => funcName;
        
    public string FilePath => $"client-src/{lang}/{guid.ToString()}.java";
}