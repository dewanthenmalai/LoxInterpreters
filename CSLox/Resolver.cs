using CSLox.Grammar;

namespace CSLox
{
	internal class Resolver : ExprVisitor<object>, StmtVisitor<object> //C# doesn't permit T to be void, so we use object and just return null
	{
		#region Members
		
		private readonly Interpreter interpreter;
		private readonly Stack<IDictionary<string, bool>> scopes = new Stack<IDictionary<string, bool>>();
		private FunctionType currentFunction = FunctionType.NONE;
		private ClassType currentClass = ClassType.NONE;
		private bool inLoop = false;
		private enum FunctionType
		{
			NONE,
			FUNCTION,
			INITIALIZER,
			METHOD
		}
		private enum ClassType
		{
			NONE,
			CLASS,
			SUBCLASS
		}
		
		#endregion
		
		#region Constructors
		
		internal Resolver(Interpreter interpreter)
		{
			this.interpreter = interpreter;
		}
		
		#endregion
		
		#region Internal Members
		
		internal void Resolve(List<Stmt> statments)
		{
			foreach(var statement in statments)
			{
				Resolve(statement);
			}
		}
		
		#endregion
		
		#region StmtVisitor<object>

		public object Visit(Block stmt)
		{
			BeginScope();
			Resolve(stmt.statments);
			EndScope();
			return null;
		}
		
		public object Visit(Break stmt)
		{
			if(!inLoop) Lox.Error(stmt.keyword, "Cannot break outside a loop.");
			return null;
		}
		
		public object Visit(Class stmt)
		{
			ClassType enclosingClass = currentClass;
			currentClass = ClassType.CLASS;
			Declare(stmt.name);
			Define(stmt.name);
			if(stmt.baseclass != null && stmt.name.lexeme.Equals(stmt.baseclass.name.lexeme)) Lox.Error(stmt.baseclass.name, "A class cannot inherit from itself.");
			if(stmt.baseclass != null)
			{
				currentClass = ClassType.SUBCLASS;
				Resolve(stmt.baseclass);
				BeginScope();
				scopes.Peek()["base"] = true;
			}
			BeginScope();
			scopes.Peek()["this"] = true;
			foreach(Function method in stmt.methods)
			{
				FunctionType declaration = FunctionType.METHOD;
				if(method.name.lexeme.Equals("init")) declaration = FunctionType.INITIALIZER;
				ResolveFunction(method, declaration);
			}
			EndScope();
			if(stmt.baseclass != null) EndScope();
			currentClass = enclosingClass;
			return null;
		}
		
		public object Visit(Continue stmt)
		{
			if(!inLoop) Lox.Error(stmt.keyword, "Cannot continue outside a loop.");
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
			if(stmt.value != null)
			{
				if(currentFunction == FunctionType.INITIALIZER) Lox.Error(stmt.keyword, "Cannot return a value from a constructor.");
				Resolve(stmt.value);
			}
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
			inLoop = true;
			Resolve(stmt.body);
			inLoop = false;
			return null;
		}
		
		#endregion
		
		#region ExprVisitor<object>

		public object Visit(Assign expr)
		{
			Resolve(expr.value);
			ResolveLocal(expr, expr.name);
			return null;
		}
		
		public object Visit(Base expr)
		{
			if(currentClass == ClassType.NONE) Lox.Error(expr.keyword, "Cannot use 'base' outside a class.");
			else if(currentClass != ClassType.SUBCLASS) Lox.Error(expr.keyword, "Cannot use 'base' in a non-inheriting class.");
			ResolveLocal(expr, expr.keyword);
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
		
		public object Visit(Get expr)
		{
			Resolve(expr.obj);
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
		
		public object Visit(Set expr)
		{
			Resolve(expr.value);
			Resolve(expr.obj);
			return null;
		}
		
		public object Visit(This expr)
		{
			ResolveLocal(expr, expr.keyword);
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
			if(scopes.Count != 0) 
			{
				bool defined;
				bool exists = scopes.Peek().TryGetValue(expr.name.lexeme, out defined);
				if(exists && !defined) Lox.Error(expr.name, "Can't read local variable in its own initializer.");
			}
			ResolveLocal(expr, expr.name);
			return null;
		}
		
		#endregion
		
		#region Private Methods
		
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
		
		#endregion
	}
}