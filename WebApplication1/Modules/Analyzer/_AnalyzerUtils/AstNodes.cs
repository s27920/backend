using OneOf;

namespace WebApplication17.Analyzer._AnalyzerUtils;

public enum MemberType
{
    Byte, Short, Int, Long, Float, Double, Char, Boolean, String /*Formally incorrect but it allows us to get away with certain simpler solutions for now*/,
}

public enum SpecialMemberType
{
    Void 
}

public enum UnaryType
{
    ArrDereference, Incr, Decr
}

public enum BinaryOperator
{
    Sum, Diff, Mul, Div, Mod, And, Or, Xor
}

public enum LogicalOperator
{
    And, Or, Xor
}

public enum AccessModifier
{
    Public, Private, Protected
}

public enum MemberModifier
{
    Static, Final
}

public enum TopLevelStatement
{
    Import, Package
}

public class ArrayType
{
    public OneOf<MemberType> BaseType { get; set; }
    public int Dim { get; set; }
}

public class AstNodeProgram
{
    public List<OneOf<AstNodeClass, AstNodeTopLevelStat>> ProgramClasses { get; set; } = new(); //TODO change this, confusing naming
}

public class AstNodeTopLevelStat
{
    public TopLevelStatement TopLevelStatement { get; set; }
    public string? Uri { get; set; }
}

public class AstNodeClass
{
    public AccessModifier ClassAccessModifier { get; set; } = AccessModifier.Private;
    public Token Identifier { get; set; }
    public AstNodeCLassScope? ClassScope { get; set; }
}

public class AstNodeCLassScope
{
    public List<AstNodeClassMember> ClassMembers { get; } = new List<AstNodeClassMember>();
    public int ScopeBeginOffset { get; set; }
    public int ScopeEndOffset { get; set; }
}

public class AstNodeClassMember
{
    public OneOf<AstNodeClassMemberFunc, AstNodeClassMemberVar> ClassMember { get; set; }
}

public class AstNodeClassMemberFunc
{
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public List<MemberModifier> Modifiers { get; set; } = new List<MemberModifier>();
    public OneOf<MemberType,SpecialMemberType, ArrayType>? FuncReturnType { get; set; }
    public Token? Identifier { get; set; }
    public List<AstNodeScopeMemberVar> FuncArgs { get; set; } = new List<AstNodeScopeMemberVar>();
    public AstNodeStatementScope? FuncScope { get; set; }
}

public class AstNodeClassMemberVar
{
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public AstNodeScopeMemberVar ScopeMemberVar { get; set; } = new AstNodeScopeMemberVar();
}

public class AstNodeScopeMemberVar
{
    public List<MemberModifier>? VarModifiers { get; set; }
    public OneOf<MemberType, ArrayType> Type { get; set; }
    public Token? Identifier { get; set; }
    public Token? LitValue { get; set; }
}

public class AstNodeStatementScope
{
    public int ScopeBeginOffset { get; set; }
    public int ScopeEndOffset { get; set; }
    public List<AstNodeStatement> ScopedStatements { get; set; } = new List<AstNodeStatement>();
}

public class AstNodeExpr
{
    public OneOf<AstNodeBinExpr, AstNodeUnaryExpr, AstNodeExprIdent>? Variant { get; set; }
}

public class AstNodeUnaryExpr
{
    public AstNodeExpr? Operand { get; set; }
}

public class AstNodeBinExpr
{
    public AstNodeExpr? ExprLhs { get; set; }
    public AstNodeExpr? ExprRhs { get; set; }
}

public class AstNodeLit
{
    public TokenType LitToken { get; set; }
}

public class AstNodeExprIdent
{
    public Token? Ident { get; set; }
}

public class AstNodeStatExpr
{
    public AstNodeExpr? Expr { get; set; }
}

public class AstNodeStatement
{
    public OneOf<AstNodeStatementScope, AstNodeStatementUnknown> Variant { get; set; }
}

public class AstNodeStatementUnknown
{
    public Token Ident { get; }

    public AstNodeStatementUnknown(Token ident)
    {
        Ident = ident;
    }
}