﻿using Client.Messaging;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Windows
{
	/// <summary>
	/// Logica di interazione per MainWindow.xaml
	/// </summary>
	/// 

	public partial class MainWindow : MetroWindow
	{


		public MainWindow()
		{
				InitializeComponent();
				Panel.Navigate(new Uri("Pages/Main.xaml", UriKind.Relative));

			Messenger.Default.Register<ShowMessage>(this, m =>
			{				
				MessageBox.Show(m.Text);
			});
		}




	}
}