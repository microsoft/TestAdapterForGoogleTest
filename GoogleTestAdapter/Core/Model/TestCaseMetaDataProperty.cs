using System;
using System.Linq;

namespace GoogleTestAdapter.Model
{
    public class TestCaseMetaDataProperty : TestProperty
    {
        public static readonly string Id = $"{typeof(TestCaseMetaDataProperty).FullName}";
        public const string Label = "Test case meta data";

        public int NrOfTestCasesInSuite { get; }
        public int NrOfTestCasesInExecutable { get; }
        public string FullyQualifiedNameWithoutNamespace { get; }

        public TestCaseMetaDataProperty(int nrOfTestCasesInSuite, int nrOfTestCasesInExecutable, string fullyQualifiedNameWithoutNamespace)
            : this($"{nrOfTestCasesInSuite}|{nrOfTestCasesInExecutable}|{fullyQualifiedNameWithoutNamespace}")
        {
        }

        public TestCaseMetaDataProperty(string serialization) : base(serialization)
        {
            string[] fields = serialization.Split('|');
            if (fields.Length != 3)
                throw new ArgumentException(serialization, nameof(serialization));
            NrOfTestCasesInSuite = int.Parse(fields[0]);
            NrOfTestCasesInExecutable = int.Parse(fields[1]);
            FullyQualifiedNameWithoutNamespace = fields[2];
        }
    }
}