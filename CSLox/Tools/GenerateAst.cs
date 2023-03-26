namespace CSLox.Tools
{
	//Generates abstract syntax trees used by interpreter
	public class GenerateAst
	{
		public static void DefineAst(string outputDir)
		{
			DefineAst(outputDir,
					  "Expression",
					  new List<string>
					  {
					  	"Binary   : Expression left, Token _operator, Expression right",
						"Grouping : Expression expression",
						"Literal  : object? value",
						"Unary    : Token _operator, Expression right"
					  });
		}
		
		private static void DefineAst(string outputDir, string baseName, List<string> types)
		{
			string path = @$"{outputDir}/{baseName}.cs";
			using(StreamWriter sw = new StreamWriter(path))
			{
				sw.WriteLine("namespace CSLox.Expressions");
				sw.WriteLine("{");
				sw.WriteLine();
				sw.WriteLine($"\tpublic abstract class {baseName}");
				sw.WriteLine("\t{");
				sw.WriteLine($"\t\tinternal abstract T Accept<T>(ExpressionVisitor<T> visitor);");
				sw.WriteLine("\t}");
				
				DefineVisitor(sw, baseName, types);
				
				foreach(string type in types)
				{
					string[] splitStr = type.Split(':');
					string className = splitStr[0].Trim();
					string fields = splitStr[1].Trim();
					sw.WriteLine();
					DefineType(sw, baseName, className, fields);
				}
				sw.WriteLine("}");
			}
		}
		
		private static void DefineType(StreamWriter sw, string baseName, string className, string fields)
		{
			string[] fieldList = fields.Split(", ");
			sw.WriteLine($"\tinternal class {className} : {baseName}");
			sw.WriteLine("\t{");
			foreach(string field in fieldList)
			{
				sw.WriteLine($"\t\tinternal readonly {field};");
			}
			sw.WriteLine();
			sw.WriteLine($"\t\tinternal {className}({fields})");
			sw.WriteLine("\t\t{");
			foreach(string field in fieldList)
			{
				string name = field.Split(' ')[1];
				sw.WriteLine($"\t\t\tthis.{name} = {name};");
			}
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			sw.WriteLine($"\t\tinternal override T Accept<T>(ExpressionVisitor<T> visitor)");
			sw.WriteLine("\t\t{");
			sw.WriteLine($"\t\treturn visitor.Visit(this);");
			sw.WriteLine("\t\t}");
			sw.WriteLine("\t}");
		}
		
		private static void DefineVisitor(StreamWriter sw, string baseName, List<string> types)
		{
			sw.WriteLine($"\tinternal interface ExpressionVisitor<T>");
			sw.WriteLine("\t{");
			foreach(string type in types)
			{
				string typeName = type.Split(':')[0].Trim();
				sw.WriteLine($"\t\tT Visit({typeName} {baseName.ToLower()});");
			}
			sw.WriteLine("\t}");
		}
	}
}