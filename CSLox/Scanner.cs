namespace CSLox
{
	internal class Scanner
	{
		private readonly string source;
		private readonly IEnumerable<Token> tokens = new List<Token>();
		private int start = 0;
		private int current = 0;
		private int line = 1;
		
		internal Scanner(string source)
		{
			this.source = source;
		}
		
		internal IEnumerable<Token> ScanTokens()
		{
			while(!IsAtEnd())
			{
				start = current;
				ScanToken();
			}
			
			tokens.Append(new Token(TokenType.EOF, "", null, line));
			return tokens;
		}
		
		private bool IsAtEnd()
		{
			return current >= source.Length;
		}
	}
}