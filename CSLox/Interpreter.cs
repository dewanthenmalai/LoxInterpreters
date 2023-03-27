using CSLox.Grammar;
using static CSLox.TokenType;

namespace CSLox
{
	internal class Interpreter : ExprVisitor<object>, StmtVisitor<object>
	{
		private Environment environment = new Environment();
		internal void Interpret(List<Stmt> statements)
		{
			try
			{
				foreach(Stmt statement in statements)
				{
					Execute(statement);
				}
			}
			catch(LoxRuntimeException lrex)
			{
				Lox.RuntimeError(lrex);
			}
		}
		
		internal void Interpret(Expr expression)
		{
			Console.WriteLine(Evaluate(expression).ToString());
		}
		
		public object Visit(Assign expr)
		{
			object value = Evaluate(expr.value);
			environment.Assign(expr.name, value);
			return value;
		}
		
		public object Visit(Binary expr)
		{
			object left = Evaluate(expr.left);
			object right = Evaluate(expr.right);
			
			switch(expr._operator.type)
			{
				case BANG_EQUAL: return !IsEqual(left, right);
				case EQUAL_EQUAL: return IsEqual(left, right);
				case GREATER:
					CheckNumberOperands(expr._operator, left, right);
					return (double)left > (double)right;
				case GREATER_EQUAL:
					CheckNumberOperands(expr._operator, left, right);
					return (double)left >= (double)right;
				case LESS:
					CheckNumberOperands(expr._operator, left, right);
					return (double)left < (double)right;
				case LESS_EQUAL: 
					CheckNumberOperands(expr._operator, left, right);
					return (double)left <= (double)right;
				case MINUS:
					CheckNumberOperands(expr._operator, left, right);
					return (double)left - (double)right;
				case SLASH:
					CheckNumberOperands(expr._operator, left, right);
					return (double)left / (double)right;
				case STAR:
					CheckNumberOperands(expr._operator, left, right);return (double)left * (double)right;
				case PLUS:
					if(left is double && right is double) return (double)left + (double)right;
					if(left is string && right is string) return (string)left + (string)right;
					throw new LoxRuntimeException(expr._operator, "Operands must be two numbers or two strings.");
			}
			return null;
		}

		public object Visit(Grouping expr) => Evaluate(expr.expression);

		public object Visit(Literal expr) => expr.value;
		
		public object Visit(Logical expr)
		{
			object left = Evaluate(expr.left);
			if(expr._operator.type == TokenType.OR)
			{
				if(IsTruthy(left)) return left;
			}
			else
			{
				if(!IsTruthy(left)) return left;
			}
			return Evaluate(expr.right);
		}

		public object Visit(Unary expr)
		{
			object right = Evaluate(expr.right);
			
			switch(expr._operator.type)
			{
				case BANG:
					return !IsTruthy(right);
				case MINUS:
					CheckNumberOperand(expr._operator, right);
					return -(double)right;
			}
			
			return null;
		}
		
		public object Visit(Block stmt)
		{
			ExecuteBlock(stmt.statments, new Environment(environment));
			return null;
		}
		
		public object Visit(Variable stmt) => environment.Get(stmt.name);
		
		public object Visit(Expression stmt)
		{
			Evaluate(stmt.expression);
			return null;
		}
		
		public object Visit(If stmt)
		{
			if(IsTruthy(Evaluate(stmt.condition)))
			{
				Execute(stmt.thenBranch);
			}
			else
			{
				Execute(stmt.elseBranch);
			}
			return null;
		}
		
		public object Visit(Print stmt)
		{
			object value = Evaluate(stmt.expression);
			Console.WriteLine(Stringify(value));
			return null;
		}
		
		public object Visit(Var stmt)
		{
			object value = null;
			if(stmt.initializer != null) value = Evaluate(stmt.initializer);
			environment.Define(stmt.name.lexeme, value);
			return null;
		}
		
		private void CheckNumberOperand(Token _operator, object operand)
		{
			if(operand is double) return;
			throw new LoxRuntimeException(_operator, "Operand must be a number.");
		}
		private void CheckNumberOperands(Token _operator, object left, object right)
		{
			if(left is double && right is double)
			{
				CheckDivideByZero(_operator, (double)right);
				return;
			}
			throw new LoxRuntimeException(_operator, "Operands must be numbers.");
		}
		
		private void CheckDivideByZero(Token _operator, double value)
		{
			if(_operator.type == SLASH && value == 0) throw new LoxRuntimeException(_operator, "Cannot divide by zero.");
		}
		
		private bool IsTruthy(object obj)
		{
			if(obj == null) return false;
			if(obj is bool) return (bool)obj;
			if(obj is double && (double)obj == 0) return false;
			if(obj is string && (string)obj == "") return false;
			return true;
		}
		
		private bool IsEqual(object a, object b)
		{
			if(a == null && b == null) return true;
			if(a == null) return false;
			return a.Equals(b);
		}
		
		private string Stringify(object obj)
		{
			if(obj == null) return "nil";
			if(obj is double)
			{
				string text = obj.ToString();
				if(text.EndsWith(".0")) text = text.Substring(0, text.Length - 2);
				return text;
			}
			return obj.ToString();
		}
		
		private object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}
		
		private void Execute(Stmt stmt)
		{
			stmt.Accept(this);
		}

		private void ExecuteBlock(List<Stmt> statements, Environment environment)
		{
			Environment previous = this.environment;
			try
			{
				this.environment = environment;
				foreach(Stmt statement in statements)
				{
					Execute(statement);
				}
			}
			finally
			{
				this.environment = previous;
			}
		}
	}
	
	internal class LoxRuntimeException : Exception
	{
		public Token Token { get; private set; }
		
		public LoxRuntimeException(Token token, string message) : base(message)
		{
			this.Token = token;
		}
	}
}