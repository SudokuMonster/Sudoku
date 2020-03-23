﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Sudoku.Forms
{
	/// <summary>
	/// Interaction logic for <c>AboutMeWindow.xaml</c>.
	/// </summary>
	public partial class AboutMeWindow : Window
	{
		public AboutMeWindow() => InitializeComponent();


		private void GitHubLink_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Hyperlink textBlock)
			{
				try
				{
					Process.Start(textBlock.NavigateUri.AbsoluteUri);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Warning");
				}
			}
		}
	}
}