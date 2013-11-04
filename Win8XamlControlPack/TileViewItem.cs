using System;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Win8XamlControlPack.Primitives;

namespace Win8XamlControlPack
{
	public class TileViewItem : HeaderedContentControl, ISelectable
	{
		public readonly static DependencyProperty MinimizedHeightProperty;
		public readonly static DependencyProperty MinimizedWidthProperty;
		public readonly static DependencyProperty PositionProperty;
		public readonly static DependencyProperty TileStateProperty;
		public readonly static DependencyProperty RestoredWidthProperty;
		public readonly static DependencyProperty HeaderStyleProperty;
		public readonly static DependencyProperty RestoredHeightProperty;
		public readonly static DependencyProperty IsMouseOverDraggingProperty;
		public readonly static DependencyProperty IsSelectedProperty;
		private readonly static TimeSpan doubleClickDelta;
		private Border outerContainer;
		private UIElement gripBar;
		private DateTime lastGripBarClickTime;
		private bool cancelTileStateChanged;
		private bool isApplyTemplateFinished;
        private TileViewItemHeader headerPart;
        //public readonly static Telerik.Windows.RoutedEvent TileStateChangedEvent;
        //public readonly static Telerik.Windows.RoutedEvent PreviewTileStateChangedEvent;
        //public readonly static Telerik.Windows.RoutedEvent PreviewSelectionChangedEvent;
        //public readonly static Telerik.Windows.RoutedEvent SelectionChangedEvent;
        //public readonly static Telerik.Windows.RoutedEvent PositionChangedEvent;
        //public readonly static Telerik.Windows.RoutedEvent PreviewPositionChangedEvent;

		internal bool CancelPositionChanged { get; set; }
		public Style HeaderStyle
		{
			get { return (Style)GetValue(HeaderStyleProperty); }
			set { SetValue(HeaderStyleProperty, value); }
		}

		public bool IsMouseOverDragging
		{
			get { return (bool)GetValue(IsMouseOverDraggingProperty); }
			set { SetValue(IsMouseOverDraggingProperty, value); }
		}

		internal bool IsPositionReverted { get; set; }

		public bool IsSelected
		{
			get { return (bool)GetValue(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}

		internal Panel LayoutRoot { get; set; }

		internal static int MaxZIndex { get; set; }

		public double MinimizedHeight
		{
			get { return (double)GetValue(MinimizedHeightProperty); }
			set { SetValue(MinimizedHeightProperty, value); }
		}

		public double MinimizedWidth
		{
			get { return (double)GetValue(MinimizedWidthProperty); }
			set { SetValue(MinimizedWidthProperty, value); }
		}

		internal TranslateTransform MoveTransform
		{
			get
			{
				if (LayoutRoot == null)
				{
					return null;
				}
				return EnsureTransform(LayoutRoot).Children[3] as TranslateTransform;
			}
		}

		public TileView ParentTileView { get; internal set; }

		public int Position
		{
			get { return (int)GetValue(PositionProperty); }
			set { SetValue(PositionProperty, value); }
		}

		internal int PreviousPosition { get; set; }

		public double RestoredHeight
		{
			get { return (double)GetValue(RestoredHeightProperty); }
			set { SetValue(RestoredHeightProperty, value); }
		}

		public double RestoredWidth
		{
			get { return (double)GetValue(RestoredWidthProperty);}
			set { SetValue(RestoredWidthProperty, value); }
		}

		public TileViewItemState TileState
		{
			get
			{
				return (TileViewItemState)base.GetValue(RadTileViewItem.TileStateProperty);
			}
			set
			{
				base.SetValue(RadTileViewItem.TileStateProperty, value);
			}
		}

		static RadTileViewItem()
		{
			RadTileViewItem.MinimizedHeightProperty = DependencyProperty.Register("MinimizedHeight", typeof(double), typeof(RadTileViewItem), new System.Windows.PropertyMetadata((object)0, new PropertyChangedCallback(RadTileViewItem.OnPropertyChanged)));
			RadTileViewItem.MinimizedWidthProperty = DependencyProperty.Register("MinimizedWidth", typeof(double), typeof(RadTileViewItem), new System.Windows.PropertyMetadata((object)0, new PropertyChangedCallback(RadTileViewItem.OnPropertyChanged)));
			RadTileViewItem.PositionProperty = DependencyProperty.Register("Position", typeof(int), typeof(RadTileViewItem), new Telerik.Windows.PropertyMetadata((object)-1, new PropertyChangedCallback(RadTileViewItem.OnPositionPropertyChanged)));
			RadTileViewItem.TileStateProperty = DependencyProperty.Register("TileState", typeof(TileViewItemState), typeof(RadTileViewItem), new Telerik.Windows.PropertyMetadata((object)TileViewItemState.Restored, new PropertyChangedCallback(RadTileViewItem.OnTileStatePropertyChanged)));
			RadTileViewItem.RestoredWidthProperty = DependencyProperty.Register("RestoredWidth", typeof(double), typeof(RadTileViewItem), new System.Windows.PropertyMetadata((object)0, new PropertyChangedCallback(RadTileViewItem.OnPropertyChanged)));
			RadTileViewItem.HeaderStyleProperty = DependencyProperty.Register("HeaderStyle", typeof(System.Windows.Style), typeof(RadTileViewItem), new Telerik.Windows.PropertyMetadata(new PropertyChangedCallback(RadTileViewItem.OnHeaderStyleChanged)));
			RadTileViewItem.RestoredHeightProperty = DependencyProperty.Register("RestoredHeight", typeof(double), typeof(RadTileViewItem), new System.Windows.PropertyMetadata((object)0, new PropertyChangedCallback(RadTileViewItem.OnPropertyChanged)));
			RadTileViewItem.IsMouseOverDraggingProperty = DependencyProperty.Register("IsMouseOverDragging", typeof(bool), typeof(RadTileViewItem), new Telerik.Windows.PropertyMetadata(false, new PropertyChangedCallback(RadTileViewItem.OnIsMouseOverDraggingPropertyChanged)));
			RadTileViewItem.IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(RadTileViewItem), new Telerik.Windows.PropertyMetadata(false, new PropertyChangedCallback(RadTileViewItem.OnIsSelectedPropertyChanged)));
			RadTileViewItem.doubleClickDelta = TimeSpan.FromMilliseconds(300);
			RadTileViewItem.TileStateChangedEvent = EventManager.RegisterRoutedEvent("TileStateChanged", RoutingStrategy.Bubble, typeof(EventHandler<RadRoutedEventArgs>), typeof(RadTileViewItem));
			RadTileViewItem.PreviewTileStateChangedEvent = EventManager.RegisterRoutedEvent("PreviewTileStateChanged", RoutingStrategy.Tunnel, typeof(EventHandler<PreviewTileStateChangedEventArgs>), typeof(RadTileViewItem));
			RadTileViewItem.PreviewSelectionChangedEvent = EventManager.RegisterRoutedEvent("PreviewSelectionChanged", RoutingStrategy.Tunnel, typeof(Telerik.Windows.Controls.SelectionChangedEventHandler), typeof(RadTileViewItem));
			RadTileViewItem.SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(Telerik.Windows.Controls.SelectionChangedEventHandler), typeof(RadTileViewItem));
			RadTileViewItem.PositionChangedEvent = EventManager.RegisterRoutedEvent("PositionChanged", RoutingStrategy.Bubble, typeof(EventHandler<RadRoutedEventArgs>), typeof(RadTileViewItem));
			RadTileViewItem.PreviewPositionChangedEvent = EventManager.RegisterRoutedEvent("PreviewPositionChanged", RoutingStrategy.Bubble, typeof(EventHandler<RadRoutedEventArgs>), typeof(RadTileViewItem));
			CommandManager.RegisterClassCommandBinding(typeof(RadTileViewItem), new CommandBinding(TileViewCommands.ToggleTileState, new ExecutedRoutedEventHandler(RadTileViewItem.OnToggleStateChanged), new CanExecuteRoutedEventHandler(RadTileViewItem.OnCanToggleState)));
		}

		public TileViewItem()
		{
			DefaultStyleKey = typeof(TileViewItem);
			lastGripBarClickTime = new DateTime();
			PreviousPosition = -1;
		}

		private bool CanChangeTileState(TileViewItemState oldValue)
		{
			if (this.ParentTileView.MaximizeMode == TileViewMaximizeMode.Zero && this.TileState != TileViewItemState.Restored || oldValue == TileViewItemState.Restored && this.TileState == TileViewItemState.Minimized && this.ParentTileView.MaximizedItem == null || this.ParentTileView.MaximizeMode == TileViewMaximizeMode.One && this.TileState != TileViewItemState.Maximized && this.ParentTileView.MaximizedContainer == this)
			{
				return false;
			}
			if (this.ParentTileView.MaximizeMode != TileViewMaximizeMode.One || oldValue != TileViewItemState.Minimized)
			{
				return true;
			}
			return this.TileState != TileViewItemState.Restored;
		}

		private bool CanToggleState()
		{
			if (this.ParentTileView == null)
			{
				return false;
			}
			if (this.ParentTileView.MaximizeMode == TileViewMaximizeMode.One && this.TileState == TileViewItemState.Maximized)
			{
				return false;
			}
			return this.ParentTileView.MaximizeMode != TileViewMaximizeMode.Zero;
		}

		private static TransformGroup EnsureTransform(UIElement element)
		{
			TransformGroup group = element.RenderTransform as TransformGroup;
			if (group == null || group.Children.Count < 4 || !(group.Children[0] is ScaleTransform) || !(group.Children[1] is SkewTransform) || !(group.Children[2] is RotateTransform) || !(group.Children[3] is TranslateTransform))
			{
				group = new TransformGroup();
				group.Children.Add(new ScaleTransform());
				group.Children.Add(new SkewTransform());
				group.Children.Add(new RotateTransform());
				group.Children.Add(new TranslateTransform());
				element.RenderTransform = group;
			}
			return group;
		}

		private void GripBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.ParentTileView == null)
			{
				return;
			}
			this.ParentTileView.TileStateChangeCandidate = this;
			DateTime now = DateTime.Now;
			if (this.ParentTileView.TileStateChangeTrigger != TileStateChangeTrigger.DoubleClick || !((now - this.lastGripBarClickTime) <= RadTileViewItem.doubleClickDelta))
			{
				this.ParentTileView.TryStartDragging(this, e.GetPosition(null));
			}
			else
			{
				this.ToggleTileState();
			}
			this.lastGripBarClickTime = now;
		}

		internal void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (this.ParentTileView != null)
			{
				this.ParentTileView.DraggingCandidate = null;
				this.ParentTileView.IsClickOutside = false;
				if (this.ParentTileView.TileStateChangeTrigger == TileStateChangeTrigger.SingleClick)
				{
					this.OnGripBarMouseUp();
				}
			}
		}

		private void InitializeTemplateChildren()
		{
			if (this.gripBar != null)
			{
				this.gripBar.MouseLeftButtonDown -= new MouseButtonEventHandler(this.GripBar_MouseLeftButtonDown);
			}
			if (this.outerContainer != null)
			{
				this.outerContainer.MouseLeftButtonDown -= new MouseButtonEventHandler(this.OuterContainer_MouseLeftButtonDown);
			}
			this.headerPart = base.GetTemplateChild("HeaderPart") as TileViewItemHeader;
			if (this.headerPart != null)
			{
				if (this.gripBar != null && !this.gripBar.Equals(this.headerPart.GripBar))
				{
					this.gripBar = null;
				}
				this.headerPart.ParentItem = this;
				StyleManager.SetThemeFromParent(this.headerPart, this);
			}
			else
			{
				this.gripBar = base.GetTemplateChild("GripBarElement") as UIElement;
			}
			this.outerContainer = base.GetTemplateChild("outerContainer") as Border;
			this.LayoutRoot = this.FindChildByType<Panel>();
			if (this.gripBar != null)
			{
				this.gripBar.MouseLeftButtonDown += new MouseButtonEventHandler(this.GripBar_MouseLeftButtonDown);
			}
			if (this.outerContainer != null)
			{
				this.outerContainer.MouseLeftButtonDown += new MouseButtonEventHandler(this.OuterContainer_MouseLeftButtonDown);
			}
		}

		/// <summary>
		/// When overridden in a derived class, is invoked whenever application code or
		/// internal processes (such as a rebuilding layout pass) call
		/// <see cref="M:System.Windows.Controls.Control.ApplyTemplate" />.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this.InitializeTemplateChildren();
			base.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.HandleMouseLeftButtonUp), true);
			this.UpdateTileStates();
			this.UpdateSelectionStates();
			this.isApplyTemplateFinished = true;
		}

		private static void OnCanToggleState(object sender, CanExecuteRoutedEventArgs e)
		{
			RadTileViewItem tileViewItem = sender as RadTileViewItem;
			if (tileViewItem != null)
			{
				e.CanExecute = tileViewItem.CanToggleState();
			}
		}

		/// <summary>
		/// Returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" />
		/// implementations for the Windows Presentation Foundation (WPF) infrastructure.
		/// </summary>
		/// <returns>
		/// The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" />
		/// implementation.
		/// </returns>
		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new Telerik.Windows.Automation.Peers.RadTileViewItemAutomationPeer(this);
		}

		private void OnGripBarMouseUp()
		{
			if (this.ParentTileView != null && !this.ParentTileView.IsDragging && this.ParentTileView.TileStateChangeCandidate == this && this.ParentTileView.TileStateChangeTrigger == TileStateChangeTrigger.SingleClick)
			{
				this.ToggleTileState();
			}
		}

		internal void OnHeaderPartLoaded()
		{
			this.gripBar = this.headerPart.GripBar;
			if (this.gripBar != null)
			{
				this.gripBar.MouseLeftButtonDown += new MouseButtonEventHandler(this.GripBar_MouseLeftButtonDown);
			}
		}

		private static void OnHeaderStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RadTileViewItem item = d as RadTileViewItem;
			if (item != null && item.isApplyTemplateFinished)
			{
				item.InitializeTemplateChildren();
			}
		}

		private static void OnIsMouseOverDraggingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RadTileViewItem source = d as RadTileViewItem;
			if (source != null)
			{
				source.UpdateMouseOverStates();
			}
		}

		/// <summary>
		///  Maintain attached Selector.IsSelectedProperty property and RadTabItem.IsSelected property synchronized.
		/// </summary>
		protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue)
		{
			this.UpdateSelectionStates();
			this.SynchronizeIsSelected(newValue);
			if (newValue && this.ParentTileView != null && this.ParentTileView.SelectionMode == Telerik.Windows.Controls.SelectionMode.Extended)
			{
				base.Focus();
				this.ParentTileView.ContainerToFocus = this;
			}
		}

		private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RadTileViewItem tileViewItem = d as RadTileViewItem;
			if (tileViewItem != null)
			{
				tileViewItem.OnIsSelectedChanged((bool)e.OldValue, (bool)e.NewValue);
			}
		}

		protected override void OnKeyDown(KeyRoutedEventArgs e)
		{
			base.OnKeyDown(e);
			if (ParentTileView != null)
			{
				//ParentTileView.HandleKeyDown();
			}
		}

		protected virtual void OnPositionChanged(int oldPosition, int newPosition)
		{
			int num = oldPosition;
			int num1 = newPosition;
			RadRoutedEventArgs previewArgs = new RadRoutedEventArgs(RadTileViewItem.PreviewPositionChangedEvent, this);
			if (!this.IsPositionReverted)
			{
				this.OnPreviewPositionChanged(previewArgs);
				if (previewArgs.Handled)
				{
					if (num >= 0 && num < this.ParentTileView.Items.Count)
					{
						this.CancelPositionChanged = true;
						this.Position = num;
						if (this.ParentTileView != null && this.ParentTileView.IsPositionChanging)
						{
							this.ParentTileView.IsHandledPositionChange = true;
						}
					}
					if (this.ParentTileView.DragMode == TileViewDragMode.Swap)
					{
						this.ParentTileView.SwappingItems.Remove(this.ParentTileView.DraggingItem);
					}
					if (this.ParentTileView.VisibleContainers.FirstOrDefault<RadTileViewItem>((RadTileViewItem i) => {
						if (i == this)
						{
							return false;
						}
						return i.Position == num;
					}) == null)
					{
						this.ParentTileView.SwappingItems.Remove(this.ParentTileView.VisibleContainers.FirstOrDefault<RadTileViewItem>((RadTileViewItem i) => {
							if (i == this)
							{
								return false;
							}
							return i.Position == num1;
						}));
						this.CancelPositionChanged = false;
						return;
					}
					this.ParentTileView.SwappingItems.Remove(this);
					num1 = oldPosition;
					num = newPosition;
				}
			}
			if (base.Visibility == System.Windows.Visibility.Visible)
			{
				this.ParentTileView.ChangePosition(this, num, num1);
				this.PreviousPosition = num;
				if (!this.IsPositionReverted && !previewArgs.Handled)
				{
					this.OnPositionChanged(new RadRoutedEventArgs(RadTileViewItem.PositionChangedEvent, this));
				}
			}
			if (this.ParentTileView == null || !this.ParentTileView.IsPositionChanging)
			{
				this.IsPositionReverted = false;
			}
			this.CancelPositionChanged = false;
		}

		/// <summary>
		/// Raises the <see cref="E:PositionChanged" /> event.
		/// </summary>
		/// <param name="e">The <see cref="T:Telerik.Windows.RadRoutedEventArgs" /> instance containing the event data.</param>
		protected virtual void OnPositionChanged(RadRoutedEventArgs e)
		{
			this.RaiseEvent(e);
		}

		private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RadTileViewItem item = d as RadTileViewItem;
			if (item != null && item.ParentTileView != null && !item.CancelPositionChanged)
			{
				int newPos = (int)e.NewValue;
				item.OnPositionChanged((int)e.OldValue, newPos);
			}
		}

		protected virtual bool OnPreviewPositionChanged(RoutedEventArgs e)
		{
			this.RaiseEvent(e);
			return e.Handled;
		}

		protected internal virtual bool OnPreviewSelectionChanged(SelectionChangedEventArgs e)
		{
			this.RaiseEvent(e);
			return e.Handled;
		}

		protected virtual void OnPreviewTileStateChanged(PreviewTileStateChangedEventArgs e)
		{
			this.RaiseEvent(e);
		}

		private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RadTileViewItem source = d as RadTileViewItem;
			if (source != null && source.ParentTileView != null)
			{
				source.ParentTileView.OnInternalLayoutChanged();
			}
		}

		protected internal virtual void OnSelectionChanged(Telerik.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.RaiseEvent(e);
		}

		protected virtual void OnTileStateChanged(TileViewItemState oldValue, TileViewItemState newValue)
		{
			this.UpdateTileStates();
			if (this.headerPart != null)
			{
				this.headerPart.TileState = newValue;
			}
			if (this.cancelTileStateChanged || this.ParentTileView == null)
			{
				if (!this.cancelTileStateChanged)
				{
					PreviewTileStateChangedEventArgs previewTileStateChangedEventArg = new PreviewTileStateChangedEventArgs()
					{
						TileState = newValue,
						RoutedEvent = RadTileViewItem.TileStateChangedEvent,
						Source = this
					};
					this.OnTileStateChanged(previewTileStateChangedEventArg);
				}
				return;
			}
			RadRoutedEventArgs tilesStateChangedArgs = new RadRoutedEventArgs(RadTileView.PreviewTilesStateChangedEvent);
			if (!this.ParentTileView.IsPreviewTilesStateRaised)
			{
				this.ParentTileView.IsPreviewTilesStateRaised = true;
				this.ParentTileView.OnPreviewTilesStateChanged(tilesStateChangedArgs);
			}
			bool rollback = false;
			if (!tilesStateChangedArgs.Handled)
			{
				PreviewTileStateChangedEventArgs previewTileStateChangedEventArg1 = new PreviewTileStateChangedEventArgs()
				{
					TileState = oldValue,
					RoutedEvent = RadTileViewItem.PreviewTileStateChangedEvent,
					Source = this
				};
				PreviewTileStateChangedEventArgs args = previewTileStateChangedEventArg1;
				this.OnPreviewTileStateChanged(args);
				if (args.Handled)
				{
					rollback = true;
				}
				else
				{
					this.OnTileStateChanged(new TileStateChangedEventArgs(RadTileViewItem.TileStateChangedEvent, this, newValue, oldValue));
				}
			}
			else
			{
				rollback = true;
			}
			if (!this.CanChangeTileState(oldValue))
			{
				rollback = true;
			}
			if (rollback)
			{
				this.ParentTileView.IsPreviewTilesStateRaised = false;
				this.cancelTileStateChanged = true;
				this.TileState = oldValue;
				this.cancelTileStateChanged = false;
				return;
			}
			if (oldValue == TileViewItemState.Maximized && this.TileState == TileViewItemState.Minimized && this.ParentTileView.MaximizedItem == this && this.ParentTileView.MaximizeMode == TileViewMaximizeMode.ZeroOrOne)
			{
				this.ParentTileView.MaximizedItem = null;
				return;
			}
			if (base.Visibility == Visibility.Visible)
			{
				this.ParentTileView.ChangeTileState(this);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:TileStateChanged" /> event.
		/// </summary>
		/// <param name="e">The <see cref="T:Telerik.Windows.RadRoutedEventArgs" /> instance containing the event data.</param>
		protected virtual void OnTileStateChanged(RadRoutedEventArgs e)
		{
			this.RaiseEvent(e);
		}

		private static void OnTileStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TileViewItem source = d as TileViewItem;
			TileViewItemState newValue = (TileViewItemState)e.NewValue;
			TileViewItemState oldValue = (TileViewItemState)e.OldValue;
			if (source != null)
			{
				source.OnTileStateChanged(oldValue, newValue);
			}
			CommandManager.InvalidateRequerySuggested();
		}

		private static void OnToggleStateChanged(object sender, RoutedEventArgs e)
		{
			var tileViewItem = sender as TileViewItem;
			if (tileViewItem != null)
			{
				tileViewItem.ToggleTileState();
			}
		}

		private void OuterContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.ParentTileView == null || !this.ParentTileView.IsSelectionEnabled)
			{
				return;
			}
			this.ParentTileView.IsClickOutside = false;
			this.ParentTileView.HandleUISelection(this);
		}

		internal void RevertSelection(bool newValue)
		{
			this.IsSelected = newValue;
		}

		private void SynchronizeIsSelected(bool newValue)
		{
			RoutedEventArgs args;
			base.SetValue(Selector.IsSelectedProperty, newValue);
			args = (newValue ? new RoutedEventArgs(Selector.SelectedEvent, this) : new RoutedEventArgs(Selector.UnselectedEvent, this));
			this.RaiseEvent(args);
		}

		/// <summary>
		/// </summary>
		void ISelectable.OnSelected(RoutedEventArgs e)
		{
		}

		/// <summary>
		/// </summary>
		void ISelectable.OnUnselected(RoutedEventArgs e)
		{
		}

		private void ToggleTileState()
		{
			if (this.CanToggleState())
			{
				if (this.TileState == TileViewItemState.Maximized)
				{
					this.TileState = TileViewItemState.Restored;
					return;
				}
				this.TileState = TileViewItemState.Maximized;
			}
		}

		private void UpdateMouseOverStates()
		{
			if (ParentTileView == null) return;
			if (IsMouseOverDragging && ParentTileView.DragMode == TileViewDragMode.Swap)
			{
				VisualStateManager.GoToState(this, "MouseOverDragging", false);
				return;
			}
			VisualStateManager.GoToState(this, "MouseNotOverDragging", false);
		}

		private void UpdateSelectionStates()
		{
			if (ParentTileView == null)
			{
				return;
			}
			if (IsSelected)
			{
				VisualStateManager.GoToState(this, "Selected", false);
				return;
			}
			VisualStateManager.GoToState(this, "Unselected", false);
		}

		private void UpdateTileStates()
		{
			if (ParentTileView == null)	return;
			if (headerPart != null)
			{
				headerPart.TileState = TileState;
				headerPart.UpdateTileStates();
				return;
			}
			if (TileState == TileViewItemState.Maximized)
			{
				VisualStateManager.GoToState(this, "Maximized", false);
				return;
			}
			VisualStateManager.GoToState(this, "Restored", false);
		}

		public event EventHandler<RoutedEventArgs> PositionChanged
		{
			add { AddHandler(PositionChangedEvent, value, false); }
			remove { RemoveHandler(PositionChangedEvent, value); }
		}

		public event EventHandler<RoutedEventArgs> PreviewPositionChanged
		{
			add { AddHandler(PreviewPositionChangedEvent, value, false); }
			remove { RemoveHandler(PreviewPositionChangedEvent, value);	}
		}

		public event SelectionChangedEventHandler PreviewSelectionChanged
		{
			add { AddHandler(PreviewSelectionChangedEvent, value); }
			remove { RemoveHandler(PreviewSelectionChangedEvent, value); }
		}

		public event EventHandler<PreviewTileStateChangedEventArgs> PreviewTileStateChanged
		{
			add { AddHandler(PreviewTileStateChangedEvent, value, false); }
			remove { RemoveHandler(PreviewTileStateChangedEvent, value); }
		}

		public event SelectionChangedEventHandler SelectionChanged
		{
			add { AddHandler(SelectionChangedEvent, value); }
			remove { RemoveHandler(SelectionChangedEvent, value); }
		}

		public event EventHandler<RoutedEventArgs> TileStateChanged
		{
			add { AddHandler(TileViewItem.TileStateChangedEvent, value, false); }
			remove { RemoveHandler(TileViewItem.TileStateChangedEvent, value); }
		}
	}
}
