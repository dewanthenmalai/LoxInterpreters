namespace CSLox
{
	class Environment
	{
		internal readonly Environment enclosing;
		private readonly Dictionary<string, object> values = new Dictionary<string, object>();
		
		internal Environment()
		{
			enclosing = null;
		}
		
		internal Environment(Environment enclosing)
		{
			this.enclosing = enclosing;
		}
		
		internal void Define(string name, object value)
		{
			values[name] = value;
		}
		
		internal Environment Ancestor(int distance)
		{
			Environment environment = this;
			for(int i = 0; i < distance; i++)
			{
				environment = environment.enclosing;
			}
			return environment;
		}
		
		internal object GetAt(int distance, string name) => Ancestor(distance).values[name];
		
		internal void AssignAt(int distance, Token name, object value) => Ancestor(distance).values[name.lexeme] = value;
		
		internal object Get(Token name)
		{
			if(values.ContainsKey(name.lexeme)) return values[name.lexeme];
			
			if(enclosing != null) return enclosing.Get(name);
			
			throw new LoxRuntimeException(name, $"Undefined variable '{name.lexeme}'.");
		}
		
		internal void Assign(Token name, object value)
		{
			if(values.ContainsKey(name.lexeme))
			{
				values[name.lexeme] = value;
				return;
			}
			
			if(enclosing != null)
			{
				enclosing.Assign(name, value);
				return;
			}
			
			throw new LoxRuntimeException(name, $"Undefined variable '{name.lexeme}'.");
		}
	}
}