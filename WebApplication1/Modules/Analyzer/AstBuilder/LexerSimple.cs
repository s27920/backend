using System.Text;
using WebApplication17.Analyzer._AnalyzerUtils;

namespace WebApplication17.Analyzer.AstBuilder;

public interface ILexer
{
    public List<Token> Tokenize(string fileContents);
}

public class LexerSimple : ILexer
{
    private char[] _fileChars;
    private int _currPos;
    private StringBuilder _buf;
    private List<Token> _tokens;
    
    public List<Token> Tokenize(string fileContents)
    {
         _tokens = new List<Token>();
        _fileChars = fileContents.ToCharArray();
        _buf = new StringBuilder();
        _currPos = 0;

        while (PeekChar() != null)
        {
            char consumedChar = ConsumeChar();
            switch (consumedChar)
            {
                case '/':
                    if (CheckForChar('/', 1))
                    {
                        ConsumeComment();
                    }else if (CheckForChar('*', 1))
                    {
                        ConsumeMultiLineComment();
                    }
                    break;
                case '{':
                    _tokens.Add(CreateToken(TokenType.OpenCurly));
                    break;
                case '}':
                    _tokens.Add(CreateToken(TokenType.CloseCurly));
                    break;
                case '[':
                    _tokens.Add(CreateToken(TokenType.OpenBrace));
                    break;
                case ']':
                    _tokens.Add(CreateToken(TokenType.CloseBrace));
                    break;
                case '(':
                    _tokens.Add(CreateToken(TokenType.OpenParen));
                    break;
                case ')':
                    _tokens.Add(CreateToken(TokenType.CloseParen));
                    break;
                case '=':
                    _tokens.Add(CreateToken(TokenType.Assign));
                    break;
                case ';':
                    _tokens.Add(CreateToken(TokenType.Semi));
                    break;
                case '.':
                    _tokens.Add(CreateToken(TokenType.Dot));
                    break;
                case ',':
                    _tokens.Add(CreateToken(TokenType.Comma));
                    break;
                    
                default:
                    if (Char.IsLetter(consumedChar))
                    {
                        _buf.Append(consumedChar);
                        _tokens.Add(ConsumeKeyword(_buf));
                    }else if (Char.IsWhiteSpace(consumedChar))
                    {
                        
                    }
                    break;
            }
        }

        // foreach (var tok in _tokens)
        // {
            // Console.WriteLine(tok.Type);
        // }
        return _tokens;
    }
    
    private Token ConsumeKeyword(StringBuilder buf)
    {
        // buf.Append(ConsumeChar());
        while (PeekChar() != null && Char.IsLetterOrDigit(PeekChar().Value)) //no it can't be a null but thank you Rider
        {
            buf.Append(ConsumeChar());
        }

        string result = buf.ToString();
        Token token = result switch
        {
            "private" => CreateToken(TokenType.Private),
            "public" => CreateToken(TokenType.Public),
            "protected" => CreateToken(TokenType.Protected),
            "void" => CreateToken(TokenType.Void),
            "byte" => CreateToken(TokenType.Byte),
            "short" => CreateToken(TokenType.Short),
            "int" => CreateToken(TokenType.Int),
            "long" => CreateToken(TokenType.Long),
            "float" => CreateToken(TokenType.Float),
            "double" => CreateToken(TokenType.Double),
            "char" => CreateToken(TokenType.Char),
            "boolean" => CreateToken(TokenType.Boolean),
            "static" => CreateToken(TokenType.Static),
            "final" => CreateToken(TokenType.Final),
            "class" => CreateToken(TokenType.Class),
            "String" => CreateToken(TokenType.String),
            "import" => CreateToken(TokenType.Import),
            _ => CreateToken(TokenType.Ident, result),
        };
        buf.Clear();

        return token;
    }

    private void ConsumeComment()
    {
        ConsumeChar(); //consume '/'
        ConsumeChar(); //consume '/'
        while (PeekChar() != null && !(CheckForChar('\n') || CheckForChar('\r')))
        {
            ConsumeChar();
        }

        ConsumeChar(); // //consume '\n' or '\r'
        if (CheckForChar('\n')) //for windows
        {
            ConsumeChar(); 
        }
    }

    private void ConsumeMultiLineComment()
    {
        ConsumeChar(); //consume '/'
        ConsumeChar(); //consume '/*'
        
        while (PeekChar() != null && !(CheckForChar('*') && CheckForChar('/', 1)))
        {
            ConsumeChar();
        }

        if (PeekChar() is not null)
        {
            ConsumeChar(); //consume '*'
        }
        if (PeekChar() is not null)
        {
            ConsumeChar(); //consume '/'
        }
    }

    private bool CheckForChar(char checkedChar, int offset = 0)
    {
        return PeekChar(offset) == checkedChar;
    }
    private char? PeekChar(int offset = 0)
    {
        int accessIndex = offset + _currPos;
        if (accessIndex < _fileChars.Length)
        {
            return _fileChars[accessIndex];
        }

        return null;
    }
    
    private Token CreateToken(TokenType type, string? value = null)
    {
        return new Token(type, _currPos - 1, value);
    }
    private char ConsumeChar()
    {
        return _fileChars[_currPos++];
    }
}