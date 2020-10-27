using Client.Enums;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client.Windows
{
	/// <summary>
	/// Logica di interazione per AlertWindows.xaml
	/// </summary>
	public partial class AlertWindows : Window
	{
		public AlertWindows()
		{
			InitializeComponent();
		}
		public AlertWindows(string message)
		{
			InitializeComponent();
			this.Message.Content = message;
			this.Show();
		}
		public AlertWindows(string message,AlertType alertType)
		{
			InitializeComponent();
			this.Message.Content = message;

			switch (alertType)
			{
				case AlertType.Success:
					Icon.Kind = PackIconFontAwesomeKind.CheckCircleSolid;
					break;
				case AlertType.Info:
					Icon.Kind = PackIconFontAwesomeKind.QuestionCircleRegular;
					break;
				case AlertType.Error:
					Icon.Kind = PackIconFontAwesomeKind.TimesCircleSolid;
					break;

			}

			this.Show();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
