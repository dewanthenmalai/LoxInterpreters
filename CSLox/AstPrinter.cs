using System.Text;
using CSLox.Grammar;

namespace CSLox
{
	internal class AstPrinter : ExprVisitor<string>
	{
		public string Visit(Binary expression)
		{
			return Parenthesize(expression._operator.lexeme, expression.left, expression.right);
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
			return Parenthesize(expression._operator.lexeme, expression.right);
		}
		
		public string Visit(Variable expr)
		{
			return Parenthesize(expr.name.lexeme);
		}
		
		private string Parenthesize(string name, params Expr[] expressions)
		{
			StringBuilder builder = new StringBuilder();
			
			builder.Append("(").Append(name);
			foreach(Expr expr in expressions)
			{
				builder.Append(" ");
				builder.Append(expr.Accept(this));
			}
			builder.Append(")");
			return builder.ToString();
		}
	}
}