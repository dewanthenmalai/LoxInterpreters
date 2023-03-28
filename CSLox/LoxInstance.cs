namespace CSLox
{
	internal class LoxInstance
	{
		private LoxClass cls;
		private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
		
		internal LoxInstance(LoxClass cls)
		{
			this.cls = cls;
		}
		
		internal object Get(Token name)
		{
			if(fields.ContainsKey(name.lexeme)) return fields[name.lexeme];
			
			LoxFunction method = cls.FindMethod(name.lexeme);
			if(method != null) return method.Bind(this);
			
			throw new LoxRuntimeException(name, $"Undefined property '{name.lexeme}'.");
		}
		
		internal void Set(Token name, object value)
		{
			fields[name.lexeme] = value;
		}
		
		public override string ToString() => $"{cls.name} instance";
	}
}