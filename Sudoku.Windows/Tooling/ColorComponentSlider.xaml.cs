﻿using System.Windows;
using System.Windows.Controls;

namespace Sudoku.Windows.Tooling
{
	public delegate void ValueChangedEventHandler(double value);

	/// <summary>
	/// Interaction logic for <c>ColorComponentSlider.xaml</c>.
	/// </summary>
	public partial class ColorComponentSlider : UserControl
	{

		protected bool _updatingValues = false;


		/// <include file='../../../GlobalDocComments.xml' path='comments/defaultConstructor'/>
		public ColorComponentSlider() => InitializeComponent();


		/// <summary>
		/// The format string.
		/// </summary>
		public string FormatString { get; set; } = "F2";


		/// <summary>
		/// The event triggering while value changed.
		/// </summary>
		public event ValueChangedEventHandler? ValueChanged;


		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			double value = _slider.Value;
			if (!_updatingValues)
			{
				_updatingValues = true;
				_textBox.Text = value.ToString(FormatString);
				ValueChanged?.Invoke(value);
				_updatingValues = false;
			}
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!_updatingValues && double.TryParse(_textBox.Text, out double parsedValue))
			{
				_updatingValues = true;
				_slider.Value = parsedValue;
				ValueChanged?.Invoke(parsedValue);
				_updatingValues = false;
			}
		}
	}
}
