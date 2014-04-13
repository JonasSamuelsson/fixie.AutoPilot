using System.Windows;

namespace Fixie.AutoRun
{
   /// <summary>
   /// Interaction logic for AppWindow.xaml
   /// </summary>
   public partial class AppWindow
   {
      public AppWindow()
      {
         InitializeComponent();
      }

      private void AppWindow_OnLoaded(object sender, RoutedEventArgs e)
      {
         ((AppWindowViewModel)DataContext).Run();
      }
   }
}
