using CSLox.Grammar;
using static CSLox.TokenType;

namespace CSLox
{
	internal class Parser
	{
		private readonly List<Token> tokens;
		private int current = 0;
		private bool isAtEnd => Peek().type == EOF;
		
		internal Parser(List<Token> tokens)
		{
			this.tokens = tokens;
		}
		
		internal List<Stmt> Parse()
		{
			List<Stmt> statements = new List<Stmt>();
			while(!isAtEnd)
			{
				statements.Add(Declaration());
			}
			return statements;
		}
		
		private Expr Expression()
		{
			return Assignment();
			//Expr expr = Equality();
			
			//while(Match(COMMA))
			//{
			//	Token _operator = Previous();
			//	Expr right = Expression();
			//	expr = new Binary(expr, _operator, right);
			//}
			//return expr;
		}
		
		private Stmt Declaration()
		{
			try
			{
				if(Match(VAR)) return VarDeclaration();
				return Statement();
			}
			catch(ParseExecption)
			{
				Synchronize();
				return null;
			}
		}
		
		private Stmt Statement()
		{
			if(Match(FOR)) return ForStatement();
			if(Match(IF)) return IfStatement();
			if(Match(PRINT)) return PrintStatment();
			if(Match(WHILE)) return WhileStatement();
			if(Match(LEFT_BRACE)) return new Block(Block());
			return ExpressionStatment();
		}
		
		private Stmt ForStatement()
		{
			Consume(LEFT_PAREN, "Expected '(' after 'for'");
			Stmt initializer;
			if(Match(SEMICOLON))
			{
				initializer = null;
			}
			else if(Match(VAR))
			{
				initializer = VarDeclaration();
			}
			else
			{
				initializer = ExpressionStatment();
			}
			Expr condition = null;
			if(!Check(SEMICOLON))
			{
				condition = Expression();
			}
			Consume(SEMICOLON, "Expected ';' after loop condition");
			Expr increment = null;
			if(!Check(RIGHT_PAREN)) increment = Expression();
			Consume(RIGHT_PAREN, "Unmatched '('");
			Stmt body = Statement();
			if(increment != null) body = new Block(new List<Stmt>{ body, new Expression(increment) });
			if(condition == null) condition = new Literal(true);
			body = new While(condition, body);
			if(initializer != null) body = new Block(new List<Stmt>{ initializer, body });
			return body;
		}
		
		private Stmt IfStatement()
		{
			Consume(LEFT_PAREN, "Expected '(' after 'if'.");
			Expr condition = Expression();
			Consume(RIGHT_PAREN, "Unmatched '('");
			Stmt thenBranch = Statement();
			Stmt elseBranch = null;
			if(Match(ELSE))
			{
				if(Peek().type == IF)
				{
					Advance();
					elseBranch = IfStatement();
				}
				else
				{
					elseBranch = Statement();
				}
			}
			return new If(condition, thenBranch, elseBranch);
		}
		
		private Stmt PrintStatment()
		{
			Expr value = Expression();
			Consume(SEMICOLON, "Expected ';' after print.");
			return new Print(value);
		}
		
		private Stmt VarDeclaration()
		{
			Token name = Consume(IDENTIFIER, "Expected variable name.");
			Expr initializer = null;
			if(Match(EQUAL))
			{
				initializer = Expression();
			}
			Consume(SEMICOLON, "Expected ';' after variable declaration");
			return new Var(name, initializer);
		}
		
		private Stmt WhileStatement()
		{
			Consume(LEFT_PAREN, "Expected'(' after 'while'.");
			Expr condition = Expression();
			Consume(RIGHT_PAREN, "Unmatched '('");
			Stmt body = Statement();
			return new While(condition, body);
		}
		
		private Stmt ExpressionStatment()
		{
			Expr expr = Expression();
			Consume(SEMICOLON, "Expected ';' after expression.");
			return new Expression(expr);
		}
		
		private List<Stmt> Block()
		{
			List<Stmt> statements = new List<Stmt>();
			while(!Check(RIGHT_BRACE) && !isAtEnd)
			{
				statements.Add(Declaration());
			}
			
			Consume(RIGHT_BRACE, "Expect '}' after block scope.");
			return statements;
		}
		
		private Expr Assignment()
		{
			Expr expr = Equality();
			
			if(Match(EQUAL))
			{
				Token equals = Previous();
				Expr value = Assignment();
				
				if(expr is Variable)
				{
					Token name = ((Variable)expr).name;
					return new Assign(name, value);
				}
				Lox.Error(equals, "Invalid assignment target.");
			}
			return expr;
		}
		
		private Expr Or()
		{
			Expr expr = And();
			while(Match(OR))
			{
				Token _operator = Previous();
				Expr right = And();
				expr = new Logical(expr, _operator, right);
			}
			return expr;
		}
		
		private Expr And()
		{
			Expr expr = Equality();
			while(Match(AND))
			{
				Token _operator = Previous();
				Expr right = Equality();
				expr = new Logical(expr, _operator, right);
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
			if(Match(NUMBER, STRING)) return new Literal(Previous().literal);
			if(Match(IDENTIFIER)) return new Variable(Previous());
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
			return Peek().type == type;
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
				if(Previous().type == SEMICOLON) return;
				
				switch(Peek().type)
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