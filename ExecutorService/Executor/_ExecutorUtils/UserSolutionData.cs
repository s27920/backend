using System.Text;

namespace ExecutorService.Executor._ExecutorUtils;

public class UserSolutionData(Guid guid, Language lang, string funcName, StringBuilder fileContents, string exerciseId)
{
    public Guid Guid => guid;

    public Language Lang => lang;

    public string FuncName => funcName;

    public StringBuilder FileContents => fileContents;

    public string ExerciseId => exerciseId;

}