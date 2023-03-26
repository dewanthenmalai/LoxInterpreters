using CSLox.Expressions;
using static CSLox.TokenType;

namespace CSLox
{
	internal class Interpreter : ExpressionVisitor<object>
	{
		public void Interpret(Expression expr)
		{
			try
			{
				object value = Evaluate(expr);
				Console.WriteLine(Stringify(value));
			}
			catch(LoxRuntimeException lrex)
			{
				Lox.RuntimeError(lrex);
			}
		}
		public object Visit(Binary expression)
		{
			object left = Evaluate(expression.left);
			object right = Evaluate(expression.right);
			
			switch(expression._operator.Type)
			{
				case BANG_EQUAL: return !IsEqual(left, right);
				case EQUAL_EQUAL: return IsEqual(left, right);
				case GREATER:
					CheckNumberOperands(expression._operator, left, right);
					return (double)left > (double)right;
				case GREATER_EQUAL:
					CheckNumberOperands(expression._operator, left, right);
					return (double)left >= (double)right;
				case LESS:
					CheckNumberOperands(expression._operator, left, right);
					return (double)left < (double)right;
				case LESS_EQUAL: 
					CheckNumberOperands(expression._operator, left, right);
					return (double)left <= (double)right;
				case MINUS:
					CheckNumberOperands(expression._operator, left, right);
					return (double)left - (double)right;
				case SLASH:
					CheckNumberOperands(expression._operator, left, right);
					return (double)left / (double)right;
				case STAR:
					CheckNumberOperands(expression._operator, left, right);return (double)left * (double)right;
				case PLUS:
					if(left is double && right is double) return (double)left + (double)right;
					if(left is string && right is string) return (string)left + (string)right;
					throw new LoxRuntimeException(expression._operator, "Operands must be two numbers or two strings.");
			}
			return null;
		}

		public object Visit(Grouping expression) => Evaluate(expression.expression);

		public object Visit(Literal expression) => expression.value;

		public object Visit(Unary expression)
		{
			object right = Evaluate(expression.right);
			
			switch(expression._operator.Type)
			{
				case BANG:
					return !IsTruthy(right);
				case MINUS:
					CheckNumberOperand(expression._operator, right);
					return -(double)right;
			}
			
			return null;
		}
		
		private void CheckNumberOperand(Token _operator, object operand)
		{
			if(operand is double) return;
			throw new LoxRuntimeException(_operator, "Operand must be a number.");
		}
		private void CheckNumberOperands(Token _operator, object left, object right)
		{
			if(left is double && right is double)
			{
				CheckDivideByZero(_operator, (double)right);
				return;
			}
			throw new LoxRuntimeException(_operator, "Operands must be numbers.");
		}
		
		private void CheckDivideByZero(Token _operator, double value)
		{
			if(_operator.Type == SLASH && value == 0) throw new LoxRuntimeException(_operator, "Cannot divide by zero.");
		}
		
		private bool IsTruthy(object obj)
		{
			if(obj == null) return false;
			if(obj is bool) return (bool)obj;
			if(obj is double && (double)obj == 0) return false;
			if(obj is string && (string)obj == "") return false;
			return true;
		}
		
		private bool IsEqual(object a, object b)
		{
			if(a == null && b == null) return true;
			if(a == null) return false;
			return a.Equals(b);
		}
		
		private string Stringify(object obj)
		{
			if(obj == null) return "nil";
			if(obj is double)
			{
				string text = obj.ToString();
				if(text.EndsWith(".0")) text = text.Substring(0, text.Length - 2);
				return text;
			}
			return obj.ToString();
		}
		
		private object Evaluate(Expression expr)
		{
			return expr.Accept(this);
		}
	}
	
	internal class LoxRuntimeException : Exception
	{
		public Token Token { get; private set; }
		
		public LoxRuntimeException(Token token, string message) : base(message)
		{
			this.Token = token;
		}
	}
}