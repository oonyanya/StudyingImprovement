using StudyingImprovement.Model;

namespace StudyingImprovement;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();
		BindingContext = Setting.Current;
	}
}