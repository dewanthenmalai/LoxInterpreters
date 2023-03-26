using CSLox.Expressions;
using CSLox.Tools;
namespace CSLox
{
	public class Lox
	{
		internal static bool hadError = false;
		
		public static void Main(string[] args)
		{
			GenerateAst.DefineAst(@".\Expressions");
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
			Expression expr = parser.Parse();
			//stop on syntax error
			if(hadError) return;
			
			Console.WriteLine(new AstPrinter().Print(expr));
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
		
		private static void Report(int line, string where, string message)
		{
			Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
			hadError = true;
		}
	}
}