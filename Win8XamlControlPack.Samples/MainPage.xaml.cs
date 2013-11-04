using Win8XamlControlPack.Samples.ViewModels;

namespace Win8XamlControlPack.Samples
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.tileView.ItemsSource = MyViewModel.GenerateItems();
        }
    }
}
