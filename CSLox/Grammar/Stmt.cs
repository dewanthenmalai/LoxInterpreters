namespace CSLox.Grammar
{

	public abstract class Stmt
	{
		internal abstract T Accept<T>(StmtVisitor<T> visitor);
	}

	internal interface StmtVisitor<T>
	{
		T Visit(Expression stmt);
		T Visit(Print stmt);
	}

	internal class Expression : Stmt
	{
		internal readonly Expr expression;

		internal Expression(Expr expression)
		{
			this.expression = expression;
		}

		internal override T Accept<T>(StmtVisitor<T> visitor)
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

		internal override T Accept<T>(StmtVisitor<T> visitor)
		{
		return visitor.Visit(this);
		}
	}
}
