namespace CSLox.Grammar
{

	public abstract class Expr
	{
		internal abstract T Accept<T>(ExprVisitor<T> visitor);
	}

	internal interface ExprVisitor<T>
	{
		T Visit(Binary expr);
		T Visit(Grouping expr);
		T Visit(Literal expr);
		T Visit(Unary expr);
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

		internal override T Accept<T>(ExprVisitor<T> visitor)
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

		internal override T Accept<T>(ExprVisitor<T> visitor)
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

		internal override T Accept<T>(ExprVisitor<T> visitor)
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

		internal override T Accept<T>(ExprVisitor<T> visitor)
		{
		return visitor.Visit(this);
		}
	}
}
