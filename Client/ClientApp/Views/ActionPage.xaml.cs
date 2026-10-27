using ClientApp.ViewModels;

namespace ClientApp.Views
{
    // Сторінка підтвердження дії бронювання (Action)
    public partial class ActionPage : ContentPage
    {
        public ActionPage(ActionViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
