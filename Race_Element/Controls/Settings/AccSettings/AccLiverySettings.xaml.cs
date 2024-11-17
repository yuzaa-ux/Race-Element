using RaceElement.Data.ACC.Config;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RaceElement.Controls;

/// <summary>
/// Interaction logic for AccLiverySettings.xaml
/// </summary>
public partial class AccLiverySettings : UserControl
{
    private readonly MenuSettingsService menu = new();

    public AccLiverySettings()
    {
        InitializeComponent();
        this.Loaded += (s, e) => LoadSettings();
        buttonResetLiverySettings.Click += (s, e) =>
        {
            menu.ResetLiverySettings();
            LoadSettings();
        };

        AddToggleListener(toggleTexDDS);
        AddToggleListener(toggleTexCap);
    }

    private void AddToggleListener(ToggleButton button)
    {
        button.Checked += (s, e) => SaveSettings(s, e);
        button.Unchecked += (s, e) => SaveSettings(s, e);
    }

    private void LoadSettings()
    {
        var settings = menu.Settings().Get(false);

        toggleTexDDS.IsChecked = settings.TexDDS == 1;
        toggleTexCap.IsChecked = settings.TexCap == 0;
    }

    private void SaveSettings(object sender, RoutedEventArgs e)
    {
        var settings = menu.Settings().Get(false);

        if (e.Source is ToggleButton toggle)
        {
            if (toggle == toggleTexCap)
                settings.TexCap = toggle.IsChecked.Value ? 0 : 1;

            if (toggle == toggleTexDDS)
                settings.TexDDS = toggle.IsChecked.Value ? 1 : 0;
        }

        menu.Settings().Save(settings);
    }
}
