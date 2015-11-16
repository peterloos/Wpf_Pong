using System;
using System.Windows;

namespace Wpf_Pong
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            if (sender == this.ButtonStart)
            {
                this.PongCanvas.Start();
            }
            else if (sender == this.ButtonStop)
            {
                this.PongCanvas.Stop();
            }
        }
    }
}
