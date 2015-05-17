using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using System.Diagnostics;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JukeBox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1 : Page
    {

        Cloudtools cloudtools = new Cloudtools();

        public BlankPage1()
        {
            this.InitializeComponent();
        }

        private void submitUserName(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Name:("+ userName.Text +")");
            cloudtools.GenerateClientID(userName.Text);
            this.Frame.Navigate(typeof(MainPage));
        }
        
        
    }
}

