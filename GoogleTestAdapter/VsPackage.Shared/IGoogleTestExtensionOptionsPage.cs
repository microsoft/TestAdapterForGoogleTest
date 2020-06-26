// This file has been modified by Microsoft on 6/2020.

using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace GoogleTestAdapter.VsPackage
{
    public interface IGoogleTestExtensionOptionsPage : IServiceProvider
    {
        bool CatchExtensions { get; set; }
        bool BreakOnFailure { get; set; }
        bool ParallelTestExecution { get; set; }
        bool PrintTestOutput { get; set; }
        IVsActivityLog GetActivityLog();
    }
}