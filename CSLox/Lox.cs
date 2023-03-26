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
				Environment.Exit(64);
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
			if(hadError) Environment.Exit(65);
			if(hadRuntimeError) Environment.Exit(70);
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
			
			interpreter.Interpret(statements);
		}
		
		internal static void Error(int line, string message)
		{
			Report(line, "", message);
		}
		
		internal static void Error(Token token, string message)
		{
			if(token.Type == TokenType.EOF)
			{
				Report(token.Line, " at end", message);
			}
			else
			{
				Report(token.Line, $" at '{token.Lexeme}'", message);
			}
		}
		
		internal static void RuntimeError(LoxRuntimeException lrex)
		{
			Console.Error.WriteLine($"{lrex.Message}\n[line {lrex.Token.Line}]");
			hadRuntimeError = true;
		}
		
		private static void Report(int line, string where, string message)
		{
			Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
			hadError = true;
		}
	}
}