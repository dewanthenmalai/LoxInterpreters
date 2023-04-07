namespace CSLox.Grammar
{

	internal interface Stmt
	{
		public T Accept<T>(StmtVisitor<T> visitor);
	}

	internal interface StmtVisitor<T>
	{
		T Visit(Block stmt);
		T Visit(Break stmt);
		T Visit(Class stmt);
		T Visit(Continue stmt);
		T Visit(Expression stmt);
		T Visit(Function stmt);
		T Visit(If stmt);
		T Visit(Print stmt);
		T Visit(Return stmt);
		T Visit(Var stmt);
		T Visit(While stmt);
	}

	internal class Block : Stmt
	{
		internal readonly List<Stmt> statements;

		internal Block(List<Stmt> statements)
		{
			this.statements = statements;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Break : Stmt
	{
		internal readonly Token keyword;

		internal Break(Token keyword)
		{
			this.keyword = keyword;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Class : Stmt
	{
		internal readonly Token name;
		internal readonly Variable baseclass;
		internal readonly List<Function> methods;

		internal Class(Token name, Variable baseclass, List<Function> methods)
		{
			this.name = name;
			this.baseclass = baseclass;
			this.methods = methods;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Continue : Stmt
	{
		internal readonly Token keyword;

		internal Continue(Token keyword)
		{
			this.keyword = keyword;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Expression : Stmt
	{
		internal readonly Expr expression;

		internal Expression(Expr expression)
		{
			this.expression = expression;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Function : Stmt
	{
		internal readonly Token name;
		internal readonly List<Token> parameters;
		internal readonly List<Stmt> body;

		internal Function(Token name, List<Token> parameters, List<Stmt> body)
		{
			this.name = name;
			this.parameters = parameters;
			this.body = body;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class If : Stmt
	{
		internal readonly Expr condition;
		internal readonly Stmt thenBranch;
		internal readonly Stmt elseBranch;

		internal If(Expr condition, Stmt thenBranch, Stmt elseBranch)
		{
			this.condition = condition;
			this.thenBranch = thenBranch;
			this.elseBranch = elseBranch;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Print : Stmt
	{
		internal readonly Expr expression;

		internal Print(Expr expression)
		{
			this.expression = expression;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Return : Stmt
	{
		internal readonly Token keyword;
		internal readonly Expr value;

		internal Return(Token keyword, Expr value)
		{
			this.keyword = keyword;
			this.value = value;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class Var : Stmt
	{
		internal readonly Token name;
		internal readonly Expr initializer;

		internal Var(Token name, Expr initializer)
		{
			this.name = name;
			this.initializer = initializer;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}

	internal class While : Stmt
	{
		internal readonly Expr condition;
		internal readonly Stmt body;
		internal readonly Stmt increment;

		internal While(Expr condition, Stmt body, Stmt increment)
		{
			this.condition = condition;
			this.body = body;
			this.increment = increment;
		}

		public T Accept<T>(StmtVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
}
