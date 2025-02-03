// This file has been modified by Microsoft on 7/2017.

using System;
using System.Collections.Generic;
using System.Linq;
using GoogleTestAdapter.Model;

namespace GoogleTestAdapter.Helpers
{

    public static class AllKindsOfExtensions
    {

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static (IEnumerable<T> predicateTrue, IEnumerable<T> predicateFalse) Partition<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            var meetsCondition = list.Where(t => predicate(t));
            return (meetsCondition, list.Except(meetsCondition));
        }

        internal static IDictionary<string, List<TestCase>> GroupByExecutable(this IEnumerable<TestCase> testcases)
        {
            var groupedTestCases = new Dictionary<string, List<TestCase>>();
            foreach (TestCase testCase in testcases)
            {
                List<TestCase> group;
                if (groupedTestCases.ContainsKey(testCase.Source))
                {
                    group = groupedTestCases[testCase.Source];
                }
                else
                {
                    group = new List<TestCase>();
                    groupedTestCases.Add(testCase.Source, group);
                }
                group.Add(testCase);
            }
            return groupedTestCases;
        }

        internal static string AppendIfNotEmpty(this string theString, string appendix)
        {
            return string.IsNullOrWhiteSpace(theString) ? theString : theString + appendix;
        }

    }

}