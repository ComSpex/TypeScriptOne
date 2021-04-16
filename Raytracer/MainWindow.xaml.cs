using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Raytracer {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow:Window {
		public MainWindow() {
			InitializeComponent();
			this.mainG.Children.Clear();
			try {
				RayTracer rt=new RayTracer();
				try {
					DefaultScene ds=new DefaultScene();
					this.mainG.Children.Add(rt.render(ds,512,512));
				} catch(Exception ex) {
					MessageBox.Show(ex.ToString());
				}
			} catch(Exception ex) {
				MessageBox.Show(ex.ToString());
			}
		}
	}
}
