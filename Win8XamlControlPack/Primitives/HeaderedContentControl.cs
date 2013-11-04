using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Win8XamlControlPack.Primitives
{
	[StyleTypedProperty(Property="FocusVisualStyle", StyleTargetType=typeof(Control))]
	[TemplateVisualState(Name="Disabled", GroupName="CommonStates")]
	[TemplateVisualState(Name="Focused", GroupName="FocusStates")]
	[TemplateVisualState(Name="Normal", GroupName="CommonStates")]
	[TemplateVisualState(Name="Unfocused", GroupName="FocusStates")]
	public class HeaderedContentControl : ContentControl
	{
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(HeaderedContentControl), new PropertyMetadata(null, OnHeaderChanged));

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(HeaderedContentControl), new PropertyMetadata(null, OnHeaderTemplateChanged));

        public static readonly DependencyProperty FocusVisualStyleProperty =
            DependencyProperty.Register("FocusVisualStyle", typeof(Style), typeof(HeaderedContentControl), new PropertyMetadata(null));

        public static readonly DependencyProperty HeaderTemplateSelectorProperty =
            DependencyProperty.Register("HeaderTemplateSelector", typeof(DataTemplateSelector), typeof(HeaderedContentControl), new PropertyMetadata(null));

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register("IsFocused", typeof(bool), typeof(HeaderedContentControl), new PropertyMetadata(false, OnIsFocusedChanged));

        public static readonly DependencyProperty HasHeaderProperty =
            DependencyProperty.Register("HasHeader", typeof(bool), typeof(HeaderedContentControl), new PropertyMetadata(false, OnHeaderChanged));


        public Style FocusVisualStyle
		{
			get { return (Style)GetValue(FocusVisualStyleProperty); }
			set { SetValue(FocusVisualStyleProperty, value); }
		}

		public bool HasHeader
		{
			get { return (bool)GetValue(HasHeaderProperty); }
			private set { SetValue(HasHeaderProperty, value); }
		}

		public object Header
		{
			get { return GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		public DataTemplate HeaderTemplate
		{
			get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
			set { SetValue(HeaderTemplateProperty, value); }
		}

		public DataTemplateSelector HeaderTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty); }
			set { SetValue(HeaderTemplateSelectorProperty, value); }
		}

		public bool IsFocused
		{
			get { return (bool)GetValue(IsFocusedProperty); }
			protected set { SetValue(IsFocusedProperty, value); }
		}

		public HeaderedContentControl()
		{
			DefaultStyleKey = typeof(HeaderedContentControl);
			IsEnabledChanged += OnIsEnabledChanged;
		}

		protected internal virtual void ChangeVisualState()
		{
			ChangeVisualState(true);
		}

		protected internal virtual void ChangeVisualState(bool useTransitions)
		{
			if (!IsEnabled)
			{
				VisualStateManager.GoToState(this, "Disabled", useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, "Normal", useTransitions);
			}
			if (IsFocused)
			{
				VisualStateManager.GoToState(this, "Focused", useTransitions);
				return;
			}
			VisualStateManager.GoToState(this, "Unfocused", useTransitions);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			OnHeaderTemplateChanged(null, HeaderTemplate);
			OnHeaderChanged(null, Header);
			ChangeVisualState();
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			if (e.OriginalSource != this) return;
			IsFocused = true;
			base.OnGotFocus(e);
		}

		protected virtual void OnHeaderChanged(object oldHeader, object newHeader)
		{
		}

		private static void OnHeaderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var control = (HeaderedContentControl)sender;
			control.HasHeader = e.NewValue != null;
			control.OnHeaderChanged(e.OldValue, e.NewValue);
		}

		protected virtual void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate)
		{
		}

		private static void OnHeaderTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			((HeaderedContentControl)sender).OnHeaderTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
		}

		protected internal virtual void OnIsEnabledChanged(DependencyPropertyChangedEventArgs e)
		{
			ChangeVisualState();
		}

		private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			OnIsEnabledChanged(e);
		}

		protected virtual void OnIsFocusedChanged(DependencyPropertyChangedEventArgs e)
		{
			ChangeVisualState();
		}

		private static void OnIsFocusedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			(sender as HeaderedContentControl).OnIsFocusedChanged(e);
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			IsFocused = false;
			base.OnLostFocus(e);
		}
	}
}