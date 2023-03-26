using System.Text;
using CSLox.Expressions;

namespace CSLox
{
	internal class AstPrinter : ExpressionVisitor<string>
	{
		internal string Print(Expression expr)
		{
			return expr?.Accept(this);
		}
		public string Visit(Binary expression)
		{
			return Parenthesize(expression._operator.Lexeme, expression.left, expression.right);
		}

		public string Visit(Grouping expression)
		{
			return Parenthesize("group", expression.expression);
		}

		public string Visit(Literal expression)
		{
			if(expression.value == null) return "nil";
			return expression.value.ToString()!;
		}

		public string Visit(Unary expression)
		{
			return Parenthesize(expression._operator.Lexeme, expression.right);
		}
		
		private string Parenthesize(string name, params Expression[] expressions)
		{
			StringBuilder builder = new StringBuilder();
			
			builder.Append("(").Append(name);
			foreach(Expression expr in expressions)
			{
				builder.Append(" ");
				builder.Append(expr.Accept(this));
			}
			builder.Append(")");
			return builder.ToString();
		}
	}
}