using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.Util.SystemExtensions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls;

internal sealed class IntegerValueControl : IValueControl<int>, IControl
{
    private readonly Grid _grid;

    private readonly Grid _labelSpaceGrid;
    private readonly Label _label;
    private readonly TextBox _labelTextBox;

    private readonly Slider _slider;

    private readonly IntRangeAttribute _intRange;

    public FrameworkElement Control => _grid;
    public int Value { get; set; }
    private readonly ConfigField _field;

    public IntegerValueControl(IntRangeAttribute intRange, ConfigField configField)
    {
        _intRange = intRange;
        _field = configField;
        _grid = new Grid()
        {
            Width = ControlConstants.ControlWidth,
            Margin = new Thickness(0, 0, 7, 0),
            Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
            Cursor = Cursors.Hand
        };
        _grid.MouseEnter += OnGridMouseEnter;
        _grid.MouseLeave += OnGridMouseLeave;
        _grid.PreviewMouseLeftButtonUp += (s, e) => Save();

        _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
        _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10, GridUnitType.Star) });

        _labelSpaceGrid = new();

        _grid.Children.Add(_labelSpaceGrid);
        Grid.SetColumn(_labelSpaceGrid, 0);

        _label = new Label()
        {
            Content = _field.Value,
            FontWeight = FontWeights.Bold,
            FontSize = 13,
        };

        _label.HorizontalContentAlignment = HorizontalAlignment.Right;
        _labelSpaceGrid.Children.Add(_label);

        _labelTextBox = new TextBox()
        {
            Visibility = Visibility.Collapsed,
            Margin = new(0, 0, 0, -1),
            TextAlignment = TextAlignment.Right,
            Text = $"{_field.Value}",
        };
        _labelSpaceGrid.Children.Add(_labelTextBox);

        _slider = new Slider()
        {
            Minimum = intRange.GetMin(GameManager.CurrentGame),
            Maximum = intRange.GetMax(GameManager.CurrentGame),
            TickFrequency = intRange.Increment,
            IsSnapToTickEnabled = true,
            Width = 220,
        };
        _slider.ValueChanged += (s, e) =>
        {
            _field.Value = _slider.Value.ToString();
            UpdateLabels();
        };
        int value = int.Parse(_field.Value.ToString());
        value.Clip(intRange.GetMin(GameManager.CurrentGame), intRange.GetMax(GameManager.CurrentGame));
        _slider.Value = value;
        _grid.Children.Add(_slider);
        _slider.HorizontalAlignment = HorizontalAlignment.Right;
        _slider.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_slider, 1);

        _label.Content = _field.Value;
        Control.MouseWheel += (sender, args) =>
        {
            int delta = args.Delta;
            _slider.Value += delta.Clip(-1, 1) * intRange.Increment;
            args.Handled = true;
            Save();
        };
    }

    private void OnGridMouseEnter(object sender, MouseEventArgs e)
    {
        _label.Visibility = Visibility.Collapsed;
        _labelTextBox.Visibility = Visibility.Visible;
        _labelTextBox.SelectAll();
        UpdateLabels();
    }

    private void OnGridMouseLeave(object sender, MouseEventArgs e)
    {
        _label.Visibility = Visibility.Visible;
        _labelTextBox.Visibility = Visibility.Collapsed;

        if (TryGetTextBoxValue(out int value))
        {
            _field.Value = value;

            UpdateLabels();

            _slider.Value = value;
            Save();
        }
    }

    /// <summary>
    /// Checks whether the textbox string after successful parsing is within the provided Integer range and tries to match it with the provided <see cref="_intRange"/>.
    /// if no match was found, 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool TryGetTextBoxValue(out int value)
    {
        string content = _labelTextBox.Text;
        value = 0;
        if (string.IsNullOrEmpty(content)) return false;

        content = content.Trim();
        if (int.TryParse(content, out int result))
        {
            // validate if it fits in the integer range;
            int min = _intRange.GetMin(GameManager.CurrentGame);
            int max = _intRange.GetMax(GameManager.CurrentGame);
            int steps = _intRange.Increment;

            // try to match any of the steps
            for (int i = min; i <= max; i += steps)
                if (result == i)
                {
                    value = result;
                    return true;
                }

            // clip the result and match it to any of the existing steps
            result.Clip(min, max);
            for (int i = min; i <= max; i += steps)
                if (result < i + steps / 2)
                {
                    value = i;
                    return true;
                }
        }

        return false;
    }

    private void UpdateLabels()
    {
        _label.Content = _field.Value;
        if (_labelTextBox.IsVisible)
            _labelTextBox.Text = $"{_field.Value}";
    }

    public void Save()
    {
        ConfigurationControls.SaveOverlayConfigField(_field);
    }

}
