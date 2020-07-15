using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DynamicLinq.Model
{
    [DebuggerDisplay(@"[case id={CaseId}; type={Type}]; state={State}; status={Status}")]
    public class Case
    {
        public string CaseId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string State { get; set; }

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

        private static readonly IList<string> PossibleStates = new List<string> {"CA", "FL", "NY", "UT", "TX"};
        private static readonly IList<string> PossibleStatuses = new List<string> {"Opened", "Resolved", "Closed", "Deleted"};
    }
}