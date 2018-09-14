using System.Windows;
using Caliburn.Micro;
using SkidScanner.ViewModels;


namespace SkidScanner
{
	public class Bootstrapper : BootstrapperBase
	{
		public Bootstrapper()
		{
			Initialize();
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			
			DisplayRootViewFor<ShellViewModel>();
		}
	}
}
