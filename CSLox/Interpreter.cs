using CSLox.ControlException;
using CSLox.Grammar;
using static CSLox.TokenType;

namespace CSLox
{
	internal class Interpreter : ExprVisitor<object>, StmtVisitor<object>
	{
		#region Members 
		
		internal readonly Environment globals = new Environment();
		private Environment environment;
		private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();
		
		#endregion
		
		#region Constructors
		
		internal Interpreter()
		{
			environment = globals;
			globals.Define("clock", new Clock());
		}
		
		#endregion
		
		#region Internal Methods
		
		internal void ExecuteBlock(List<Stmt> statements, Environment environment)
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
			var result = Evaluate(expression);
			if(result != null) Console.WriteLine(result.ToString());
		}
		
		internal void Resolve(Expr expr, int depth)
		{
			locals[expr] = depth;
		}
		
		#endregion
		
		#region StmtVisitor<object>
		
		public object Visit(Block stmt)
		{
			ExecuteBlock(stmt.statements, new Environment(environment));
			return null;
		}
		
		public object Visit(Break stmt)
		{
			throw new BreakException();
		}
		
		public object Visit(Class stmt)
		{
			object baseclass = null;
			if(stmt.baseclass != null)
			{
				baseclass = Evaluate(stmt.baseclass);
				if(!(baseclass is LoxClass)) throw new LoxRuntimeException(stmt.baseclass.name, "Base class must be a class.");
			}
			environment.Define(stmt.name.lexeme, null);
			if(stmt.baseclass != null) 
			{
				environment = new Environment(environment);
				environment.Define("base", baseclass);
			}
			Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
			foreach(Function method in stmt.methods)
			{
				LoxFunction function = new LoxFunction(method, environment, method.name.lexeme.Equals("init"));
				methods[method.name.lexeme] = function;
			}
			LoxClass klass = new LoxClass(stmt.name.lexeme, (LoxClass)baseclass, methods);
			if(baseclass != null)
			{
				environment = environment.enclosing;
			}
			environment.Assign(stmt.name, klass);
			return null;
		}
		
		public object Visit(Continue stmt)
		{
			throw new ContinueException();
		}
		
		public object Visit(Expression stmt)
		{
			Evaluate(stmt.expression);
			return null;
		}
		
		public object Visit(Function stmt)
		{
			LoxFunction function = new LoxFunction(stmt, environment, false);
			environment.Define(stmt.name.lexeme, function);
			return null;
		}
		
		public object Visit(If stmt)
		{
			if(IsTruthy(Evaluate(stmt.condition)))
			{
				Execute(stmt.thenBranch);
			}
			else if(stmt.elseBranch != null)
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
		
		public object Visit(Return stmt)
		{
			object value = null;
			if(stmt.value != null) value = Evaluate(stmt.value);
			throw new ReturnException(value);
		}
		
		public object Visit(Var stmt)
		{
			object value = null;
			if(stmt.initializer != null) value = Evaluate(stmt.initializer);
			environment.Define(stmt.name.lexeme, value);
			return null;
		}
		
		public object Visit(While stmt)
		{
			while(IsTruthy(Evaluate(stmt.condition)))
			{
				try
				{
					Execute(stmt.body);
				}
				catch(BreakException)
				{
					break;
				}
				catch(ContinueException)
				{
					environment = new Environment(environment); //create new environment to properly resolve increment variable depth
					if(stmt.increment != null)
					{
						Execute(stmt.increment);
					}
					environment = environment.enclosing;
					continue;
				}
			}
			return null;
		}
		
		#endregion
		
		#region ExprVisitor<object>
		
		public object Visit(Assign expr)
		{
			object value = Evaluate(expr.value);
			if(locals.ContainsKey(expr))
			{
				environment.AssignAt(locals[expr], expr.name, value);
			}
			else
			{
				globals.Assign(expr.name, value);
			}
			return null;
		}
		
		public object Visit(Base expr)
		{
			int distance = locals[expr];
			LoxClass baseclass = (LoxClass)environment.GetAt(distance, "base");
			LoxInstance obj = (LoxInstance)environment.GetAt(distance - 1, "this");
			LoxFunction method = baseclass.FindMethod(expr.method.lexeme);
			if(method == null) throw new LoxRuntimeException(expr.method, $"Undefined property '{expr.method.lexeme}'.");
			return method.Bind(obj);
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
		
		public object Visit(Call expr)
		{
			object callee = Evaluate(expr.callee);
			List<object> arguments = expr.arguments.Select(e => Evaluate(e)).ToList();
			if(!(callee is LoxCallable)) throw new LoxRuntimeException(expr.paren, "Only function and class names can be called.");
			LoxCallable function = (LoxCallable)callee;
			if(arguments.Count != function.Arity()) throw new LoxRuntimeException(expr.paren, $"Expected {function.Arity()} argument but received {arguments.Count}.");
			return function.Call(this, arguments);
		}
		
		public object Visit(Get expr)
		{
			object obj = Evaluate(expr.obj);
			if(obj is LoxInstance) return ((LoxInstance)obj).Get(expr.name);
			throw new LoxRuntimeException(expr.name, "Only instances have properties.");
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
		
		public object Visit(Set expr)
		{
			object obj = Evaluate(expr.obj);
			if(!(obj is LoxInstance))
			{
				throw new LoxRuntimeException(expr.name, "Only instances have fields");
			}
			object value = Evaluate(expr.value);
			((LoxInstance)obj).Set(expr.name, value);
			return value;
		}
		
		public object Visit(This expr)
		{
			return LookUpVariable(expr.keyword, expr);
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
		
		public object Visit(Variable expr) => LookUpVariable(expr.name, expr);
		
		#endregion
		
		#region Private Methods
		
		private void CheckDivideByZero(Token _operator, double value)
		{
			if(_operator.type == SLASH && value == 0) throw new LoxRuntimeException(_operator, "Cannot divide by zero.");
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
		
		private object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}
		
		private void Execute(Stmt stmt)
		{
			stmt.Accept(this);
		}
		
		private bool IsEqual(object a, object b)
		{
			if(a == null && b == null) return true;
			if(a == null) return false;
			return a.Equals(b);
		}
		
		private bool IsTruthy(object obj)
		{
			if(obj == null) return false;
			if(obj is bool) return (bool)obj;
			if(obj is double && (double)obj == 0) return false;
			if(obj is string && (string)obj == "") return false;
			return true;
		}
		
		private object LookUpVariable(Token name, Expr expr)
		{
			if(locals.ContainsKey(expr))
			{
				return environment.GetAt(locals[expr], name.lexeme);
			}
			return globals.Get(name);
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
		
		#endregion				
	}
	
	internal class LoxRuntimeException : Exception
	{
		public Token Token { get; private set; }
		
		public LoxRuntimeException(Token token, string message) : base(message)
		{
			this.Token = token;
		}
	}
	
	#region Native Classes
	
	
	
	#endregion

	#region Native Methods
	
	//C# doesn't permmit anonymous class definitions, so a concrete type must be created
	internal class Clock : LoxCallable
	{
		public int Arity() => 0;

		public object Call(Interpreter interpreter, List<object> arguments) => (DateTime.UtcNow - new DateTime(1970,1,1,0,0,0)).TotalSeconds;
		
		public override string ToString() => "<native function>";
	}
	
	#endregion
}