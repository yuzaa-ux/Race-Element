using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.Util.SystemExtensions;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls;

internal sealed class FloatValueControl : IValueControl<float>
{
    private readonly Grid _grid;

    private readonly Grid _labelSpaceGrid;
    private readonly Label _label;
    private readonly TextBox _labelTextBox;

    private readonly Slider _slider;

    private readonly FloatRangeAttribute _floatRange;

    public FrameworkElement Control => _grid;
    public float Value { get; set; }
    private readonly ConfigField _field;

    public FloatValueControl(FloatRangeAttribute floatRange, ConfigField configField)
    {
        _floatRange = floatRange;
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
            ContextMenu = null,
        };
        _labelTextBox.KeyUp += OnLabelTextBoxKeyUp;
        _labelSpaceGrid.Children.Add(_labelTextBox);


        _slider = new Slider()
        {
            Minimum = floatRange.GetMin(GameManager.CurrentGame),
            Maximum = floatRange.GetMax(GameManager.CurrentGame),
            TickFrequency = floatRange.Increment,
            IsSnapToTickEnabled = true,
            Width = 220
        };
        _slider.PreviewKeyDown += OnSliderKeyUp;
        _slider.ValueChanged += (s, e) =>
        {
            _field.Value = _slider.Value.ToString($"F{floatRange.Decimals}");
            UpdateLabels(floatRange.Decimals);
        };

        float value = float.Parse(configField.Value.ToString());
        value.Clip(floatRange.GetMin(GameManager.CurrentGame), floatRange.GetMax(GameManager.CurrentGame));
        _slider.Value = value;

        _grid.Children.Add(_slider);
        _slider.HorizontalAlignment = HorizontalAlignment.Right;
        _slider.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_slider, 1);

        Control.MouseWheel += (sender, args) =>
        {
            int delta = args.Delta;
            _slider.Value += delta.Clip(-1, 1) * floatRange.Increment;
            args.Handled = true;
            Save();
        };

        UpdateLabels(floatRange.Decimals);
    }

    private void OnGridMouseEnter(object sender, MouseEventArgs e)
    {
        _label.Visibility = Visibility.Collapsed;
        _labelTextBox.Visibility = Visibility.Visible;

        UpdateLabels(_floatRange.Decimals);
    }

    private void OnGridMouseLeave(object sender, MouseEventArgs e)
    {
        _label.Visibility = Visibility.Visible;
        _labelTextBox.Visibility = Visibility.Collapsed;

        if (TryGetTextBoxValue(out float value))
        {
            _field.Value = value.ToString($"F{_floatRange.Decimals}");

            UpdateLabels(_floatRange.Decimals);

            _slider.Value = value;
            Save();
        }
    }

    private void OnSliderKeyUp(object sender, KeyEventArgs e)
    {

        switch (e.Key)
        {
            case Key.Down:
            case Key.Left:
                {
                    _slider.Value -= _floatRange.Increment;
                    e.Handled = true;
                    Save();
                    UpdateLabels(_floatRange.Decimals);
                    break;
                }
            case Key.Up:
            case Key.Right:
                {
                    _slider.Value += _floatRange.Increment;
                    e.Handled = true;
                    Save();
                    UpdateLabels(_floatRange.Decimals);
                    break;
                }
        }
    }

    private void OnLabelTextBoxKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (TryGetTextBoxValue(out float value))
        {
            _field.Value = value.ToString($"F{_floatRange.Decimals}");

            UpdateLabels(_floatRange.Decimals);

            _slider.Value = value;
            Save();
        }
    }

    /// <summary>
    /// Checks whether the textbox string after successful parsing is within the provided Integer range and tries to match it with the provided <see cref="_floatRange"/>.
    /// if no match was found, 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool TryGetTextBoxValue(out float value)
    {
        string content = _labelTextBox.Text;
        value = 0;
        if (string.IsNullOrEmpty(content)) return false;

        content = content.Trim();
        if (float.TryParse(content, out float result))
        {
            // validate if it fits in the integer range;
            float min = _floatRange.GetMin(GameManager.CurrentGame);
            float max = _floatRange.GetMax(GameManager.CurrentGame);
            float steps = _floatRange.Increment;

            // try to match any of the steps
            for (float i = min; i <= max; i += steps)
                if (result > i - steps / 3f && result < i + steps / 3f)
                {
                    value = result;
                    return true;
                }

            // clip the result and match it to any of the existing steps
            result.Clip(min, max);
            for (float i = min; i <= max + steps / 3; i += steps)
                if (result > i - steps / 2f && result < i + steps / 2f)
                {
                    value = i;
                    return true;
                }
        }

        return false;
    }

    private void UpdateLabels(int decimals)
    {
        _label.Content = $"{_slider.Value.ToString($"F{decimals}")}";
        if (_labelTextBox.IsVisible)
            _labelTextBox.Text = $"{_slider.Value.ToString($"F{decimals}")}";
    }

    public void Save()
    {
        ConfigurationControls.SaveOverlayConfigField(_field);
    }
}
