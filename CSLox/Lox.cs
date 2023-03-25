namespace CSLox
{
    public class Lox
	{
		public static bool hadError = false;
		
		public static void Main(string[] args)
		{
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
				string? line = Console.ReadLine();
				if(line == null) break;
				Run(line);
				hadError = false;
			}
		}
		
		private static void Run(string source)
		{
			List<string> tokens = source.Split(' ').ToList();
			foreach(var token in tokens)
			{
				Console.WriteLine(token);
			}
		}
		
		private static void Error(int line, string message)
		{
			Report(line, "", message);
		}
		
		private static void Report(int line, string where, string message)
		{
			Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
			hadError = true;
		}
	}
}