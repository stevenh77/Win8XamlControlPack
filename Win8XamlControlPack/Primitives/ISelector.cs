using Windows.UI.Xaml.Controls;

namespace Win8XamlControlPack.Primitives
{
    public interface ISelector
    {
        int SelectedIndex { get; }

        object SelectedItem { get; }

        object SelectedValue { get; }

        event SelectionChangedEventHandler SelectionChanged;
    }
}
