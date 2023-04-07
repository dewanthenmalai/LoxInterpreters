using CSLox.Grammar;
using static CSLox.TokenType;

namespace CSLox
{
	internal class Parser
	{
		#region Members
		
		private readonly List<Token> tokens;
		private int current = 0;
		private bool isAtEnd => Peek().type == EOF;
		private int loopDepth;
		
		#endregion
		
		#region Constructors
		
		internal Parser(List<Token> tokens)
		{
			this.tokens = tokens;
		}
		
		#endregion
		
		#region Internal Methods
		
		internal List<Stmt> Parse()
		{
			List<Stmt> statements = new List<Stmt>();
			while(!isAtEnd)
			{
				statements.Add(Declaration());
			}
			return statements;
		}
		
		#endregion
		
		#region Stmt Methods
		
		private Stmt Declaration()
		{
			try
			{
				if(Match(CLASS)) return ClassDeclaration();
				if(Match(FUN)) return Function("function");
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
			if(Match(BREAK)) return BreakStatement();
			if(Match(CONTINUE)) return ContinueStatement();
			if(Match(RETURN)) return ReturnStatment();
			if(Match(WHILE)) return WhileStatement();
			if(Match(LEFT_BRACE)) return new Block(Block());
			return ExpressionStatment();
		}
		
		private List<Stmt> Block()
		{
			List<Stmt> statements = new List<Stmt>();
			while(!Check(RIGHT_BRACE) && !isAtEnd)
			{
				statements.Add(Declaration());
			}
			
			Consume(RIGHT_BRACE, "Unmatched '{'");
			return statements;
		}
		
		private Stmt ClassDeclaration()
		{
			Token name = Consume(IDENTIFIER, "Expected class name.");
			Variable baseclass = null;
			if(Match(LESS))
			{
				Consume(IDENTIFIER, "Expected base class name.");
				baseclass = new Variable(Previous());
			}
			Consume(LEFT_BRACE, "Expected '{' before class body.");
			List<Function> methods = new List<Function>();
			while(!Check(RIGHT_BRACE) && !isAtEnd)
			{
				methods.Add(Function("method"));
			}
			Consume(RIGHT_BRACE, "Unmatched'{'");
			return new Class(name, baseclass, methods);
		}
		
		private Function Function(string kind)
		{
			Token name = Consume(IDENTIFIER, $"Expected {kind} name.");
			Consume(LEFT_PAREN, $"Expected '(' after {kind} name.");
			List<Token> parameters = new List<Token>();
			if(!Check(RIGHT_PAREN))
			{
				do
				{
					if(parameters.Count >= 255)
					{
						Lox.Error(Peek(), "Cannot use more than 255 arguemnts.");
					}
					parameters.Add(Consume(IDENTIFIER, "Expected parameter name."));
				} while(Match(COMMA));
			}
			Consume(RIGHT_PAREN, "Unmatched '('");
			
			Consume(LEFT_BRACE, $"Expected '{{' before {kind} body.");
			List<Stmt> body = Block();
			return new Function(name, parameters, body);
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
		
		private Stmt ExpressionStatment()
		{
			Expr expr = Expression();
			Consume(SEMICOLON, "Expected ';' after expression.");
			return new Expression(expr);
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
			
			try
			{
				loopDepth++;
				Stmt body = Statement();
				Expression incrementExpression = null;
				if(increment != null)
				{
					incrementExpression = new Expression(increment);
					body = new Block(new List<Stmt>{ body, incrementExpression });
				}
				if(condition == null) condition = new Literal(true);
				body = new While(condition, body, incrementExpression);
				if(initializer != null) body = new Block(new List<Stmt>{ initializer, body });
				return body;
			}
			finally
			{
				loopDepth--;
			}
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
				if(Match(IF))
				{
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
			Consume(SEMICOLON, "Expected ';' after 'print'.");
			return new Print(value);
		}
		
		private Stmt BreakStatement()
		{
			Token keyword = Previous();
			if(loopDepth == 0) Lox.Error(keyword, "Must be in a loop to use 'break'.");
			Consume(SEMICOLON, "Expected ';' after 'break'.");
			return new Break(keyword);
		}
		
		private Stmt ContinueStatement()
		{
			Token keyword = Previous();
			Consume(SEMICOLON, "Expected ';' after 'continue'.");
			return new Continue(keyword);
		}
		
		private Stmt ReturnStatment()
		{
			Token keyword = Previous();
			Expr value = null;
			if(!Check(SEMICOLON))
			{
				value = Expression();
			}
			Consume(SEMICOLON, "Expected ';' after return statement.");
			return new Return(keyword, value);
		}
		
		
		
		private Stmt WhileStatement()
		{
			Consume(LEFT_PAREN, "Expected'(' after 'while'.");
			Expr condition = Expression();
			Consume(RIGHT_PAREN, "Unmatched '('");
			try
			{
				loopDepth++;
				Stmt body = Statement();
				return new While(condition, body, null);
			}
			finally
			{
				loopDepth--;
			}
			
		}
		
		#endregion
		
		#region Expr Methods
		
		private Expr Expression()
		{
			return Assignment();
		}
		
		private Expr Assignment()
		{
			Expr expr = Or();
			
			if(Match(EQUAL))
			{
				Token equals = Previous();
				Expr value = Assignment();
				
				if(expr is Variable)
				{
					Token name = ((Variable)expr).name;
					return new Assign(name, value);
				}
				else if(expr is Get)
				{
					Get get = (Get)expr;
					return new Set(get.obj, get.name, value);
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
			
			return Call();
		}
		
		private Expr Call()
		{
			Expr expr = Primary();
			while(true)
			{
				if(Match(LEFT_PAREN))
				{
					expr = FinishCall(expr);
				}
				else if(Match(DOT))
				{
					Token name = Consume(IDENTIFIER, "Expected property name after '.'");
					expr = new Get(expr, name);
				}
				else
				{
					break;
				}
			}
			return expr;
		}
		
		private Expr FinishCall(Expr callee)
		{
			List<Expr> arguments = new List<Expr>();
			if(!Check(RIGHT_PAREN))
			{
				do
				{
					if(arguments.Count >= 255) Lox.Error(Peek(), "Cannot use more than 255 arguemnts.");
					arguments.Add(Expression());
				} while(Match(COMMA));
			}
			Token paren = Consume(RIGHT_PAREN, "Unmatched '('");
			return new Call(callee, paren, arguments);
		}
		
		private Expr Primary()
		{
			if(Match(FALSE)) return new Literal(false);
			if(Match(TRUE)) return new Literal(true);
			if(Match(NIL)) return new Literal(null);
			if(Match(NUMBER, STRING)) return new Literal(Previous().literal);
			if(Match(BASE))
			{
				Token keyword = Previous();
				Consume(DOT, "Expected '.' after 'base'.");
				Token method = Consume(IDENTIFIER, "Expected base class method name");
				return new Base(keyword, method);
			}
			if(Match(THIS)) return new This(Previous());
			if(Match(IDENTIFIER)) return new Variable(Previous());
			if(Match(LEFT_PAREN))
			{
				Expr expr = Expression();
				Consume(RIGHT_PAREN, "Expect ')' after expression");
				return new Grouping(expr);
			}
			
			throw Exception(Peek(), "Expect expression.");
		}
		
		#endregion
		
		#region Private Members
		
		private Token Advance()
		{
			if(!isAtEnd) current++;
			return Previous();
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
		
		private ParseExecption Exception(Token token, string message)
		{
			Lox.Error(token, message);
			return new ParseExecption();
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
		
		private Token Peek() => tokens[current];
		
		private Token Previous() => tokens[current - 1];
		
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
		
		#endregion
	}
	
	internal class ParseExecption : Exception
	{
		
	}
}