// This file has been modified by Microsoft on 9/2017.

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GoogleTestAdapter.Common;
using GoogleTestAdapter.Framework;
using GoogleTestAdapter.Helpers;
using GoogleTestAdapter.Model;

namespace GoogleTestAdapter.TestResults
{
    public class StreamingStandardOutputTestResultParser
    {
        public static readonly Regex PrefixedLineRegex;
        public static readonly Regex FixtureMethodResultRegex;

        public TestCase CrashedTestCase { get; private set; }
        public IList<TestResult> TestResults { get; } = new List<TestResult>();
        public List<TestResult> XmlTestResults { get; set; } = new List<TestResult>();

        // Used so that we only get the XML results once per test run.
        private bool doXMLResultsExist = false;
        private readonly List<TestCase> _testCasesRun;
        private readonly ILogger _logger;
        private readonly ITestFrameworkReporter _reporter;

        private readonly object _syncObject = new object();
        private readonly List<string> _consoleOutput = new List<string>();
        private readonly string _executable;

        static StreamingStandardOutputTestResultParser()
        {
            string passedMarker = Regex.Escape(StandardOutputTestResultParser.Passed);
            string failedMarker = Regex.Escape(StandardOutputTestResultParser.Failed);
            PrefixedLineRegex = new Regex($"(.+)((?:{passedMarker}|{failedMarker}).*)", RegexOptions.Compiled);
            FixtureMethodResultRegex = new Regex($@"(?:{failedMarker}\s*)(\w+):(?:\s+{StandardOutputTestResultParser.FailedFixture})", RegexOptions.Compiled);
        }

        public StreamingStandardOutputTestResultParser(IEnumerable<TestCase> testCasesRun,
                ILogger logger, ITestFrameworkReporter reporter, string executable)
        {
            _testCasesRun = testCasesRun.ToList();
            _logger = logger;
            _reporter = reporter;
            _executable = executable;
        }

        public List<TestResult> GetXMLResults(IEnumerable<TestCase> testCasesRun)
        {
            // Parse in XML so we can get the error message and stack trace in UTF8 since console output does not support this by default.
            string xmlResultFilePath = Path.Combine(Path.GetDirectoryName(_executable), "XMLGoogleTestResults.xml");
            var xmlParser = new XmlTestResultParser(testCasesRun, xmlResultFilePath, _logger);
            var xmlTestResults = xmlParser.GetTestResults();
            return xmlTestResults;
        }

        public void ReportLine(string line)
        {
            lock (_syncObject)
            {
                Match testEndMatch = PrefixedLineRegex.Match(line);
                if (testEndMatch.Success)
                {
                    string restOfErrorMessage = testEndMatch.Groups[1].Value;
                    if (!string.IsNullOrEmpty(restOfErrorMessage))
                        DoReportLine(restOfErrorMessage);

                    string testEndPart = testEndMatch.Groups[2].Value;
                    DoReportLine(testEndPart);
                }
                else
                {
                    DoReportLine(line);
                }
            }
        }

        private void DoReportLine(string line)
        {
            if (StandardOutputTestResultParser.IsRunLine(line))
            {
                if (_consoleOutput.Count > 0)
                {
                    ReportTestResult();
                    _consoleOutput.Clear();
                }
                ReportTestStart(line);
            }
            else if (StandardOutputTestResultParser.IsFailedLine(line) && line.Contains(StandardOutputTestResultParser.FailedFixture))
            {
                ReportFixtureMethodFailure(line);
            }

            _consoleOutput.Add(line);
        }

        public void Flush()
        {
            lock (_syncObject)
            {
                if (_consoleOutput.Count > 0)
                {
                    ReportTestResult();
                    _consoleOutput.Clear();
                }
            }
        }

        private void ReportTestStart(string line)
        {
            string qualifiedTestname = StandardOutputTestResultParser.RemovePrefix(line).Trim();
            TestCase testCase = StandardOutputTestResultParser.FindTestcase(qualifiedTestname, _testCasesRun);
            if (testCase != null)
                _reporter.ReportTestsStarted(testCase.Yield());
        }

        private void ReportTestResult()
        {
            TestResult result = CreateTestResult();
            if (result != null)
            {
                _reporter.ReportTestResults(result.Yield());
                TestResults.Add(result);
            }
        }

        private void ReportFixtureMethodFailure(string line)
        {
            // Google test reports fixture method failures ambiguously with the output:
            // [  FAILED  ] TestMe: SetUpTestSuite or TearDownTestSuite
            // For V1, we fail both SetUp and TearDown nodes if a failure is reported.
            string suite = FixtureMethodResultRegex.Match(line).Groups[1].Value;
            string[] supportedFixtureMethods = { GoogleTestConstants.SetUpFixtureMethod, GoogleTestConstants.TearDownFixtureMethod };
            foreach (string fixtureMethodName in supportedFixtureMethods)
            {
                string qualifiedTestName = $"{suite}.{fixtureMethodName}";
                TestCase testCase = StandardOutputTestResultParser.FindTestcase(qualifiedTestName, _testCasesRun);
                if(testCase != null)
                {
                    TestResult result = StandardOutputTestResultParser.CreateFailedTestResult(testCase, TimeSpan.FromMilliseconds(0), "", "", "");
                    if (result != null)
                    {
                        _reporter.ReportTestResults(result.Yield());
                        TestResults.Add(result);
                    }
                }
            }
        }

        private TestResult CreateTestResult()
        {
            int currentLineIndex = 0;
            while (currentLineIndex < _consoleOutput.Count &&
                !StandardOutputTestResultParser.IsRunLine(_consoleOutput[currentLineIndex]))
                currentLineIndex++;

            if (currentLineIndex == _consoleOutput.Count)
                return null;

            string line = _consoleOutput[currentLineIndex++];
            string qualifiedTestname = StandardOutputTestResultParser.RemovePrefix(line).Trim();
            TestCase testCase = StandardOutputTestResultParser.FindTestcase(qualifiedTestname, _testCasesRun);
            if (testCase == null)
            {
                _logger.DebugWarning(String.Format(Resources.NoKnownTestCaseMessage, line));
                return null;
            }

            if (currentLineIndex == _consoleOutput.Count)
            {
                CrashedTestCase = testCase;
                return StandardOutputTestResultParser.CreateFailedTestResult(
                    testCase,
                    TimeSpan.FromMilliseconds(0),
                    StandardOutputTestResultParser.CrashText,
                    "",
                    "");
            }

            line = _consoleOutput[currentLineIndex++];

            // Capture all output and treat it as standard output for the test explorer. If we wanted to distinguish between stdout and stderr, we have
            // to change the way we capture the output in ProcessLauncher and ProcessExecutor since they merge the two streams.
            string consoleOutput = "";
            while (
                !(StandardOutputTestResultParser.IsFailedLine(line)
                    || StandardOutputTestResultParser.IsPassedLine(line))
                && currentLineIndex <= _consoleOutput.Count)
            {
                consoleOutput += line + "\n";
                line = currentLineIndex < _consoleOutput.Count ? _consoleOutput[currentLineIndex] : "";
                currentLineIndex++;
            }
            if (StandardOutputTestResultParser.IsFailedLine(line))
            {
                string testResultErrorMessage = String.Empty;
                string testResultErrorStackTrace = String.Empty;
                string xmlErrorMessage = String.Empty;
                string xmlErrorStackTrace = String.Empty;

                // Map and extract error messages from xml parser to inject into this test failed result. This allows support for UTF8 error messages. Bug 1951549.
                //  Console output does not always support UTF8 output by default, but XML results do.
                if (!doXMLResultsExist)
                {
                    XmlTestResults = GetXMLResults(_testCasesRun.AsEnumerable());
                    doXMLResultsExist = true;
                }

                foreach (var xmlTestResult in XmlTestResults)
                {
                    if (xmlTestResult.TestCase.FullyQualifiedNameWithNamespace == testCase.FullyQualifiedNameWithNamespace)
                    {
                        testResultErrorMessage = xmlTestResult.ErrorMessage;
                        testResultErrorStackTrace = xmlTestResult.ErrorStackTrace;
                    }
                }

                // If we did not find the error message or stack trace in the XML parser, then parse the error message from the console output.
                if (testResultErrorMessage == String.Empty || testResultErrorStackTrace == String.Empty)
                {
                    ErrorMessageParser parser = new ErrorMessageParser(consoleOutput);
                    parser.Parse();
                    testResultErrorMessage = parser.ErrorMessage;
                    testResultErrorStackTrace = parser.ErrorStackTrace;
                }

                return StandardOutputTestResultParser.CreateFailedTestResult(
                    testCase,
                    StandardOutputTestResultParser.ParseDuration(line, _logger),
                    testResultErrorMessage,
                    testResultErrorStackTrace,
                    consoleOutput);
            }
            if (StandardOutputTestResultParser.IsPassedLine(line))
            {
                return StandardOutputTestResultParser.CreatePassedTestResult(
                    testCase,
                    StandardOutputTestResultParser.ParseDuration(line, _logger),
                    consoleOutput);
            }

            CrashedTestCase = testCase;
            string message = StandardOutputTestResultParser.CrashText;
            message += consoleOutput == "" ? "" : ("\n" + Resources.TestOutput + $"\n\n{consoleOutput}");
            TestResult result = StandardOutputTestResultParser.CreateFailedTestResult(
                testCase,
                TimeSpan.FromMilliseconds(0),
                message,
                "",
                consoleOutput);
            return result;
        }

    }

}