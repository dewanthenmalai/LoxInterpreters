namespace CSLox.Tools
{
	//Generates abstract syntax trees used by interpreter
	public class GenerateAst
	{
		public static void DefineAst(string outputDir)
		{
			DefineAst(outputDir,
					  "Expr",
					  new List<string>
					  {
						"Assign   : Token name, Expr value",
					  	"Binary   : Expr left, Token _operator, Expr right",
						"Grouping : Expr expression",
						"Literal  : object value",
						"Unary    : Token _operator, Expr right",
						"Variable : Token name"
					  });
			DefineAst(outputDir,
					  "Stmt",
					  new List<string>
					  {
						"Block      : List<Stmt> statments",
					  	"Expression : Expr expression",
						"Print      : Expr expression",
						"Var        : Token name, Expr initializer"
					  });
		}
		
		private static void DefineAst(string outputDir, string baseName, List<string> types)
		{
			string path = @$"{outputDir}/{baseName}.cs";
			using(StreamWriter sw = new StreamWriter(path))
			{
				sw.WriteLine("namespace CSLox.Grammar");
				sw.WriteLine("{");
				sw.WriteLine();
				sw.WriteLine($"\tinternal interface {baseName}");
				sw.WriteLine("\t{");
				sw.WriteLine($"\t\tpublic T Accept<T>({baseName}Visitor<T> visitor);");
				sw.WriteLine("\t}");
				sw.WriteLine();
				
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
			sw.WriteLine($"\t\tpublic T Accept<T>({baseName}Visitor<T> visitor)");
			sw.WriteLine("\t\t{");
			sw.WriteLine($"\t\t\treturn visitor.Visit(this);");
			sw.WriteLine("\t\t}");
			sw.WriteLine("\t}");
		}
		
		private static void DefineVisitor(StreamWriter sw, string baseName, List<string> types)
		{
			sw.WriteLine($"\tinternal interface {baseName}Visitor<T>");
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