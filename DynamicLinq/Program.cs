using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace DynamicLinq
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            TestFilteringOnDictionary.Run();
            //TestFilteringOnObjects.Run();
        }    

        
        
    }
}