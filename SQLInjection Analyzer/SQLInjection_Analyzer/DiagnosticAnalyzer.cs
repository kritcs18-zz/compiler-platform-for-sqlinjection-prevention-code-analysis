using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SQLInjection_Analyzer.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace SQLInjection_Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SQLInjection_AnalyzerAnalyzer : DiagnosticAnalyzer
    {
        string _apikey = "Y0IvgWPDLzsXkreEudCW5IZTl5Igh/XF0VzZx21r+QSytVCHabv/RhfhPi5RfX/2xOkVb3ACyd+awSopadQdtw==";
        string getJson = "";
        string getResult = "";

        string reportWarningUnionquery = "0";
        string reportWarningIllegalquery = "0";
        string reportWarningPiggybackedquery = "0";

        string SingleLineComment = "0";
        string Semicolon = "0";
        string ThreeSingleQuote = "0";
        string TwoSingleQuote = "0";
        string TwoSingaporeQuote = "0";
        string TrueCaseZero = "0";
        string TrueCaseOne = "0";
        string TrueCaseCharX = "0";
        string TrueCaseVarA = "0";
        string TrueCaseCharA = "0";
        string DoubleQuote = "0";
        string MultipleLineComment = "0";
        string SETIDENTITYINSERT = "0";
        string TRUNCATETABLE = "0";
        string DROPtable = "0";
        string UPDATE = "0";
        string INSERTinto = "0";
        string DELETE = "0";
        string union = "0";
        string IllegalLogicallyIncorrectQuery = "0";
        string UnionQuery = "0";
        string PiggyBackedQuery = "0";

        

        public const string DiagnosticId = "SQLInjection_Analyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeConstructorNode, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzerAssignmentNode, SyntaxKind.SimpleAssignmentExpression);
        }

        //Analyzer CommandText
        private void AnalyzerAssignmentNode(SyntaxNodeAnalysisContext context)
        {
            var assignmentExpression = (AssignmentExpressionSyntax)context.Node;

            if (!assignmentExpression.Left.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                return;
            }

            if (!assignmentExpression.Left.ToString().Contains("CommandText"))
            {
                return;
            }

            RunDiagnostic(context, assignmentExpression.Right);
        }

        //Analyzer SqlCommand
        private void AnalyzeConstructorNode(SyntaxNodeAnalysisContext context)
        {
            var objectCreationExpression = (ObjectCreationExpressionSyntax)context.Node;

            if (!objectCreationExpression.Type.ToString().Contains("SqlCommand"))
            {
                return;
            }

            if (objectCreationExpression.ArgumentList.Arguments.Count == 0)
            {
                return;
            }

            ExpressionSyntax expressionSyntax = objectCreationExpression.ArgumentList.Arguments.First().Expression;
            RunDiagnostic(context, expressionSyntax);
        }

        //Check
        private void RunDiagnostic(SyntaxNodeAnalysisContext context, ExpressionSyntax expressionSyntax)
        {
            var literalExpression = expressionSyntax as LiteralExpressionSyntax;
            //Report error syntax
            if (literalExpression != null)
            {
                Diagnostics.LiteralExpressionDiagnostic.Run(context, literalExpression);
                //return;
            }
            //Detect binaryExpression and Report error syntax
            try
            {
                var binaryExpression = expressionSyntax as BinaryExpressionSyntax;
                var TempNode = binaryExpression.DescendantNodes();

                if (binaryExpression != null)
                {
                    foreach (var i in TempNode)
                    {
                        if (i.Kind() == SyntaxKind.IdentifierName)
                        {
                            Diagnostics.Parameters.Run(context);
                            break;
                        }
                    }

                    Diagnostics.BinaryExpressionDiagnostic.Run(context, binaryExpression);

                    //return;
                }
            }
            catch { }

            //Detect interpolatedExpression and Report error syntax 
            var interpolatedExpression = expressionSyntax as InterpolatedStringExpressionSyntax;
            if (interpolatedExpression != null)
            {
                Diagnostics.InterpolatedStringExpressionDiagnostic.Run(context, interpolatedExpression);
                //return;
            }
            Diagnostics.ExpressionDiagnostic.Run(context, expressionSyntax);

            //Start Detect Attribute
            try
            {
                if (literalExpression != null)
                {
                    string sql = literalExpression.Token.ValueText;
                    AzureML(sql);
                    SendData(getJson);
                    while (getResult == "") { }
                    Deserialize(getResult);
                    getResult = "";       
                }
                
            } catch { }
                
                //Report warning union query injection
                if (reportWarningUnionquery != "0")
                {
                    Diagnostics.UnionQuery.Run(context, literalExpression);
                }
                //Report warning illegal query injection
                if (reportWarningIllegalquery != "0")
                {
                    Diagnostics.Illegal.Run(context,literalExpression);
                }
                //Report warning piggybacked query injection
                if (reportWarningPiggybackedquery != "0")
                {
                    Diagnostics.Piggybacked.Run(context,literalExpression);
                }   
            }
        //Detect Attribute
        private void AzureML(string sql)
        {
            if (sql.Contains("--"))
            {
                SingleLineComment = "1";
            }
            else { SingleLineComment = "0"; }
            if (sql.Contains(";"))
            {
                Semicolon = "1";
            }
            else { Semicolon = "0"; }
            if (sql.Contains("'''"))
            {
                ThreeSingleQuote = "1";
            }
            else { ThreeSingleQuote = "0"; }
            if (sql.Contains("''"))
            {
                TwoSingleQuote = "1";
            }
            else { TwoSingleQuote = "0"; }
            if (sql.Contains("' '"))
            {
                TwoSingaporeQuote = "1";
            }
            else { TwoSingaporeQuote = "0"; }
            if (sql.Contains("0=0"))
            {
                TrueCaseZero = "1";
            }
            else { TrueCaseZero = "0"; }
            if (sql.Contains("1=1"))
            {
                TrueCaseOne = "1";
            }
            else { TrueCaseOne = "0"; }
            if (sql.Contains("'x'='x'"))
            {
                TrueCaseCharX = "1";
            }
            else { TrueCaseCharX = "0"; }
            if (sql.Contains("a=a"))
            {
                TrueCaseVarA = "1";
            }
            else { TrueCaseVarA = "0"; }
            if (sql.Contains("'a'='a'"))
            {
                TrueCaseCharA = "1";
            }
            else { TrueCaseCharA = "0"; }
            if (sql.Contains("\""))
            {
                DoubleQuote = "1";
            }
            else { DoubleQuote = "0"; }
            if (sql.Contains("/*"))
            {
                MultipleLineComment = "1";
            }
            else { MultipleLineComment = "0"; }
            if (sql.Contains("; SET IDENTITY_INSERT"))
            {
                SETIDENTITYINSERT = "1";
            }
            else { SETIDENTITYINSERT = "0"; }
            if (sql.Contains("; TRUNCATE TABLE"))
            {
                TRUNCATETABLE = "1";
            }
            else { TRUNCATETABLE = "0"; }
            if (sql.Contains("; DROP table"))
            {
                DROPtable = "1";
            }
            else { DROPtable = "0"; }
            if (sql.Contains("; UPDATE"))
            {
                UPDATE = "1";
            }
            else { UPDATE = "0"; }
            if (sql.Contains("; INSERT into"))
            {
                INSERTinto = "1";
            }
            else { INSERTinto = "0"; }
            if (sql.Contains("; DELETE"))
            {
                DELETE = "1";
            }
            else { DELETE = "0"; }
            if (sql.Contains("union"))
            {
                union = "1";
            }
            else { union = "0"; }

            //Model 
            var objects = new First()
            {
                Inputs = new Second()
                {
                    
                    input3 = new modelUnionquery()
                    {
                        ColumnNames = new string[]
                         {
                            "SingleLine Comment",
                            "Semicolon",
                            "Three Single Quote",
                            "Two Single Quote",
                            "Two Singapore Quote",
                            "True Case Zero",
                            "True Case One",
                            "True Case CharX",
                            "True Case VarA",
                            "True Case CharA",
                            "Double Quote",
                            "Multiple Line Comment",
                            "SET IDENTITY_INSERT",
                            "TRUNCATE TABLE",
                            "DROP table",
                            "UPDATE",
                            "INSERT into",
                            "DELETE",
                            "union",
                            "Union Query"
                         },
                        Values = new string[,]
                         {
                            {
                                SingleLineComment,
                                Semicolon,
                                ThreeSingleQuote,
                                TwoSingleQuote,
                                TwoSingaporeQuote,
                                TrueCaseZero,
                                TrueCaseOne,
                                TrueCaseCharX,
                                TrueCaseVarA,
                                TrueCaseCharA,
                                DoubleQuote,
                                MultipleLineComment,
                                SETIDENTITYINSERT,
                                TRUNCATETABLE,
                                DROPtable,
                                UPDATE,
                                INSERTinto,
                                DELETE,
                                union,
                                UnionQuery
                            }
                        }
                    },
               
                    
                    input1 = new modelIllegalquery()
                    {
                        ColumnNames = new string[]
                         {
                            "SingleLine Comment",
                            "Semicolon",
                            "Three Single Quote",
                            "Two Single Quote",
                            "Two Singapore Quote",
                            "True Case Zero",
                            "True Case One",
                            "True Case CharX",
                            "True Case VarA",
                            "True Case CharA",
                            "Double Quote",
                            "Multiple Line Comment",
                            "SET IDENTITY_INSERT",
                            "TRUNCATE TABLE",
                            "DROP table",
                            "UPDATE",
                            "INSERT into",
                            "DELETE",
                            "union",
                            "Illegal/Logically Incorrect Query"
                         },
                        Values = new string[,]
                         {
                            {
                                SingleLineComment,
                                Semicolon,
                                ThreeSingleQuote,
                                TwoSingleQuote,
                                TwoSingaporeQuote,
                                TrueCaseZero,
                                TrueCaseOne,
                                TrueCaseCharX,
                                TrueCaseVarA,
                                TrueCaseCharA,
                                DoubleQuote,
                                MultipleLineComment,
                                SETIDENTITYINSERT,
                                TRUNCATETABLE,
                                DROPtable,
                                UPDATE,
                                INSERTinto,
                                DELETE,
                                union,
                                IllegalLogicallyIncorrectQuery
                            }
                        }
                    },
                    input2 = new modelPiggybackedquery()
                    {
                        ColumnNames = new string[]
                         {
                            "SingleLine Comment",
                            "Semicolon",
                            "Three Single Quote",
                            "Two Single Quote",
                            "Two Singapore Quote",
                            "True Case Zero",
                            "True Case One",
                            "True Case CharX",
                            "True Case VarA",
                            "True Case CharA",
                            "Double Quote",
                            "Multiple Line Comment",
                            "SET IDENTITY_INSERT",
                            "TRUNCATE TABLE",
                            "DROP table",
                            "UPDATE",
                            "INSERT into",
                            "DELETE",
                            "union",
                            "PiggyBacked Query"
                         },
                        Values = new string[,]
                         {
                            {
                                SingleLineComment,
                                Semicolon,
                                ThreeSingleQuote,
                                TwoSingleQuote,
                                TwoSingaporeQuote,
                                TrueCaseZero,
                                TrueCaseOne,
                                TrueCaseCharX,
                                TrueCaseVarA,
                                TrueCaseCharA,
                                DoubleQuote,
                                MultipleLineComment,
                                SETIDENTITYINSERT,
                                TRUNCATETABLE,
                                DROPtable,
                                UPDATE,
                                INSERTinto,
                                DELETE,
                                union,
                                PiggyBackedQuery
                            }
                        }
                    }
                },

                GlobalParameters = new Dictionary<string, string>() { }
            };
            getJson = JsonConvert.SerializeObject(objects);
        }
        //Send Data to AzureML
        private async void SendData(string getJson)
         {
            try
            {
                Uri uri = new Uri("https://asiasoutheast.services.azureml.net/workspaces/c3874d2f3c654288a19877ef68901e1d/services/58b9cd9c82594b83b3f6c3ac10c42d47/execute?api-version=2.0&details=true");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apikey);
                HttpResponseMessage response = await client.PostAsync(uri, new StringContent(getJson, System.Text.Encoding.UTF8, "application/json"));
                getResult = await response.Content.ReadAsStringAsync();
             }
            catch { }
         }
        //Result from AzureML
        private void Deserialize(string getResult)
        {
            var parseJson = JObject.Parse(getResult);

            reportWarningUnionquery = (string)parseJson["Results"]["output3"]["value"]["Values"][0][20];
            reportWarningPiggybackedquery = (string)parseJson["Results"]["output2"]["value"]["Values"][0][20];
            reportWarningIllegalquery = (string)parseJson["Results"]["output1"]["value"]["Values"][0][20];
            if (reportWarningUnionquery == "0")
            {
                reportWarningUnionquery = "0";
            }
            if (reportWarningIllegalquery == "0")
            {
                reportWarningIllegalquery = "0";
            }
            if (reportWarningPiggybackedquery == "0")
            {
                reportWarningPiggybackedquery = "0";
            } 
        }
    }
}
