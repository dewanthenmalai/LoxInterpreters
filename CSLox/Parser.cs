using CSLox.Grammar;
using static CSLox.TokenType;

namespace CSLox
{
	internal class Parser
	{
		private readonly List<Token> tokens;
		private int current = 0;
		private bool isAtEnd => Peek().Type == EOF;
		
		internal Parser(List<Token> tokens)
		{
			this.tokens = tokens;
		}
		
		internal Expr Parse()
		{
			try
			{
				return Expression();
			}
			catch(ParseExecption)
			{
				return null;
			}
		}
		
		private Expr Expression()
		{
			Expr expr = Equality();
			
			while(Match(COMMA))
			{
				Token _operator = Previous();
				Expr right = Expression();
				expr = new Binary(expr, _operator, right);
			}
			return expr;
		}
		
		private Expr Equality()
		{
			Expr expr = Comparison();
			
			while(Match(BANG_EQUAL, EQUAL_EQUAL))
			{
				Token _operator = Previous();
				Expr right = Comparison();
				expr = new Binary(expr, _operator, right);
			}
			
			return expr;
		}
		
		private Expr Comparison()
		{
			Expr expr = Term();
			
			while(Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
			{
				Token _operator = Previous();
				Expr right = Term();
				expr = new Binary(expr, _operator, right);
			}
			
			return expr;
		}
		
		private Expr Term()
		{
			Expr expr = Factor();
			
			while(Match(PLUS, MINUS))
			{
				Token _operator = Previous();
				Expr right = Factor();
				expr = new Binary(expr, _operator, right);
			}
			
			return expr;
		}
		
		private Expr Factor()
		{
			Expr expr = Unary();
			
			while(Match(SLASH, STAR))
			{
				Token _operator = Previous();
				Expr right = Unary();
				expr = new Binary(expr, _operator, right);
			}
			
			return expr;
		}
		
		private Expr Unary()
		{
			if(Match(BANG, MINUS))
			{
				Token _operator = Previous();
				Expr right = Unary();
				return new Unary(_operator, right);
			}
			if(Match(BANG_EQUAL, EQUAL_EQUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL, PLUS, SLASH, STAR)) throw Exception(Previous(), "Missing left operand of binary operator");
			
			return Primary();
		}
		
		private Expr Primary()
		{
			if(Match(FALSE)) return new Literal(false);
			if(Match(TRUE)) return new Literal(true);
			if(Match(NIL)) return new Literal(null);
			if(Match(NUMBER, STRING)) return new Literal(Previous().Literal);
			if(Match(LEFT_PAREN))
			{
				Expr expr = Expression();
				Consume(RIGHT_PAREN, "Expect ')' after expression");
				return new Grouping(expr);
			}
			
			throw Exception(Peek(), "Expect expression.");
		}
		
		private bool Match(params TokenType[] types)
		{
			foreach(TokenType type in types)
			{
				if(Check(type))
				{
					Advance();
					return true;
				}
			}
			return false;
		}
		
		private Token Consume(TokenType type, String message)
		{
			if(Check(type)) return Advance();
			throw Exception(Peek(), message);
		}
		
		private bool Check(TokenType type)
		{
			if(isAtEnd) return false;
			return Peek().Type == type;
		}
		
		private Token Advance()
		{
			if(!isAtEnd) current++;
			return Previous();
		}
		
		private Token Peek() => tokens[current];
		
		private Token Previous() => tokens[current - 1];
		
		private ParseExecption Exception(Token token, string message)
		{
			Lox.Error(token, message);
			return new ParseExecption();
		}
		
		private void Synchronize()
		{
			Advance();
			
			while(!isAtEnd)
			{
				if(Previous().Type == SEMICOLON) return;
				
				switch(Peek().Type)
				{
					case CLASS:
					case FUN:
					case VAR:
					case FOR:
					case IF:
					case WHILE:
					case PRINT:
					case RETURN:
						return;
				}
				
				Advance();
			}
		}
	}
	
	internal class ParseExecption : Exception
	{
		
	}
}