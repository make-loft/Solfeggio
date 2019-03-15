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
			var optionsView = new Window { Content = new OptionsView(), Width = 400, Height = 300 };
			optionsView.Show();
			Closed += (o, e) =>
			{
				optionsView.Close();
				Store.Snapshot();
				Environment.Exit(0);
			};
		}
	}
}
