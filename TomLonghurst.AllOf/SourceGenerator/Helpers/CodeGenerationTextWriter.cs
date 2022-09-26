using System.CodeDom.Compiler;

namespace TomLonghurst.AllOf.SourceGenerator.Helpers;

public class CodeGenerationTextWriter : IndentedTextWriter
{
    public CodeGenerationTextWriter() : base(new StringWriter())
    {
    }

    public override void Write(char value)
    {
        if (value is '{')
        {
            Indent++;
        }
        
        if (value is '}')
        {
            Indent--;
        }
    }

    public override void WriteLine(string s)
    {
        var trimmed = s.Trim();
        
        if (trimmed.StartsWith("}"))
        {
            Indent--;
        }
        
        base.WriteLine(s);
        
        if (trimmed.StartsWith("{"))
        {
            Indent++;
        }
    }

    public override string ToString()
    {
        Flush();
        return InnerWriter.ToString();
    }
}