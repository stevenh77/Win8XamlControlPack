using Windows.UI.Xaml;

namespace Win8XamlControlPack.Primitives
{
    internal interface ISelectable
    {
        bool IsSelected { get; set; }

        void OnSelected(RoutedEventArgs e);

        void OnUnselected(RoutedEventArgs e);
    }
}
