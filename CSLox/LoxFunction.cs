using CSLox.Grammar;

namespace CSLox
{
	internal class LoxFunction : LoxCallable
	{
		private readonly Function declaration;
		private readonly Environment closure;
		
		internal LoxFunction(Function declaration, Environment closure)
		{
			this.closure = closure;
			this.declaration = declaration;
		}

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
				return returnValue.value;
			}
			return null;
		}
		
		public override string ToString() => $"<function {declaration.name.lexeme}>";
	}
}