namespace CSLox
{
	internal class LoxClass : LoxCallable
	{
		internal readonly string name;
		internal readonly LoxClass baseclass;
		private readonly IDictionary<string, LoxFunction> methods;
	
		internal LoxClass(string name, LoxClass baseclass, IDictionary<string, LoxFunction> methods)
		{
			this.name = name;
			this.baseclass = baseclass;
			this.methods = methods;
		}
		
		internal LoxFunction FindMethod(string name)
		{
			if(methods.ContainsKey(name)) return methods[name];
			if(baseclass != null) return baseclass.FindMethod(name);
			return null;
		}
		
		public int Arity()
		{
			LoxFunction initializer = FindMethod("init");
			if(initializer == null) return 0;
			return initializer.Arity();
		}
		
		public object Call(Interpreter interpreter, List<object> arguments)
		{
			LoxInstance instance = new LoxInstance(this);
			return instance;
		}
	
		public override string ToString() => name;
	}
}