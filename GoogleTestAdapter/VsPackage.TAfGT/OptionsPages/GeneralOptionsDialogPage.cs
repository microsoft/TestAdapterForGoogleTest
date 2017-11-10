// This file has been modified by Microsoft on 11/2017.

using GoogleTestAdapter.Settings;

namespace GoogleTestAdapter.VsPackage.OptionsPages
{

    public partial class GeneralOptionsDialogPage : NotifyingDialogPage
    {
        [LocalizedCategory("CategoryMiscName")]
        [LocalizedDisplayName("OptionReportNewProjectTelemetry")]
        [LocalizedDescription("OptionReportNewProjectTelemetryDescription")]
        public bool ReportNewProjectTelemetry
        {
            get { return _reportNewProjectTelemetry; }
            set { SetAndNotify(ref _reportNewProjectTelemetry, value); }
        }
        private bool _reportNewProjectTelemetry = SettingsWrapper.OptionReportNewProjectTelemetryDefaultValue;
    }

}