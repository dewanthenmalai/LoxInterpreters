using static CSLox.TokenType;

namespace CSLox
{
	internal class Scanner
	{
		private readonly string source;
		private readonly List<Token> tokens = new List<Token>();
		private int start = 0;
		private int current = 0;
		private int line = 1;
		private bool isAtEnd => (current >= source.Length);
		private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
		{
			{"and", AND},
			{"class", CLASS},
			{"else", ELSE},
			{"false", FALSE},
			{"for", FOR},
			{"fun", FUN},
			{"if", IF},
			{"nil", NIL},
			{"or", OR},
			{"print", PRINT},
			{"return", RETURN},
			{"super", SUPER},
			{"this", THIS},
			{"true", TRUE},
			{"var", VAR},
			{"while", WHILE}
		};
		
		internal Scanner(string source)
		{
			this.source = source;
		}
		
		internal List<Token> ScanTokens()
		{
			while(!isAtEnd)
			{
				start = current;
				ScanToken();
			}
			
			tokens.Add(new Token(TokenType.EOF, "", null, line));
			return tokens;
		}
		
		private void ScanToken()
		{
			char c = Advance();
			{
				switch(c)
				{
					case '(': AddToken(LEFT_PAREN); break;
					case ')': AddToken(RIGHT_PAREN); break;
					case '{': AddToken(LEFT_BRACE); break;
					case '}': AddToken(RIGHT_BRACE); break;
					case ',': AddToken(COMMA); break;
					case '.': AddToken(DOT); break;
					case '-': AddToken(MINUS); break;
					case '+': AddToken(PLUS); break;
					case ';': AddToken(SEMICOLON); break;
					case '*': AddToken(STAR); break;
					case '!':
						AddToken(Match('=') ? BANG_EQUAL : BANG); break;
					case '=':
						AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
					case '<':
						AddToken(Match('=') ? LESS_EQUAL : LESS); break;
					case '>':
						AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
					case '/':
						CheckSlash(); break;
					case ' ':
					case '\r':
					case '\t':
						break;
					case '\n':
						line++; break;
					case'"':
						String(); break;
					case 'o':
						if(Match('r')) AddToken(OR); break;
					default:
						Default(c); break;
				}
			}
		}
		
		private void Default(char c)
		{
			if(IsDigit(c))
			{
				Number();
			}
			else if(IsAlpha(c))
			{
				Identifier();
			}
			else
			{
				Lox.Error(line, @$"Unexpected character '{c}'.");
			}
		}
		
		private void Number()
		{
			while(IsDigit(Peek())) Advance();
			//check decmial part
			if(Peek() == '.' && IsDigit(PeekNext()))
			{
				Advance();
				while(IsDigit(Peek())) Advance();
			}
			int length = current - start;
			AddToken(NUMBER, Double.Parse(source.Substring(start, length)));
		}
		
		private void Identifier()
		{
			while(IsAlphaNumeric(Peek())) Advance();
			int length = current - start;
			string text = source.Substring(start, length);
			TokenType type = IDENTIFIER;
			if(keywords.ContainsKey(text))
			{
				type = keywords[text];
			}
			AddToken(type);
		}
		
		private char Advance()
		{
			return source[current++];
		}
		
		private void AddToken(TokenType type)
		{
			AddToken(type, null);
		}
		
		private void AddToken(TokenType type, object literal)
		{
			int length = current - start;
			string text = source.Substring(start, length);
			tokens.Add(new Token(type, text, literal, line));
		}
		
		private bool Match(char expected)
		{
			if(isAtEnd) return false;
			if(source[current] != expected) return false;
			current++;
			return true;
		}
		
		private char Peek()
		{
			if(isAtEnd) return '\0';
			return source[current];
		}
		
		private void CheckSlash()
		{
			if(Match('/'))
			{
				while(Peek() != '\n' && !isAtEnd) Advance();
			}
			else
			{
				AddToken(SLASH);
			}
		}
		
		private void String()
		{
			while(Peek() != '"' && !isAtEnd)
			{
				if(Peek() == '\n') line++;
				Advance();
			}
			if(isAtEnd)
			{
				Lox.Error(line, "Unterminated string.");
				return;
			}
			Advance();
			int length = (current - (start + 1) - 1);
			string value = source.Substring(start+1, length);
			AddToken(STRING, value);
		}
		
		private bool IsDigit(char c)
		{
			return (c >= '0' && c <= '9');
		}
		
		private char PeekNext()
		{
			if(current + 1 >= source.Length) return '\0';
			return source[current+1];
		}
		
		private bool IsAlpha(char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
		}
		
		private bool IsAlphaNumeric(char c)
		{
			return IsAlpha(c) || IsDigit(c);
		}
	}
}