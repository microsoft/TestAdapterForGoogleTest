using System.Collections.Generic;

namespace GoogleTestAdapter.Model
{
    public class TestCase
    {
        public string Source { get; }

        public string FullyQualifiedNameWithoutNamespace { get; }
        public string FullyQualifiedName { get; }
        public string DisplayName { get; }

        public string CodeFilePath { get; }
        public int LineNumber { get; }

        public List<Trait> Traits { get; } = new List<Trait>();
        public List<TestProperty> Properties { get; } = new List<TestProperty>();

        public TestCase(string fullyQualifiedNameWithoutNamespace, string fullyQualifiedName, string source, string displayName, string codeFilePath, int lineNumber)
        {
            FullyQualifiedNameWithoutNamespace = fullyQualifiedNameWithoutNamespace;
            FullyQualifiedName = fullyQualifiedName;
            Source = source;
            DisplayName = displayName;
            CodeFilePath = codeFilePath;
            LineNumber = lineNumber;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TestCase;

            if (other == null)
                return false;

            return FullyQualifiedNameWithoutNamespace == other.FullyQualifiedNameWithoutNamespace && Source == other.Source;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + FullyQualifiedNameWithoutNamespace.GetHashCode();
            hash = hash * 31 + Source.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return DisplayName;
        }

    }

}