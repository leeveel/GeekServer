using Microsoft.CodeAnalysis;

namespace Geek.Server.CodeGenerator.Utils
{
    public static class Logger
    {
        public static void LogError(this GeneratorExecutionContext context, string msg)
        {
            DiagnosticDescriptor InvalidXmlWarning = new DiagnosticDescriptor(id: "Error",
                                                                                               title: "Code Generator Error",
                                                                                               messageFormat: "{0}",
                                                                                               category: "CodeGenerator",
                                                                                               DiagnosticSeverity.Error,
                                                                                               isEnabledByDefault: true);
            context.ReportDiagnostic(Diagnostic.Create(InvalidXmlWarning, Location.None, msg));
        }
    }
}
