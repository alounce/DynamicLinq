using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace DynamicLinq
{
    public class Case
    {
        public string CaseId { get; private set; }
        public string Type { get; private set; }
        public string Status { get; private set; }
        public string State { get; private set; }

        public override string ToString()
        {
            return $"<case id: {CaseId}; status: {Status}; state: {State}; type: {Type}>";
        }
        
        // factory method to get random case collection
        internal static IEnumerable<Case> GetCases()
        {
            var rand = new Random(33);
            var result = new List<Case>();
            for (var i = 0; i < 10; i++)
            {
                var caseId = Guid.NewGuid().ToString();
                
                var isEven = rand.Next(2) == 0;
                var type = isEven ? "claim" : "script";
                
                var statusIdx = rand.Next(0, PossibleStatuses.Count);
                var status = PossibleStatuses[statusIdx];
                
                var usStateIdx = rand.Next(0, PossibleStates.Count);
                var usState = PossibleStates[usStateIdx];
                
                
                result.Add(new Case { CaseId = caseId, Type = type, Status = status, State = usState });
            }
            return result;
        }

        private static readonly IList<string> PossibleStates = new[] {"CA", "FL", "NY", "UT", "TX"};
        private static readonly IList<string> PossibleStatuses = new[] {"Opened", "Resolved", "Closed", "Deleted"};
    }

    internal static class Program
    {
        static void Main(string[] args)
        {
            // for IQueryable ....
            var allCases = Case.GetCases().AsQueryable();
            Print(allCases, "All cases as IQueryable");
            
            // first condition (only NY scripts):
            const string condition1 = @"(Type = ""script"" AND State = ""NY"")";
            var caseParam = Expression.Parameter(typeof(Case), "Case");
            var expression1 = DynamicExpressionParser.ParseLambda(new[] { caseParam }, null, condition1);
            var filtered = (IQueryable<Case>) allCases.Where(expression1);
            Print(filtered, "Applied Dynamic condition #1 to IQueryable");
            
            // second condition (NY scripts OR all opened)
            const string condition2 = @"(Type = ""script"" AND State = ""NY"") OR (Status = ""Opened"")";
            var expression2 = DynamicExpressionParser.ParseLambda(new[] { caseParam }, null, condition2);
            filtered = (IQueryable<Case>) allCases.Where(expression2);
            Print(filtered, "Applied Dynamic condition #2 for IQueryable");
            
            // the same but for IEnumerable .... 
            var allCases2 = Case.GetCases().AsEnumerable();
            var expression3 = DynamicExpressionParser.ParseLambda<Case, bool>(ParsingConfig.Default, false, condition2, new[] { caseParam });
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