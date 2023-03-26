namespace CSLox
{
	internal class Token
	{
		internal readonly TokenType Type;
		internal readonly string Lexeme;
		internal readonly object? Literal;
		internal readonly int Line;
		
		internal Token(TokenType type, string lexeme, object? literal, int line)
		{
			this.Type = type;
			this.Lexeme = lexeme;
			this.Literal = literal;
			this.Line = line;
		}
		
		public override string ToString()
		{
			return $"{Type} {Lexeme} {Literal}";
		}
	}
}