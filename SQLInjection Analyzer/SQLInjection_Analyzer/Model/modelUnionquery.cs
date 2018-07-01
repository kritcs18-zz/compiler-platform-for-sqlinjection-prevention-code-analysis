using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInjection_Analyzer.Model
{
    public class modelUnionquery
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
}
