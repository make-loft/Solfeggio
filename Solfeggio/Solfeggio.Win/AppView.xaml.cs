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
			new MonitorView().To(out var optionsView).Show();

			Closed += (o, e) =>
			{
				optionsView.Close();
				Store.Snapshot();
				Environment.Exit(0);
			};
		}
	}
}
