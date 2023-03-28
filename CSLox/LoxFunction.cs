using CSLox.Grammar;

namespace CSLox
{
	internal class LoxFunction : LoxCallable
	{
		private readonly Function declaration;
		private readonly Environment closure;
		private readonly bool isInitializer;
		
		internal LoxFunction(Function declaration, Environment closure, bool isInitializer)
		{
			this.closure = closure;
			this.declaration = declaration;
			this.isInitializer = isInitializer;
		}
		
		internal LoxFunction Bind(LoxInstance instance)
		{
			Environment environment = new Environment(closure);
			environment.Define("this", instance);
			return new LoxFunction(declaration, environment, isInitializer);
		}

		#region LoxCallable
		
		public int Arity() => declaration.parameters.Count;

		public object Call(Interpreter interpreter, List<object> arguments)
		{
			Environment environment = new Environment(closure);
			for(int i = 0; i < declaration.parameters.Count; i++)
			{
				environment.Define(declaration.parameters[i].lexeme, arguments[i]);
			}
			try
			{
				interpreter.ExecuteBlock(declaration.body, environment);
			}
			catch(ReturnException returnValue)
			{
				if(isInitializer) return closure.GetAt(0, "this");
				return returnValue.value;
			}
			if(isInitializer) return closure.GetAt(0, "this");
			return null;
		}
		
		#endregion
		
		public override string ToString() => $"<function {declaration.name.lexeme}>";
	}
}