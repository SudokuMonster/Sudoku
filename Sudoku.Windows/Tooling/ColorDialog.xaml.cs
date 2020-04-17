﻿using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sudoku.Drawing.Extensions;
using Sudoku.Windows.Extensions;

namespace Sudoku.Windows.Tooling
{
	/// <summary>
	/// Interaction logic for <c>ColorDialog.xaml</c>.
	/// </summary>
	public partial class ColorDialog : Window
	{
		/// <summary>
		/// The default brush.
		/// </summary>
		private readonly Brush _brush = new SolidBrush(Color.Black);


		/// <include file='../../GlobalDocComments.xml' path='comments/defaultConstructor'/>
		public ColorDialog() => InitializeComponent();


		/// <summary>
		/// Indicates the selected color.
		/// </summary>
		public Color SelectedColor { get; private set; }


		/// <inheritdoc/>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			SelectedColor = Color.FromArgb(255, Color.Black);

			UpdatePreview();
		}

		/// <summary>
		/// To update the preview.
		/// </summary>
		private void UpdatePreview()
		{
			var bitmap = new Bitmap((int)_imagePreview.Width, (int)_imagePreview.Height);
			using var g = Graphics.FromImage(bitmap);
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			g.DrawString(
				"sudoku", new Font("Times New Roman", 16F), _brush,
				bitmap.Width >> 1, bitmap.Height >> 1, new StringFormat
				{
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Center
				});
			g.Clear(SelectedColor);

			_imagePreview.Source = bitmap.ToImageSource();

			GC.Collect();
		}

		private void ButtonApply_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;

			_brush.Dispose();
			Close();
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;

			_brush.Dispose();
			Close();
		}

		private void TextBoxA_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (!byte.TryParse(textBox.Text, out byte value))
				{
					e.Handled = true;
					return;
				}

				SelectedColor = Color.FromArgb(value, SelectedColor);
				UpdatePreview();
			}
		}

		private void TextBoxR_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (!byte.TryParse(textBox.Text, out byte value))
				{
					e.Handled = true;
					return;
				}

				SelectedColor = Color.FromArgb(SelectedColor.A, value, SelectedColor.G, SelectedColor.B);
				UpdatePreview();
			}
		}

		private void TextBoxG_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (!byte.TryParse(textBox.Text, out byte value))
				{
					e.Handled = true;
					return;
				}

				SelectedColor = Color.FromArgb(SelectedColor.A, SelectedColor.R, value, SelectedColor.B);
				UpdatePreview();
			}
		}

		private void TextBoxB_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (!byte.TryParse(textBox.Text, out byte value))
				{
					e.Handled = true;
					return;
				}

				SelectedColor = Color.FromArgb(SelectedColor.A, SelectedColor.R, SelectedColor.G, value);
				UpdatePreview();
			}
		}
	}
}