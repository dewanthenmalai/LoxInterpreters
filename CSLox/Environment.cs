namespace CSLox
{
	class Environment
	{
		private readonly Dictionary<string, object> values = new Dictionary<string, object>();
		
		internal void Define(string name, object value)
		{
			values[name] = value;
		}
		
		internal object Get(Token name)
		{
			if(values.ContainsKey(name.lexeme)) return values[name.lexeme];
			
			throw new LoxRuntimeException(name, $"Undefined variable '{name.lexeme}'.");
		}
		
		internal void Assign(Token name, object value)
		{
			if(values.ContainsKey(name.lexeme))
			{
				values[name.lexeme] = value;
			}
			
			throw new LoxRuntimeException(name, $"Undefined variable '{name.lexeme}'.");
		}
	}
}