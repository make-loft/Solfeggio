using System;
using System.Windows;
using Ace;
using Solfeggio.Views;

namespace Solfeggio
{
	public partial class AppView
	{
		public AppView()
		{
			InitializeComponent();
			new Window
			{
				Width = 800,
				Height = 300,
				Content = new OptionsView(),
				ResizeMode = ResizeMode.CanResizeWithGrip
			}.To(out var optionsView).Show();

			Closed += (o, e) =>
			{
				optionsView.Close();
				Store.Snapshot();
				Environment.Exit(0);
			};
		}
	}
}
