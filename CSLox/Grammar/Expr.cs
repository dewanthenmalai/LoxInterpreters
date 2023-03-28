namespace CSLox.Grammar
{

	internal interface Expr
	{
		public T Accept<T>(ExprVisitor<T> visitor);
	}

	internal interface ExprVisitor<T>
	{
		T Visit(Assign expr);
		T Visit(Base expr);
		T Visit(Binary expr);
		T Visit(Call expr);
		T Visit(Get expr);
		T Visit(Grouping expr);
		T Visit(Literal expr);
		T Visit(Logical expr);
		T Visit(Set expr);
		T Visit(This expr);
		T Visit(Unary expr);
		T Visit(Variable expr);
	}

	internal class Assign : Expr
	{
		internal readonly Token name;
		internal readonly Expr value;

		internal Assign(Token name, Expr value)
		{
			this.name = name;
			this.value = value;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Base : Expr
	{
		internal readonly Token keyword;
		internal readonly Token method;

		internal Base(Token keyword, Token method)
		{
			this.keyword = keyword;
			this.method = method;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Binary : Expr
	{
		internal readonly Expr left;
		internal readonly Token _operator;
		internal readonly Expr right;

		internal Binary(Expr left, Token _operator, Expr right)
		{
			this.left = left;
			this._operator = _operator;
			this.right = right;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Call : Expr
	{
		internal readonly Expr callee;
		internal readonly Token paren;
		internal readonly List<Expr> arguments;

		internal Call(Expr callee, Token paren, List<Expr> arguments)
		{
			this.callee = callee;
			this.paren = paren;
			this.arguments = arguments;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Get : Expr
	{
		internal readonly Expr obj;
		internal readonly Token name;

		internal Get(Expr obj, Token name)
		{
			this.obj = obj;
			this.name = name;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Grouping : Expr
	{
		internal readonly Expr expression;

		internal Grouping(Expr expression)
		{
			this.expression = expression;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Literal : Expr
	{
		internal readonly object value;

		internal Literal(object value)
		{
			this.value = value;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Logical : Expr
	{
		internal readonly Expr left;
		internal readonly Token _operator;
		internal readonly Expr right;

		internal Logical(Expr left, Token _operator, Expr right)
		{
			this.left = left;
			this._operator = _operator;
			this.right = right;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Set : Expr
	{
		internal readonly Expr obj;
		internal readonly Token name;
		internal readonly Expr value;

		internal Set(Expr obj, Token name, Expr value)
		{
			this.obj = obj;
			this.name = name;
			this.value = value;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class This : Expr
	{
		internal readonly Token keyword;

		internal This(Token keyword)
		{
			this.keyword = keyword;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Unary : Expr
	{
		internal readonly Token _operator;
		internal readonly Expr right;

		internal Unary(Token _operator, Expr right)
		{
			this._operator = _operator;
			this.right = right;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Variable : Expr
	{
		internal readonly Token name;

		internal Variable(Token name)
		{
			this.name = name;
		}

		public T Accept<T>(ExprVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
}
