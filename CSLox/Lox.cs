using CSLox.Grammar;
using CSLox.Tools;
namespace CSLox
{
	public class Lox
	{
		private static bool hadError = false;
		private static bool hadRuntimeError = false;
		private static readonly Interpreter interpreter = new Interpreter();
		
		public static void Main(string[] args)
		{
			GenerateAst.DefineAst(@".\Grammar"); //hack to generate ASTs, since C# doesn't allow multiple Main methods
			if(args.Length > 1)
			{
				Console.WriteLine("Usage: cslox [script]");
				System.Environment.Exit(64);
			}
			else if(args.Length == 1)
			{
				RunFile(args[0]);
			}
			else
			{
				RunPrompt();
			}
		}
		
		private static void RunFile(string file)
		{
			byte[] bytes = File.ReadAllBytes(Path.GetFullPath(file));
			Run(System.Text.Encoding.Default.GetString(bytes));
			if(hadError) System.Environment.Exit(65);
			if(hadRuntimeError) System.Environment.Exit(70);
		}
		
		private static void RunPrompt()
		{
			for(;;)
			{
				Console.Write("$> ");
				string line = Console.ReadLine();
				if(line == null) break;
				Run(line);
				hadError = false;
			}
		}
		
		private static void Run(string source)
		{
			Scanner scanner = new Scanner(source);
			List<Token> tokens = scanner.ScanTokens();
			Parser parser = new Parser(tokens);
			List<Stmt> statements = parser.Parse();
			//stop on syntax error
			if(hadError) return;
			
			if(statements.Count == 1 && statements[0] is Expression)
			{
				interpreter.Interpret(((Expression)statements[0]).expression);
				return;
			}
			
			interpreter.Interpret(statements);
		}
		
		internal static void Error(int line, string message)
		{
			Report(line, "", message);
		}
		
		internal static void Error(Token token, string message)
		{
			if(token.type == TokenType.EOF)
			{
				Report(token.line, " at end", message);
			}
			else
			{
				Report(token.line, $" at '{token.lexeme}'", message);
			}
		}
		
		internal static void RuntimeError(LoxRuntimeException lrex)
		{
			Console.Error.WriteLine($"{lrex.Message}\n[line {lrex.Token.line}]");
			hadRuntimeError = true;
		}
		
		private static void Report(int line, string where, string message)
		{
			Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
			hadError = true;
		}
	}
}