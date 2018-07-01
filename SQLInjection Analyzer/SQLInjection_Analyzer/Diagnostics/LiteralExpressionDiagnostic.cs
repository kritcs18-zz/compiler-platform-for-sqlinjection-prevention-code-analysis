using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInjection_Analyzer.Diagnostics
{
    internal class LiteralExpressionDiagnostic
    {
        public const string DiagnosticId = "SQLInjection_Analyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,Title,MessageFormat,Category,DiagnosticSeverity.Error,isEnabledByDefault : true);
        internal static void Run(SyntaxNodeAnalysisContext context,LiteralExpressionSyntax literalExpression)
        {   
            if (literalExpression == null)
            {
                return;
            }

            if (literalExpression.IsKind(SyntaxKind.StringLiteralExpression) && literalExpression.Token.IsKind(SyntaxKind.StringLiteralToken))
            {   //Check literalstring
                var sql = literalExpression.Token.ValueText;
                if (string.IsNullOrWhiteSpace(sql))
                {
                    return;
                }
                //Parse error
                List<string> errors = SqlParser.Parse(sql);
                if (errors.Count == 0)
                {
                    return;
                }
                
                //Report error
                string errorText = String.Join("\r\n",errors);
                var diagnostic = Diagnostic.Create(Rule,literalExpression.GetLocation(),errorText);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
