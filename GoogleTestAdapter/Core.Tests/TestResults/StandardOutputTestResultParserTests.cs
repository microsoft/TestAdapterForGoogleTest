﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using GoogleTestAdapter.DiaResolver;
using GoogleTestAdapter.Model;
using GoogleTestAdapter.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static GoogleTestAdapter.Tests.Common.TestMetadata.TestCategories;

namespace GoogleTestAdapter.TestResults
{
    [TestClass]
    public class StandardOutputTestResultParserTests : TestsBase
    {
        private string[] ConsoleOutput1 { get; } = {
            @"[==========] Running 3 tests from 1 test case.",
            @"[----------] Global test environment set-up.",
            @"[----------] 3 tests from TestMath",
            @"[ RUN      ] TestMath.AddFails",
            @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp(6): error: Value of: Add(10, 10)",
            @"  Actual: 20",
            @"Expected: 1000",
            @"[  FAILED  ] TestMath.AddFails (3 ms)",
            @"[ RUN      ] TestMath.AddPasses"
        };

        private string[] ConsoleOutput1WithInvalidDuration { get; } = {
            @"[==========] Running 3 tests from 1 test case.",
            @"[----------] Global test environment set-up.",
            @"[----------] 3 tests from TestMath",
            @"[ RUN      ] TestMath.AddFails",
            @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp(6): error: Value of: Add(10, 10)",
            @"  Actual: 20",
            @"Expected: 1000",
            @"[  FAILED  ] TestMath.AddFails (3 s)"
        };

        private string[] ConsoleOutput1WithThousandsSeparatorInDuration { get; } = {
            @"[==========] Running 3 tests from 1 test case.",
            @"[----------] Global test environment set-up.",
            @"[----------] 3 tests from TestMath",
            @"[ RUN      ] TestMath.AddFails",
            @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp(6): error: Value of: Add(10, 10)",
            @"  Actual: 20",
            @"Expected: 1000",
            @"[  FAILED  ] TestMath.AddFails (4,656 ms)",
        };

        private string[] ConsoleOutput2 { get; } = {
            @"[       OK ] TestMath.AddPasses(0 ms)",
            @"[ RUN      ] TestMath.Crash",
            @"unknown file: error: SEH exception with code 0xc0000005 thrown in the test body.",
        };

        private string[] ConsoleOutput3 { get; } = {
            @"[  FAILED  ] TestMath.Crash(9 ms)",
            @"[----------] 3 tests from TestMath(26 ms total)",
            @"",
            @"[----------] Global test environment tear-down",
            @"[==========] 3 tests from 1 test case ran. (36 ms total)",
            @"[  PASSED  ] 1 test.",
            @"[  FAILED  ] 2 tests, listed below:",
            @"[  FAILED  ] TestMath.AddFails",
            @"[  FAILED  ] TestMath.Crash",
            @"",
            @" 2 FAILED TESTS",
            @"",
        };

        private string[] ConsoleOutputWithOutputOfExe { get; } = {
            @"[==========] Running 1 tests from 1 test case.",
            @"[----------] Global test environment set-up.",
            @"[----------] 1 tests from TestMath",
            @"[ RUN      ] TestMath.AddPasses",
            @"Some output produced by the exe",
            @"[       OK ] TestMath.AddPasses(0 ms)",
            @"[----------] 1 tests from TestMath(26 ms total)",
            @"",
            @"[----------] Global test environment tear-down",
            @"[==========] 3 tests from 1 test case ran. (36 ms total)",
            @"[  PASSED  ] 1 test.",
        };

        private string[] ConsoleOutputWithPrefixingTest { get; } = {
            @"[==========] Running 2 tests from 1 test case.",
            @"[----------] Global test environment set-up.",
            @"[----------] 2 tests from TestMath",
            @"[ RUN      ] Test.AB",
            @"[       OK ] Test.A(0 ms)",
            @"[ RUN      ] Test.A",
            @"[       OK ] Test.A(0 ms)",
            @"[----------] 2 tests from TestMath(26 ms total)",
            @"",
            @"[----------] Global test environment tear-down",
            @"[==========] 2 tests from 1 test case ran. (36 ms total)",
            @"[  PASSED  ] 2 test.",
        };


        private List<string> CrashesImmediately { get; set; }
        private List<string> CrashesAfterErrorMsg { get; set; }
        private List<string> Complete { get; set; }
        private List<string> WrongDurationUnit { get; set; }
        private List<string> ThousandsSeparatorInDuration { get; set; }
        private List<string> PassingTestProducesConsoleOutput { get; set; }
        private List<string> CompleteStandardOutput { get; set; }

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();

            CrashesImmediately = new List<string>(ConsoleOutput1);

            CrashesAfterErrorMsg = new List<string>(ConsoleOutput1);
            CrashesAfterErrorMsg.AddRange(ConsoleOutput2);

            Complete = new List<string>(ConsoleOutput1);
            Complete.AddRange(ConsoleOutput2);
            Complete.AddRange(ConsoleOutput3);

            WrongDurationUnit = new List<string>(ConsoleOutput1WithInvalidDuration);

            ThousandsSeparatorInDuration = new List<string>(ConsoleOutput1WithThousandsSeparatorInDuration);

            PassingTestProducesConsoleOutput = new List<string>(ConsoleOutputWithOutputOfExe);

            CompleteStandardOutput = new List<string>(File.ReadAllLines(TestResources.Tests_ReleaseX64_Output, Encoding.Default));
        }


        [TestMethod]
        [TestCategory(Unit)]
        public void GetTestResults_CompleteOutput_ParsedCorrectly()
        {
            List<TestResult> results = ComputeTestResults(Complete);

            results.Count.Should().Be(3);

            results[0].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddFails");
            XmlTestResultParserTests.AssertTestResultIsFailure(results[0]);
            results[0].ErrorMessage.Should().NotContain(StandardOutputTestResultParser.CrashText);
            results[0].Duration.Should().Be(TimeSpan.FromMilliseconds(3));
            results[0].ErrorStackTrace.Should()
                .Contain(
                    @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp");

            results[1].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddPasses");
            XmlTestResultParserTests.AssertTestResultIsPassed(results[1]);
            results[1].Duration.Should().Be(StandardOutputTestResultParser.ShortTestDuration);

            results[2].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.Crash");
            XmlTestResultParserTests.AssertTestResultIsFailure(results[2]);
            results[2].ErrorMessage.Should().NotContain(StandardOutputTestResultParser.CrashText);
            results[2].Duration.Should().Be(TimeSpan.FromMilliseconds(9));
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void GetTestResults_OutputWithImmediateCrash_CorrectResultHasCrashText()
        {
            List<TestResult> results = ComputeTestResults(CrashesImmediately);

            results.Count.Should().Be(2);

            results[0].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddFails");
            XmlTestResultParserTests.AssertTestResultIsFailure(results[0]);
            results[0].ErrorMessage.Should().NotContain(StandardOutputTestResultParser.CrashText);
            results[0].Duration.Should().Be(TimeSpan.FromMilliseconds(3));
            results[0].ErrorStackTrace.Should().Contain(@"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp");

            results[1].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddPasses");
            XmlTestResultParserTests.AssertTestResultIsFailure(results[1]);
            results[1].ErrorMessage.Should().Contain(StandardOutputTestResultParser.CrashText);
            results[1].ErrorMessage.Should().NotContain("Test output:");
            results[1].Duration.Should().Be(TimeSpan.FromMilliseconds(0));
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void GetTestResults_OutputWithCrashAfterErrorMessage_CorrectResultHasCrashText()
        {
            List<TestResult> results = ComputeTestResults(CrashesAfterErrorMsg);

            results.Count.Should().Be(3);

            results[0].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddFails");
            XmlTestResultParserTests.AssertTestResultIsFailure(results[0]);
            results[0].ErrorMessage.Should().NotContain(StandardOutputTestResultParser.CrashText);
            results[0].Duration.Should().Be(TimeSpan.FromMilliseconds(3));
            results[0].ErrorStackTrace.Should().Contain(@"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp");

            results[1].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddPasses");
            XmlTestResultParserTests.AssertTestResultIsPassed(results[1]);
            results[1].Duration.Should().Be(StandardOutputTestResultParser.ShortTestDuration);

            results[2].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.Crash");
            XmlTestResultParserTests.AssertTestResultIsFailure(results[2]);
            results[2].ErrorMessage.Should().Contain(StandardOutputTestResultParser.CrashText);
            results[2].ErrorMessage.Should().Contain("Test output:");
            results[2].ErrorMessage.Should().Contain("unknown file: error: SEH exception with code 0xc0000005 thrown in the test body.");
            results[2].Duration.Should().Be(TimeSpan.FromMilliseconds(0));
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void GetTestResults_OutputWithInvalidDurationUnit_DefaultDurationIsUsedAndWarningIsProduced()
        {
            List<TestResult> results = ComputeTestResults(WrongDurationUnit);

            results.Count.Should().Be(1);
            results[0].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddFails");
            results[0].Duration.Should().Be(TimeSpan.FromMilliseconds(1));
            results[0].ErrorStackTrace.Should().Contain(@"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp");

            MockLogger.Verify(l => l.LogWarning(
                It.Is<string>(s => s.Contains("'[  FAILED  ] TestMath.AddFails (3 s)'"))), Times.Exactly(1));
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void GetTestResults_OutputWithThousandsSeparatorInDuration_ParsedCorrectly()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            try
            {
                List<TestResult> results = ComputeTestResults(ThousandsSeparatorInDuration);

                results.Count.Should().Be(1);
                results[0].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddFails");
                results[0].Duration.Should().Be(TimeSpan.FromMilliseconds(4656));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void GetTestResults_OutputWithConsoleOutput_ConsoleOutputIsIgnored()
        {
            List<TestResult> results = ComputeTestResults(PassingTestProducesConsoleOutput);

            results.Count.Should().Be(1);
            results[0].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("TestMath.AddPasses");
            XmlTestResultParserTests.AssertTestResultIsPassed(results[0]);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void GetTestResults_OutputWithPrefixingTest_BothTestsAreFound()
        {
            var cases = new List<TestCase>
            {
                TestDataCreator.ToTestCase("Test.AB", TestDataCreator.DummyExecutable,
                    @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp"),
                TestDataCreator.ToTestCase("Test.A", TestDataCreator.DummyExecutable,
                    @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp")
            };

            var results = new StandardOutputTestResultParser(cases, ConsoleOutputWithPrefixingTest, TestEnvironment.Logger)
                .GetTestResults();

            results.Count.Should().Be(2);
            results[0].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("Test.AB");
            XmlTestResultParserTests.AssertTestResultIsPassed(results[0]);
            results[1].TestCase.FullyQualifiedNameWithoutNamespace.Should().Be("Test.A");
            XmlTestResultParserTests.AssertTestResultIsPassed(results[1]);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void OutputHandling_OutputManyLinesWithNewlines_IsParsedCorrectly()
        {
            var results = GetTestResultsFromCompleteOutputFile();

            var testResult = results.Single(tr => tr.DisplayName == "OutputHandling.Output_ManyLinesWithNewlines");
            var expectedErrorMessage = 
                "before test 1\nbefore test 2\nExpected: 1\nTo be equal to: 2\ntest output\nafter test 1\nafter test 2";
            testResult.ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void OutputHandling_OutputOneLineWithNewlines_IsParsedCorrectly()
        {
            var results = GetTestResultsFromCompleteOutputFile();

            var testResult = results.Single(tr => tr.DisplayName == "OutputHandling.Output_OneLineWithNewlines");
            var expectedErrorMessage = 
                "before test\nExpected: 1\nTo be equal to: 2\ntest output\nafter test";
            testResult.ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void OutputHandling_OutputOneLine_IsParsedCorrectly()
        {
            var results = GetTestResultsFromCompleteOutputFile();

            var testResult = results.Single(tr => tr.DisplayName == "OutputHandling.Output_OneLine");
            var expectedErrorMessage =
                "before test\nExpected: 1\nTo be equal to: 2\ntest output\nafter test";
            testResult.ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void OutputHandling_ManyLinesWithNewlines_IsParsedCorrectly()
        {
            var results = GetTestResultsFromCompleteOutputFile();

            var testResult = results.Single(tr => tr.DisplayName == "OutputHandling.ManyLinesWithNewlines");
            var expectedErrorMessage =
                "before test 1\nbefore test 2\nExpected: 1\nTo be equal to: 2\nafter test 1\nafter test 2";
            testResult.ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void OutputHandling_OneLineWithNewlines_IsParsedCorrectly()
        {
            var results = GetTestResultsFromCompleteOutputFile();

            var testResult = results.Single(tr => tr.DisplayName == "OutputHandling.Output_OneLineWithNewlines");
            var expectedErrorMessage =
                "before test\nExpected: 1\nTo be equal to: 2\ntest output\nafter test";
            testResult.ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void OutputHandling_OneLine_IsParsedCorrectly()
        {
            var results = GetTestResultsFromCompleteOutputFile();

            var testResult = results.Single(tr => tr.DisplayName == "OutputHandling.OneLine");
            var expectedErrorMessage =
                "before test\nExpected: 1\nTo be equal to: 2\nafter test";
            testResult.ErrorMessage.Should().Be(expectedErrorMessage);
        }

        private IList<TestResult> GetTestResultsFromCompleteOutputFile()
        {
            var testCases = new GoogleTestDiscoverer(MockLogger.Object, MockOptions.Object, new DefaultDiaResolverFactory())
                .GetTestsFromExecutable(TestResources.Tests_ReleaseX64);
            return new StandardOutputTestResultParser(testCases, CompleteStandardOutput, MockLogger.Object)
                .GetTestResults();
        }


        private List<TestResult> ComputeTestResults(List<string> consoleOutput)
        {
            var cases = new List<TestCase>
            {
                TestDataCreator.ToTestCase("TestMath.AddFails", TestDataCreator.DummyExecutable,
                    @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp"),
                TestDataCreator.ToTestCase("TestMath.Crash", TestDataCreator.DummyExecutable,
                    @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp"),
                TestDataCreator.ToTestCase("TestMath.AddPasses", TestDataCreator.DummyExecutable,
                    @"c:\users\chris\documents\visual studio 2015\projects\consoleapplication1\consoleapplication1tests\source.cpp")
            };
            var parser = new StandardOutputTestResultParser(cases, consoleOutput, TestEnvironment.Logger);
            return parser.GetTestResults();
        }

    }

}