using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using SQLInjection_Analyzer.Model;
using System.Collections;
using Microsoft.CodeAnalysis.Editing;
namespace SQLInjection_Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SQLInjection_AnalyzerCodeFixProvider)), Shared]
    public class CodeFixProviderCommandText : CodeFixProvider
    {
        private const string title = "Make SqlParameter From Variable At CommandText";
        private Document newDoc;
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(SQLInjection_AnalyzerAnalyzer.DiagnosticId);
            }
        }
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            try
            {
                var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
                var diagnostic = context.Diagnostics.First();
                var diagnosticSpan = diagnostic.Location.SourceSpan;
                // Find the type declaration identified by the diagnostic.
                var assignmentExpressionSyntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().First();

                if (!assignmentExpressionSyntax.Left.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    return;
                }

                if (!assignmentExpressionSyntax.Left.ToString().Contains("CommandText"))
                {
                    return;
                }
                ExpressionSyntax assignmentExpression = assignmentExpressionSyntax.Right;
                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        c => MakeParameterFromVarAsyn(context.Document, assignmentExpressionSyntax, c),
                        equivalenceKey: title),
                        diagnostic);
            } catch { }
        }

        private async Task<Document> MakeParameterFromVarAsyn(Document document, ExpressionSyntax assignmentExpressionSyntax, CancellationToken c)
        {
            
                var binExpression = ((Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax)assignmentExpressionSyntax).Right as BinaryExpressionSyntax;
                var NewSqlCommand = "";
                var TempNode = binExpression.DescendantNodes();
                var TempList = new List<SyntaxNode>();
                var TempSyntax = new List<StatementSyntax>();
                int index = 0;
                bool HaveVar = false;
                string sqlcommandIdentifierName = ((Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)((Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)((Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax)((Microsoft.CodeAnalysis.SyntaxNode)binExpression).Parent).Left).Expression).Identifier.ValueText;
                var tempBlock = ((Microsoft.CodeAnalysis.SyntaxNode)binExpression).Parent.Parent.Parent;
                var Tempblock = tempBlock as BlockSyntax;
                ExpressionStatementSyntax newSyntax;
                //Check IdentifierName and StringLiteralExpression && Add syntaxnode in Templist  
                foreach (var t in TempNode)
                {
                    if (t.Kind() == SyntaxKind.IdentifierName || t.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        TempList.Add(t);
                        if (t.Kind() == SyntaxKind.IdentifierName) HaveVar = true;
                    }
                }
                if (binExpression == null || !HaveVar)
                {
                    return document;
                }
                else
                {
                    foreach (var i in TempList)
                    {
                        //Check stringliteral and delete @,",' 
                        string TempString = "";
                        if (i.Kind() == SyntaxKind.StringLiteralExpression)
                        {
                            TempString = i.ToString();
                            if (TempString.Contains("@"))
                            {
                                TempString = TempString.Replace("@", string.Empty);
                            }
                            if (TempString.Contains("\""))
                            {
                                TempString = TempString.Replace("\"", string.Empty);
                            }
                            if (TempString.Contains("\'"))
                            {
                                TempString = TempString.Replace("\'", string.Empty);
                            }
                        }

                        //Add @" in front of stringliteral 
                        if (i == TempList[0])
                        {

                            NewSqlCommand = string.Empty;
                            NewSqlCommand = "@\"" + TempString;

                        }
                        //Create newSqlQuery 
                        if (i.Kind() == SyntaxKind.StringLiteralExpression && i != TempList[0])
                        {
                            NewSqlCommand = NewSqlCommand + " " + TempString;
                        }
                        //Create @Parameters and create sqlParameter
                        if (i.Kind() == SyntaxKind.IdentifierName)
                        {
                            NewSqlCommand = NewSqlCommand + "@" + i;
                            newSyntax = SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.IdentifierName(sqlcommandIdentifierName),
                                                        SyntaxFactory.IdentifierName("Parameters")),
                                                    SyntaxFactory.IdentifierName("Add")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.ObjectCreationExpression(
                                                                SyntaxFactory.IdentifierName("SqlParameter"))
                                                            .WithNewKeyword(
                                                                SyntaxFactory.Token(
                                                                    SyntaxFactory.TriviaList(),
                                                                    SyntaxKind.NewKeyword,
                                                                    SyntaxFactory.TriviaList(
                                                                        SyntaxFactory.Space)))
                                                            .WithArgumentList(
                                                                SyntaxFactory.ArgumentList(
                                                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                        new SyntaxNodeOrToken[]{
                                                                        SyntaxFactory.Argument(
                                                                            SyntaxFactory.LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                SyntaxFactory.Literal("@"+i))),
                                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                        SyntaxFactory.Argument(
                                                                            SyntaxFactory.IdentifierName(i.ToString()))}))))))));
                            TempSyntax.Add(newSyntax);
                            //Create comma in case identifierName adjacent
                            if (index + 1 < TempList.Count)
                            {
                                if (TempList[index + 1].Kind() == SyntaxKind.IdentifierName)
                                {
                                    NewSqlCommand = NewSqlCommand + ",";
                                }

                            }
                        }
                        //Create " After the sentence
                        if (i == TempList[TempList.Count - 1])
                        {
                            NewSqlCommand = NewSqlCommand + "\"";
                        }

                        index++;
                    }
                    //Add new node and Create new node 
                    var newLiteral = SyntaxFactory.ParseExpression(NewSqlCommand)
                            .WithLeadingTrivia(binExpression.Right.GetLeadingTrivia())
                            .WithTrailingTrivia(binExpression.Right.GetTrailingTrivia());
                    var documentEditor = await DocumentEditor.CreateAsync(document);
                    documentEditor.ReplaceNode(binExpression, newLiteral);

                    foreach (var k in TempSyntax)
                    {
                        documentEditor.InsertAfter(Tempblock.Statements.Last(), k);
                        newDoc = documentEditor.GetChangedDocument();
                    }

                    return newDoc;

                }
        }
    }
}
