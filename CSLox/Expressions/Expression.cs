namespace CSLox.Expressions
{

	public abstract class Expression
	{
		internal abstract T Accept<T>(ExpressionVisitor<T> visitor);
	}
	internal interface ExpressionVisitor<T>
	{
		T Visit(Binary expression);
		T Visit(Grouping expression);
		T Visit(Literal expression);
		T Visit(Unary expression);
	}

	internal class Binary : Expression
	{
		internal readonly Expression left;
		internal readonly Token _operator;
		internal readonly Expression right;

		internal Binary(Expression left, Token _operator, Expression right)
		{
			this.left = left;
			this._operator = _operator;
			this.right = right;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
		return visitor.Visit(this);
		}
	}

	internal class Grouping : Expression
	{
		internal readonly Expression expression;

		internal Grouping(Expression expression)
		{
			this.expression = expression;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
		return visitor.Visit(this);
		}
	}

	internal class Literal : Expression
	{
		internal readonly object? value;

		internal Literal(object? value)
		{
			this.value = value;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
		return visitor.Visit(this);
		}
	}

	internal class Unary : Expression
	{
		internal readonly Token _operator;
		internal readonly Expression right;

		internal Unary(Token _operator, Expression right)
		{
			this._operator = _operator;
			this.right = right;
		}

		internal override T Accept<T>(ExpressionVisitor<T> visitor)
		{
		return visitor.Visit(this);
		}
	}
}
