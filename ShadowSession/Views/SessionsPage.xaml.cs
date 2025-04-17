using ShadowSession.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShadowSession.Views
{
    /// <summary>
    /// Interaction logic for SessionsPage.xaml
    /// </summary>
    public partial class SessionsPage : UserControl, IView
    {
        public SessionsPage()
        {
            DataContext = new SessionsPageViewModel();
            InitializeComponent();
        }

        public void Refresh()
        {
            ((SessionsPageViewModel)DataContext).RefreshSessionsCommand.Execute(null);
        }

        private void OnSessionsDataGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                ((DataGrid)sender).UnselectAll();
            }
        }
    }
}
