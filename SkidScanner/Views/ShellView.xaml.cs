using System.Windows;
using System.Windows.Media;
using SkidScanner.Models;
using SkidScanner.ViewModels;

namespace SkidScanner.Views
{
	/// <summary>
	/// Interaction logic for ShellView.xaml
	/// </summary>
	public partial class ShellView : Window
	{
		public ShellView()
		{
			InitializeComponent();

		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var t = (ShellViewModel)DataContext;
			t.Main();
			tb.Focus();
			
		}

		private void Window_Unloaded(object sender, RoutedEventArgs e)
		{
			var t = (ShellViewModel)DataContext;
			t.Disconnect();
		}
	}
}
