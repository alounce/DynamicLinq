using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DynamicLinq
{
    public static class TestFilteringOnDictionary
    {
        public static void Run()
        {
            // Gets all cases as a List of Dictionaries,
            // where each dictionary represents the case object as string key and object as value
            // IList<IDictionary<string, object>> 
            IList<IDictionary> cases = GetCasesAsDictionaryList();
            Print(cases, "All cases as list of dictionaries (key=value)");
            
            // condition against which we are going to test all our dictionaries
            const string condition = @"(Type = ""script"" AND State = ""NY"") OR (Status = ""Opened"")";
            
            // generate an expression from the given condition... 
            var elementParam = Expression.Parameter(typeof(IDictionary));
            var config = ParsingConfig.Default;
            config.RenameParameterExpression = true;
            var expr = DynamicExpressionParser.ParseLambda<IDictionary, bool>(config, false, condition, elementParam);
            Console.WriteLine($"-- Expression: {expr}");
            
            // compile it to lambda
            var lambda = expr.Compile();
            
            // ... and apply this lambda to the list of our dictionaries
            var filtered = (IEnumerable<IDictionary>) cases.Where(lambda);
            Print(filtered, "Applied Dynamic condition #1 to IQueryable");
            
        }

        private static IList<IDictionary> GetCasesAsDictionaryList()
        {
            var caseType = typeof(Case);
            var props = caseType.GetProperties();
            var cases = Case
                .GetCases()
                .Select(c =>
                {
                    var result = new Dictionary<string, object>();
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(c);
                        result.Add(prop.Name, value);
                    }
                    return result;
                })
                .ToList<IDictionary>();
            return cases;
        }

        private static void Print(IEnumerable<IDictionary> cases, string caption)
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"-- {caption}");
            if (cases is IQueryable queryable)
            {
                Console.WriteLine($"-- {queryable.Expression}");    
            }
            Console.WriteLine(string.Join("\n", cases.Select(x=> $"{x["CaseId"]};\t {x["Type"]};  \t {x["Status"]};\t {x["State"]};")));
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();
        }
    }
}