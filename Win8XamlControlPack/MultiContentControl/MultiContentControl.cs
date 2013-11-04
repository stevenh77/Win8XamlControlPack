using System;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Win8XamlControlPack
{
    /// <summary>
    /// The MultiContentControl has three content properties which are displayed depending on the available space.
    /// Using the adjustable threshold properties, the exact points at which the visible content is switched can be
    /// precisely controlled.
    /// </summary>	
    public class MultiContentControl : ContentControl
    {
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(MultiContentControlState), typeof(MultiContentControl), new PropertyMetadata(MultiContentControlState.Large, OnStateChanged));

        public static readonly DependencyProperty SmallContentProperty =
            DependencyProperty.Register("SmallContent", typeof(object), typeof(MultiContentControl), new PropertyMetadata(null, OnAnyContentPropertyChanged));

        public static readonly DependencyProperty SmallContentTemplateProperty =
            DependencyProperty.Register("SmallContentTemplate", typeof(DataTemplate), typeof(MultiContentControl), new PropertyMetadata(null));

        public static readonly DependencyProperty LargeContentProperty =
            DependencyProperty.Register("LargeContent", typeof(object), typeof(MultiContentControl), new PropertyMetadata(null, OnAnyContentPropertyChanged));

        public static readonly DependencyProperty LargeContentTemplateProperty =
            DependencyProperty.Register("LargeContentTemplate", typeof(DataTemplate), typeof(MultiContentControl), new PropertyMetadata(null));

        public static readonly DependencyProperty NormalToSmallThresholdProperty =
            DependencyProperty.Register("NormalToSmallThreshold", typeof(Size), typeof(MultiContentControl), new PropertyMetadata(new Size(150, 150)));

        public static readonly DependencyProperty NormalToLargeThresholdProperty =
            DependencyProperty.Register("NormalToSmallThreshold", typeof(Size), typeof(MultiContentControl), new PropertyMetadata(new Size(150, 150)));

        public static readonly DependencyProperty ContentChangeModeProperty =
            DependencyProperty.Register("ContentChangeMode", typeof(ContentChangeMode), typeof(MultiContentControl), null);

        public static readonly DependencyProperty VisibleContentTemplateProperty =
            DependencyProperty.Register("VisibleContentTemplate", typeof(DataTemplate), typeof(MultiContentControl), null);

        public static readonly DependencyProperty VisibleContentProperty =
            DependencyProperty.Register("VisibleContent", typeof(object), typeof(MultiContentControl), null); 
        
        public ContentChangeMode ContentChangeMode
        {
            get { return (ContentChangeMode) GetValue(ContentChangeModeProperty); }
            set { SetValue(ContentChangeModeProperty, value); }
        }

        public object LargeContent
        {
            get { return GetValue(LargeContentProperty); }
            set { SetValue(LargeContentProperty, value); }
        }

        public DataTemplate LargeContentTemplate
        {
            get { return (DataTemplate) GetValue(LargeContentTemplateProperty); }
            set { SetValue(LargeContentTemplateProperty, value); }
        }

        public Size NormalToLargeThreshold
        {
            get { return (Size) GetValue(NormalToLargeThresholdProperty); }
            set { SetValue(NormalToLargeThresholdProperty, value); }
        }

        public Size NormalToSmallThreshold
        {
            get { return (Size) GetValue(NormalToSmallThresholdProperty); }
            set { SetValue(NormalToSmallThresholdProperty, value); }
        }

        public object SmallContent
        {
            get { return GetValue(SmallContentProperty); }
            set { SetValue(SmallContentProperty, value); }
        }

        public DataTemplate SmallContentTemplate
        {
            get { return (DataTemplate) GetValue(SmallContentTemplateProperty); }
            set { SetValue(SmallContentTemplateProperty, value); }
        }

        public MultiContentControlState State
        {
            get { return (MultiContentControlState) GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public object VisibleContent
        {
            get { return GetValue(VisibleContentProperty); }
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DataTemplate VisibleContentTemplate
        {
            get { return (DataTemplate) GetValue(VisibleContentTemplateProperty); }
            set { SetValue(VisibleContentTemplateProperty, value); }
        }
        
        public MultiContentControl()
        {
            DefaultStyleKey = typeof (MultiContentControl);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ContentChangeMode == ContentChangeMode.Automatic)
            {
                if (NormalToLargeThreshold.Width <= availableSize.Width && NormalToLargeThreshold.Height <= availableSize.Height)
                {
                    State = MultiContentControlState.Large;
                }
                else if (NormalToSmallThreshold.Width < availableSize.Width || NormalToSmallThreshold.Height < availableSize.Height)
                {
                    State = MultiContentControlState.Normal;
                }
                else
                {
                    State = MultiContentControlState.Small;
                }
            }
            var size = base.MeasureOverride(availableSize);
            if (!double.IsInfinity(availableSize.Width) && !double.IsInfinity(availableSize.Height) && !double.IsNaN(availableSize.Width) && !double.IsNaN(availableSize.Height))
            {
                return availableSize;
            }
            return size;
        }

        private static void OnAnyContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = (MultiContentControl) sender;
            if (thisControl != null)
            {
                thisControl.UpdateVisibleContent();
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisibleContent();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            UpdateVisibleContent();
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (MultiContentControl) d;
            if (sender == null) return;
            sender.UpdateVisibleContent();
            if (sender.StateChanged == null) return;
            sender.StateChanged(sender, new MultiContentControlStateChangedEventArgs((MultiContentControlState) e.NewValue, (MultiContentControlState) e.OldValue));
        }

        internal void UpdateVisibleContent()
        {
            switch (State)
            {
                case MultiContentControlState.Small:
                {
                    this.SetValue(MultiContentControl.VisibleContentProperty, SmallContent);
                    VisibleContentTemplate = SmallContentTemplate;
                    return;
                }
                case MultiContentControlState.Normal:
                {
                    this.SetValue(MultiContentControl.VisibleContentProperty, Content);
                    VisibleContentTemplate = ContentTemplate;
                    return;
                }
                case MultiContentControlState.Large:
                {
                    this.SetValue(MultiContentControl.VisibleContentProperty, LargeContent);
                    VisibleContentTemplate = LargeContentTemplate;
                    return;
                }
                default:
                {
                    return;
                }
            }
        }

        public event EventHandler<MultiContentControlStateChangedEventArgs> StateChanged;
    }
}