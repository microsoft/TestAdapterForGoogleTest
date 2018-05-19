using System.Collections.Generic;

namespace GoogleTestAdapter.Settings
{
    public interface ICTestPropertySettings
    {
        IDictionary<string, string> Environment { get; }
        string WorkingDirectory { get; }
    }
}
