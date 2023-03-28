using CSLox.Grammar;

namespace CSLox
{
	internal class Resolver : ExprVisitor<object>, StmtVisitor<object>
	{
		private readonly Interpreter interpreter;
		private readonly Stack<IDictionary<string, bool>> scopes = new Stack<IDictionary<string, bool>>();
		private FunctionType currentFunction = FunctionType.NONE;
		
		internal Resolver(Interpreter interpreter)
		{
			this.interpreter = interpreter;
		}
		
		private enum FunctionType
		{
			NONE,
			FUNCTION
		}
		
		internal void Resolve(List<Stmt> statments)
		{
			foreach(var statement in statments)
			{
				Resolve(statement);
			}
		}

		public object Visit(Block stmt)
		{
			BeginScope();
			Resolve(stmt.statments);
			EndScope();
			return null;
		}

		public object Visit(Expression stmt)
		{
			Resolve(stmt.expression);
			return null;
		}

		public object Visit(Function stmt)
		{
			Declare(stmt.name);
			Define(stmt.name);
			ResolveFunction(stmt, FunctionType.FUNCTION);
			return null;
		}

		public object Visit(If stmt)
		{
			Resolve(stmt.condition);
			Resolve(stmt.thenBranch);
			if(stmt.elseBranch != null) Resolve(stmt.elseBranch);
			return null;
		}

		public object Visit(Print stmt)
		{
			Resolve(stmt.expression);
			return null;
		}

		public object Visit(Return stmt)
		{
			if(currentFunction == FunctionType.NONE) Lox.Error(stmt.keyword, "Cannot return from top level code.");
			if(stmt.value != null) Resolve(stmt.value);
			return null;
		}

		public object Visit(Var stmt)
		{
			Declare(stmt.name);
			if(stmt.initializer != null)
			{
				Resolve(stmt.initializer);
			}
			Define(stmt.name);
			return null;
		}

		public object Visit(While stmt)
		{
			Resolve(stmt.condition);
			Resolve(stmt.body);
			return null;
		}

		public object Visit(Assign expr)
		{
			Resolve(expr.value);
			ResolveLocal(expr, expr.name);
			return null;
		}

		public object Visit(Binary expr)
		{
			Resolve(expr.left);
			Resolve(expr.right);
			return null;
		}

		public object Visit(Call expr)
		{
			Resolve(expr.callee);
			foreach(Expr argument in expr.arguments)
			{
				Resolve(argument);
			}
			return null;
		}

		public object Visit(Grouping expr)
		{
			Resolve(expr.expression);
			return null;
		}

		public object Visit(Literal expr)
		{
			return null;
		}

		public object Visit(Logical expr)
		{
			Resolve(expr.left);
			Resolve(expr.right);
			return null;
		}

		public object Visit(Unary expr)
		{
			Resolve(expr.right);
			return null;
		}

		public object Visit(Variable expr)
		{
			if(scopes.Count != 0 && !scopes.Peek()[expr.name.lexeme]) Lox.Error(expr.name, "Can't read local variable in its own initializer.");
			ResolveLocal(expr, expr.name);
			return null;
		}
		
		private void Resolve(Stmt stmt)
		{
			stmt.Accept(this);
		}
		
		private void Resolve(Expr expr)
		{
			expr.Accept(this);
		}
		
		private void BeginScope()
		{
			scopes.Push(new Dictionary<string, bool>());
		}
		
		private void EndScope()
		{
			scopes.Pop();
		}
		
		private void Declare(Token name)
		{
			if(scopes.Count == 0) return;
			IDictionary<string, bool> scope = scopes.Peek();
			if(scope.ContainsKey(name.lexeme)) Lox.Error(name, $"There is already a variable named {name.lexeme} in this scope");
			scope[name.lexeme] = false;
		}
		
		private void Define(Token name)
		{
			if(scopes.Count == 0) return;
			scopes.Peek()[name.lexeme] = true;
		}
		
		private void ResolveLocal(Expr expr, Token name)
		{
			for(int i = scopes.Count - 1; i >= 0 ; i--)
			{
				if(scopes.ElementAt(i).ContainsKey(name.lexeme))
				{
					interpreter.Resolve(expr, scopes.Count - 1 - i);
					return;
				}
			}
		}
		
		private void ResolveFunction(Function function, FunctionType type)
		{
			FunctionType enclosingFunction = currentFunction;
			currentFunction = type;
			BeginScope();
			foreach(Token parameter in function.parameters)
			{
				Declare(parameter);
				Define(parameter);
			}
			Resolve(function.body);
			EndScope();
			currentFunction = enclosingFunction;
		}
	}
}