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
    internal static class Helper
    {
        internal static string BuildSqlStringFromIdString(SyntaxNodeAnalysisContext context,string id)
        {   //Build SqlString From IdString
            string sql = string.Empty;

            id = id.Replace("{", " + {").Replace("}","} + ");

            string[] list = id.Split('+');

            foreach (string s in list)
            {
                if(s.Contains("{") == false)
                {
                    sql += s.Replace("$\"", string.Empty).Replace("\"",string.Empty);
                }
                else
                {
                    id = s.Replace(" ", "").Replace("{", "").Replace("}", "");

                    BlockSyntax method = context.Node.FirstAncestorOrSelf<BlockSyntax>();
                    if (method == null)
                    {
                        break;
                    }
                    var t = method.DescendantTokens().Where<SyntaxToken>(st => st.ValueText == id).First<SyntaxToken>();
                    if (string.IsNullOrWhiteSpace(t.ValueText))
                    {
                        break;
                    }

                    sql += t.GetNextToken().GetNextToken().Value.ToString();

                }
            }
            return sql; 
        }
    }
    
}
