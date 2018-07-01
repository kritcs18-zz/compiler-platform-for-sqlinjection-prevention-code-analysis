using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInjection_Analyzer.Diagnostics
{
    internal class IllegalExpression
    {
        public const string DiagnosticId = "SQLInjection_Analyzer";

        private const string Title = "";
        private const string MessageFormat = "Illegal/Logically Incorrect Query";
        private const string Description = "";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        internal static void Run(SyntaxNodeAnalysisContext context, SyntaxToken syntaxToken)
        {   //Report warning illgal in expression case
            try
            {
                var diagnostic = Diagnostic.Create(Rule, syntaxToken.GetNextToken().GetNextToken().GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            catch { }
        }
    }
}
