using CSLox.Expressions;
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
		
		internal Expression? Parse()
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
		
		private Expression Expression()
		{
			return Equality();
		}
		
		private Expression Equality()
		{
			Expression expr = Comparison();
			
			while(Match(BANG_EQUAL, EQUAL_EQUAL))
			{
				Token _operator = Previous();
				Expression right = Comparison();
				expr = new Binary(expr, _operator, right);
			}
			
			return expr;
		}
		
		private Expression Comparison()
		{
			Expression expr = Term();
			
			while(Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
			{
				Token _operator = Previous();
				Expression right = Term();
				expr = new Binary(expr, _operator, right);
			}
			
			return expr;
		}
		
		private Expression Term()
		{
			Expression expr = Factor();
			
			while(Match(PLUS, MINUS))
			{
				Token _operator = Previous();
				Expression right = Factor();
				expr = new Binary(expr, _operator, right);
			}
			
			return expr;
		}
		
		private Expression Factor()
		{
			Expression expr = Unary();
			
			while(Match(SLASH, STAR))
			{
				Token _operator = Previous();
				Expression right = Unary();
				expr = new Binary(expr, _operator, right);
			}
			
			return expr;
		}
		
		private Expression Unary()
		{
			if(Match(BANG, MINUS))
			{
				Token _operator = Previous();
				Expression right = Unary();
				return new Unary(_operator, right);
			}
			
			return Primary();
		}
		
		private Expression Primary()
		{
			if(Match(FALSE)) return new Literal(false);
			if(Match(TRUE)) return new Literal(true);
			if(Match(NIL)) return new Literal(null);
			if(Match(NUMBER, STRING)) return new Literal(Previous().Literal);
			if(Match(LEFT_PAREN))
			{
				Expression expr = Expression();
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