using System.Text;

namespace ExecutorService.Executor._ExecutorUtils;

public class SrcFileData(Guid guid, Language lang, string funcName, StringBuilder fileContents)
{
    public Guid Guid => guid;

    public Language Lang => lang;

    public string FuncName => funcName;

    public StringBuilder FileContents => fileContents;

}