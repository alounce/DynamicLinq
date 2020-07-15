using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using DynamicLinq.Model;

namespace DynamicLinq.Old
{
    public static class TestFilteringOnObjects
    {
        public static void Run()
        {
            // STEP1: for IQueryable we can use Expressions ....
            var allCases = Case.GetCases().AsQueryable();
            Print(allCases, "All cases as IQueryable");
            
            // first condition (only NY scripts):
            const string condition1 = @"(Type = ""script"" AND State = ""NY"")";
            var caseParam = Expression.Parameter(typeof(Case), "Case");
            var expression1 = DynamicExpressionParser.ParseLambda(
                new[] { caseParam }, null, condition1);
            var filtered = (IQueryable<Case>) allCases.Where(expression1);
            Print(filtered, "Applied Dynamic condition #1 to IQueryable");
            
            // second condition (NY scripts OR all opened)
            const string condition2 = @"(Type = ""script"" AND State = ""NY"") OR (Status = ""Opened"")";
            var expression2 = DynamicExpressionParser.ParseLambda(
                new[] { caseParam }, null, condition2);
            filtered = (IQueryable<Case>) allCases.Where(expression2);
            Print(filtered, "Applied Dynamic condition #2 for IQueryable");
            
            
            // STEP 2: the same like above but for IEnumerable, where we compile Expression to Lambda .... 
            var allCases2 = Case.GetCases().AsEnumerable();
            var expression3 = DynamicExpressionParser.ParseLambda<Case, bool>(
                ParsingConfig.Default, false, condition2, caseParam);
            // build the lambda from an expression
            var predicateDelegate = expression3.Compile();
            var filtered2 = allCases2.Where(predicateDelegate);
            Print(filtered2, "Applied Dynamic condition #2 for IEnumerable");
        }
        
        private static void Print(IEnumerable<Case> cases, string caption)
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"-- {caption}");
            if (cases is IQueryable queryable)
            {
                Console.WriteLine($"-- {queryable.Expression}");    
            }
            Console.WriteLine(string.Join("\n", cases.Select(x=>x.ToString())));
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();
        }
    }
}