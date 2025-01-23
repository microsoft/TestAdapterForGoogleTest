using System.Collections.Generic;
using GoogleTestAdapter.Helpers;

namespace GoogleTestAdapter.TestCases
{

    public class ListTestsParser
    {
        private readonly string _testNameSeparator;
        private readonly bool _showFixtureMethodNode;

        public ListTestsParser(string testNameSeparator, bool showFixtureMethodNode)
        {
            _testNameSeparator = testNameSeparator;
            _showFixtureMethodNode = showFixtureMethodNode;
        }

        public IList<TestCaseDescriptor> ParseListTestsOutput(IEnumerable<string> consoleOutput)
        {
            var testCaseDescriptors = new List<TestCaseDescriptor>();

            var actualParser = new StreamingListTestsParser(_testNameSeparator, _showFixtureMethodNode);
            actualParser.TestCaseDescriptorCreated += (sender, args) => testCaseDescriptors.Add(args.TestCaseDescriptor);

            foreach (string line in consoleOutput)
            {
                actualParser.ReportLine(line);
            }
            return testCaseDescriptors;
        }

    }

}