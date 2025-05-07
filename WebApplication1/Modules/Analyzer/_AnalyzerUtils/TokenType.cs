namespace WebApplication1.Modules.Analyzer._AnalyzerUtils;

public enum TokenType
{
    Ident, OpenCurly, CloseCurly, OpenParen, CloseParen, OpenBrace, CloseBrace,

    Assign, 

    Semi,
    
    Class,

    Public, Private, Protected,

    Byte, Short, Int, Long, Float, Double, Char, Boolean, String /*No string in future*/, Void /*Special*/, 

    FloatLit, DoubleLit, CharLit, BooleanLit, IntLit, LongLit, StringLit,
    
    Static, Final,
    
    Import, Package,
    
    Dot, Comma,
    
}