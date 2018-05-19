
namespace GoogleTestAdapter.Settings
{
    public interface ICTestPropertySettingsContainer
    {
        bool TryGetSettings(string testName, out ICTestPropertySettings settings);
    }
}
