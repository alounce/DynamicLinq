using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamicLinq
{
    // Not sure it will work with JObject, but will work with IDictionary
    // So we can receive JSONS, parse them to JObject and then convert to IDictionary<string, object>
    //    var json = JObject.Parse(jsonString)
    //    var dict = json.ToObject<IDictionary<string, object>>();
    // As well as vise versa, from IDictionary<string, object> to JObject
    //    var json = JObject.FromObject(dict);
    public static class TestConditionOnContext
    {
        private static IDictionary CaseDict => 
            new Dictionary<string, object>
            {
                {"CaseId", Guid.NewGuid().ToString()},
                {"State", "UT"},
                {"Status", "Opened"},
                {"Type", "script"}
            };
        
        private static IDictionary UserDict => 
            new Dictionary<string, object>
            {
                {"UserId", Guid.NewGuid().ToString()},
                {"UserName", "John Doe"},
                {"Status", "Active"},
                {"Type", "Software Developer"}
            };
        
        private static IDictionary CustomDict => 
            new Dictionary<string, object>
            {
                {"AlwaysPass", "YES"}
            };

        public static void Run()
        {
            // Context will contains one or more objects that we are going to validate with the condition
            // where 
            // context.Key = EntityName
            // context.Value = EntityData which in turn is the IDictionary<string, object>
            var context = new Dictionary<string, object> { { "Case", CaseDict } };
            
            // TEST #1: Simple condition on 1 object
            Test("TEST #1: Should PASS: Simple condition on 1 object...",
                @"Case.Type = ""script""",  context);
            
            // TEST #2: Two objects in the context 
            context = new Dictionary<string, object> { { "Case", CaseDict },  { "User", UserDict } };
            Test("TEST #2: Should PASS: Simple OR condition on 2 objects...",
                @"(Case.State = ""UT"") OR (User.Status = ""Active"")", context);
            
            // TEST #3: Example that should not pass the test
            Test("TEST #3: Should FAIL: Similar to previous",
                @"(Case.State = ""CA"") OR (User.Status = ""Suspended"")", context);
            
            // TEST #4: Some more crazy thing, like 3 tested object in the context
            context = new Dictionary<string, object> { { "Case", CaseDict },  { "User", UserDict }, { "Others", CustomDict } };
            Test("TEST #4: Should PASS: Complex condition on 3 objects...",
                @"(Others.AlwaysPass = ""YES"") OR ((Case.State = ""UT"") AND (User.Status = ""Active""))", context);
            
            // TEST #5: let's change context to make left-hand side condition false... 
            ((IDictionary)context["Others"])["AlwaysPass"] = "NO";
            Test("TEST #5: Should PASS: Similar to above but the left-hand condition is False, but right-hand is True",
                @"(Others.AlwaysPass = ""YES"") OR (Case.State = ""UT"" AND User.Status = ""Active"")", context);
            
            // TEST #6: and last one should NOT pass, because both left and right hand side condition are false
            ((IDictionary)context["User"])["Status"] = "Suspended";
            Test("TEST #6: Should PASS: Similar to above but the left-hand condition is False, but right-hand is True",
                @"(Others.AlwaysPass = ""YES"") OR (Case.State = ""UT"" AND User.Status = ""Active"")", context);
        }

        private static bool Test(
            string description, 
            string condition,
            IDictionary<string, object> context)
        {
            // Execute all logic
            // Generate params array that is used in the condition
            var parameters = context.Keys.Select(n => Expression.Parameter(typeof(IDictionary), n)).ToArray();
            // Parse condition to Expression
            var expression = DynamicExpressionParser.ParseLambda(parameters, null, condition);
            // Turn an expression to the lambda
            var lambda = expression.Compile();
            // ...and test our context.... 
            var objects = context.Values.ToArray();
            var result = lambda.DynamicInvoke(objects);
            var isPassed = Convert.ToBoolean(result);
            
            // (not interesting and rather boring stuff) Write a report
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Description:\t{description}");
            Console.WriteLine($"Condition:\t{condition}");
            Console.WriteLine($"Context:\t{context.Count} item(s)");
            foreach (var (entityName, entityDict) in context)
            {
                var s = string.Join("\n", JObject.FromObject(entityDict).ToString(Formatting.None));
                Console.WriteLine($"\t\t{entityName}: {s}");
            }
            Console.WriteLine($"Expression:\t{expression}");
            Console.WriteLine($"Is it passed?:\t{isPassed}");
            Console.WriteLine();
            return isPassed;
        }
    }
    
}