using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLInjection_Analyzer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SQLInjection_Analyzer.Diagnostics
{
    internal class BinaryExpressionDiagnostic
    {
        public const string DiagnosticId = "SQLInjection_Analyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        public static string _apikey = "o6B8eZRysBy3G3AwN13dVYYjPbVtJMl12KYdtsnNPHxqVBodtsnHZYlle7R1KnN2XPVBtRk6m+XpsU+M5919pw==";

        public static string getJson = "";
        public static string getResult = "";

        public static string reportWarningUnionquery = "0";
        public static string reportWarningIllegalquery = "0";
        public static string reportWarningPiggybackedquery = "0";

        public static string SingleLineComment = "";
        public static string Semicolon = "0";
        public static string ThreeSingleQuote = "0";
        public static string TwoSingleQuote = "0";
        public static string TwoSingaporeQuote = "0";
        public static string TrueCaseZero = "0";
        public static string TrueCaseOne = "0";
        public static string TrueCaseCharX = "0";
        public static string TrueCaseVarA = "0";
        public static string TrueCaseCharA = "0";
        public static string DoubleQuote = "0";
        public static string MultipleLineComment = "0";
        public static string SETIDENTITYINSERT = "0";
        public static string TRUNCATETABLE = "0";
        public static string DROPtable = "0";
        public static string UPDATE = "0";
        public static string INSERTinto = "0";
        public static string DELETE = "0";
        public static string union = "0";
        public static string IllegalLogicallyIncorrectQuery = "0";
        public static string UnionQuery = "0";
        public static string PiggyBackedQuery = "0";


        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
        internal static void Run(SyntaxNodeAnalysisContext context,BinaryExpressionSyntax token)
        {   //Check binaryexpression case 
            string id = token.ToFullString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }
            if (id.Contains("+") == false)
            {
                return;
            }
            //Split string
            string[] list = id.Split('+');
            string sql = BuildSqlStringFromList(list,context, id);

            try
            {
                AzureML(sql);
                sendData(getJson);
                while (getResult == "") { }
                Deserialize(getResult);
                getResult = "";
            } catch { }

            //Report warning union query injection
            if (reportWarningUnionquery != "0")
            {
                Diagnostics.UnionQueryBinary.Run(context);
            }
            //Report warning illegal query injection
            if (reportWarningIllegalquery != "0")
            {
                Diagnostics.IllegalBinary.Run(context);
            }
            //Report warning piggybacked query injection
            if (reportWarningPiggybackedquery != "0")
            {
                Diagnostics.PiggybackedBinary.Run(context);
            }
            //Check string 
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }
            //Check Error
            List<string> errors = SqlParser.Parse(sql);
            if (errors.Count == 0)
            {
                return;
            }
            //Report Error
            string errorText = String.Join("\r\n",errors);
            var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(),errorText);

            context.ReportDiagnostic(diagnostic);
        }
        //Result from AzureML
        private static void Deserialize(string getResult)
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
        //Send Data to AzureML
        private async static void sendData(string getJson)
        {
            try
            {
                Uri uri = new Uri("https://asiasoutheast.services.azureml.net/workspaces/c3874d2f3c654288a19877ef68901e1d/services/85c81e6f8b3e46baa7e2e559cad8d67d/execute?api-version=2.0&details=true");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apikey);
                HttpResponseMessage response = await client.PostAsync(uri, new StringContent(getJson, System.Text.Encoding.UTF8, "application/json"));
                getResult = await response.Content.ReadAsStringAsync();
            }
            catch { }
        }
        //Detect Attribute
        private static void AzureML(string sql)
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
                    }
                },
                GlobalParameters = new Dictionary<string, string>() { }
            };
            getJson = JsonConvert.SerializeObject(objects);
        }
        //Split string
        private static string BuildSqlStringFromList(string[] list,SyntaxNodeAnalysisContext context,string id)
        {
            string sql = string.Empty;
            foreach (string s in list)
            {
                if (s.Contains("\""))
                {
                    sql += s.Replace("\"",string.Empty);
                }
                else
                {
                    id = s.Replace(" ","");

                    BlockSyntax method = context.Node.FirstAncestorOrSelf<BlockSyntax>();
                    if (method == null)
                    {
                        break;
                    }
                    try
                    {
                        var t = method.DescendantTokens().Where<SyntaxToken>(st => st.ValueText == id).First<SyntaxToken>();

                        if (string.IsNullOrWhiteSpace(t.ValueText))
                        {
                            break;
                        }
                        sql += t.GetNextToken().GetNextToken().Value.ToString();
                    } catch { }
                }
            }
            return sql;
        }
    }
}
