using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInjection_Analyzer.Diagnostics
{
    internal class IllegalBinary
    {
        public const string DiagnosticId = "SQLInjection_Analyzer";

        private const string Title = "";
        private const string MessageFormat = "Illegal/Logically Incorrect Query";
        private const string Description = "";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        internal static void Run(SyntaxNodeAnalysisContext context)
        {   //Report warning illegal in binary case
            try
            {
                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            catch { }
        }
    }
}
