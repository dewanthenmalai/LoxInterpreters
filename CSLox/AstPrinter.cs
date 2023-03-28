using System.Text;
using CSLox.Grammar;

namespace CSLox
{
	internal class AstPrinter : ExprVisitor<string>
	{
		public string Visit(Assign expr)
		{
			return Parenthesize(expr.name.lexeme, expr.value);
		}
		
		public string Visit(Binary expr)
		{
			return Parenthesize(expr._operator.lexeme, expr.left, expr.right);
		}
		
		public string Visit(Call expr)
		{
			throw new NotImplementedException();
		}

		public string Visit(Grouping expr)
		{
			return Parenthesize("group", expr.expression);
		}

		public string Visit(Literal expr)
		{
			if(expr.value == null) return "nil";
			return expr.value.ToString()!;
		}
		
		public string Visit(Logical expr)
		{
			return Parenthesize(expr._operator.lexeme, expr.left, expr.right);
		}

		public string Visit(Unary expr)
		{
			return Parenthesize(expr._operator.lexeme, expr.right);
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