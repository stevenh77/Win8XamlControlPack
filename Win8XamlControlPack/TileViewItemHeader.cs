using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Win8XamlControlPack
{
    public class TileViewItemHeader : ContentControl
    {
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(TileViewItemHeader), new PropertyMetadata(null, OnHeaderTemplateChanged));

        public static readonly DependencyProperty TileStateProperty =
            DependencyProperty.Register("TileState", typeof(TileViewItemState), typeof(TileViewItemHeader), new PropertyMetadata(TileViewItemState.Restored));

        internal UIElement GripBar { get; set; }

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        internal TileViewItem ParentItem { get; set; }

        public TileViewItemState TileState
        {
            get { return (TileViewItemState)GetValue(TileStateProperty); }
            set { SetValue(TileStateProperty, value); }
        }

        public TileViewItemHeader()
        {
            base.DefaultStyleKey = typeof(TileViewItemHeader);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            GripBar = GetTemplateChild("GripBarElement") as UIElement;
            if (ParentItem != null)
            {
                ParentItem.OnHeaderPartLoaded();
            }
            UpdateTileStates();
        }

        private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var headerItem = d as TileViewItemHeader;
            if (headerItem == null) return;
            headerItem.ContentTemplate = e.NewValue as DataTemplate;
        }

        internal void UpdateTileStates()
        {
            if (TileState == TileViewItemState.Maximized)
            {
                VisualStateManager.GoToState(this, "Maximized", false);
                return;
            }
            VisualStateManager.GoToState(this, "Restored", false);
        }
    }
}