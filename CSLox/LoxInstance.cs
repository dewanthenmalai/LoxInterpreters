namespace CSLox
{
	internal class LoxInstance
	{
		private LoxClass klass;
		private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
		
		internal LoxInstance(LoxClass klass)
		{
			this.klass = klass;
		}
		
		internal object Get(Token name)
		{
			if(fields.ContainsKey(name.lexeme)) return fields[name.lexeme];
			
			LoxFunction method = klass.FindMethod(name.lexeme);
			if(method != null) return method.Bind(this);
			
			throw new LoxRuntimeException(name, $"Undefined property '{name.lexeme}'.");
		}
		
		internal void Set(Token name, object value)
		{
			fields[name.lexeme] = value;
		}
		
		public override string ToString() => $"{klass.name} instance";
	}
}