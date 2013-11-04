using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Win8XamlControlPack
{
    public class TileView : Selector
    {
        private const int DraggingOffset = 4;

        private readonly Dictionary<RadTileViewItem, VisibilityHelper> visibilityHelpers;

        private DragEventArgs mouseArgs;

        private Point lastMousePosition;

        private bool shouldMaximizeDraggedItem;

        private List<object> itemsToSelect;

        private List<object> itemsToUnselect;

        private bool isRangeSelection;

        private bool hasSelectionChanged;

        private Telerik.Windows.Controls.SelectionChanger<object> selectionChanger;

        private bool shouldHandleTab;

        private int lastSelecteIndex = -1;

        private bool shouldRemoveIndex = true;

        private object itemToSelect;

        private RadTileViewItem containerToSwap;

        private bool hasItemsSwapped;

        /// <summary>
        /// Occurs before a drag operation is started.
        /// </summary>
        /// <remarks>
        /// 	Use this event to detect when a drag operation is about to begin.
        /// 	In cases when you need to prevent the dragging, you can handle this event.
        /// </remarks>
        public readonly static Telerik.Windows.RoutedEvent PreviewTileDragStartedEvent;

        /// <summary>
        /// Occurs when a drag operation has started.
        /// </summary>
        /// <remarks>
        /// 	Use this event to detect when a tile has started being dragged.
        /// </remarks>
        public readonly static Telerik.Windows.RoutedEvent TileDragStartedEvent;

        /// <summary>
        /// Occurs when a drag operation has ended.
        /// </summary>
        /// <remarks>
        /// 	Use this event to detect when a dragging operation has ended.
        /// </remarks>
        public readonly static Telerik.Windows.RoutedEvent TileDragEndedEvent;

        /// <summary>
        /// Occurs after all <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItems</see> have changed their <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see>.
        /// </summary>
        public readonly static Telerik.Windows.RoutedEvent TilesStateChangedEvent;

        /// <summary>
        /// Occurs after all <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItems</see> have changed their <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see>.
        /// </summary>
        public readonly static Telerik.Windows.RoutedEvent TilesPositionChangedEvent;

        /// <summary>
        /// Occurs before all <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItems</see> have changed their <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see>.
        /// </summary>
        public readonly static Telerik.Windows.RoutedEvent PreviewTilesStateChangedEvent;

        /// <summary>
        /// Identifies the PreviewSelectionChanged routed event.
        /// </summary>
        public readonly static Telerik.Windows.RoutedEvent PreviewTilesSelectionChangedEvent;

        /// <summary>
        /// Identifies the TilesSelectionChanged routed event.
        /// </summary>
        public readonly static Telerik.Windows.RoutedEvent TilesSelectionChangedEvent;

        /// <summary>
        /// Identifies the IsItemDraggingEnabled dependency property.
        /// </summary>
        public readonly static DependencyProperty IsItemDraggingEnabledProperty;

        /// <summary>
        /// Identifies the IsItemsAnimationEnabled dependency property.
        /// </summary>
        public readonly static DependencyProperty IsItemsAnimationEnabledProperty;

        /// <summary>
        /// Identifies the MaxColumns dependency property.
        /// </summary>
        public readonly static DependencyProperty MaxColumnsProperty;

        /// <summary>
        /// Identifies the MaxRows dependency property.
        /// </summary>
        public readonly static DependencyProperty MaxRowsProperty;

        /// <summary>
        /// Identifies the MaximizedItem dependency property.
        /// </summary>
        public readonly static DependencyProperty MaximizedItemProperty;

        /// <summary>
        /// Identifies the MinimizedColumnWidth dependency property.
        /// </summary>
        public readonly static DependencyProperty MinimizedColumnWidthProperty;

        /// <summary>
        /// Identifies the MinimizedItemsPosition dependency property.
        /// </summary>
        public readonly static DependencyProperty MinimizedItemsPositionProperty;

        /// <summary>
        /// Identifies the MinimizedRowHeight dependency property.
        /// </summary>
        public readonly static DependencyProperty MinimizedRowHeightProperty;

        /// <summary>
        /// Identifies the ReorderingDuration dependency property.
        /// </summary>
        public readonly static DependencyProperty ReorderingDurationProperty;

        /// <summary>
        /// Identifies the ResizingDuration dependency property.
        /// </summary>
        public readonly static DependencyProperty ResizingDurationProperty;

        /// <summary>
        /// Identifies the ReorderingEasing dependency property.
        /// </summary>
        public readonly static DependencyProperty ReorderingEasingProperty;

        /// <summary>
        /// Identifies the ResizingEasing dependency property.
        /// </summary>
        public readonly static DependencyProperty ResizingEasingProperty;

        /// <summary>
        /// Identifies the MaximizeMode dependency property.
        /// </summary>
        public readonly static DependencyProperty MaximizeModeProperty;

        /// <summary>
        /// Identifies the ContentTemplate dependency property.
        /// </summary>
        public readonly static DependencyProperty ContentTemplateProperty;

        /// <summary>
        /// Identifies the ContentTemplateSelector dependency property.
        /// </summary>
        public readonly static DependencyProperty ContentTemplateSelectorProperty;

        /// <summary>
        /// Identifies the TileStateChangeTrigger dependency property.
        /// </summary>
        public readonly static DependencyProperty TileStateChangeTriggerProperty;

        /// <summary>
        /// Identifies the RowHeight dependency property.
        /// </summary>
        public readonly static DependencyProperty RowHeightProperty;

        /// <summary>
        /// Identifies the ColumnWidth dependency property.
        /// </summary>
        public readonly static DependencyProperty ColumnWidthProperty;

        /// <summary>
        /// Identifies the PreservePositionWhenMaximized dependency property.
        /// </summary>
        public readonly static DependencyProperty PreservePositionWhenMaximizedProperty;

        /// <summary>
        /// Identifies the ColumnsCount dependency property.
        /// </summary>
        public readonly static DependencyProperty ColumnsCountProperty;

        /// <summary>
        /// Identifies the RowsCount dependency property.
        /// </summary>
        public readonly static DependencyProperty RowsCountProperty;

        /// <summary>
        /// Identifies the PossibleDockingPosition dependency property.
        /// </summary>
        public readonly static DependencyProperty PossibleDockingPositionProperty;

        /// <summary>
        /// Identifies the IsDockingEnabled dependency property.
        /// </summary>
        public readonly static DependencyProperty IsDockingEnabledProperty;

        /// <summary>
        /// Identifies the IsAutoScrollingEnabled dependency property.
        /// </summary>
        public readonly static DependencyProperty IsAutoScrollingEnabledProperty;

        /// <summary>
        /// Identifies the IsVirtualizing dependency property.
        /// </summary>
        public readonly static DependencyProperty IsVirtualizingProperty;

        /// <summary>
        /// Identifies the IsSelectionEnabled dependency property.
        /// </summary>
        public readonly static DependencyProperty IsSelectionEnabledProperty;

        /// <summary>
        /// Identifies the DragMode dependency property.
        /// </summary>
        public readonly static DependencyProperty DragModeProperty;

        /// <summary>
        /// Identifies the HeaderStyle dependency property.
        /// </summary>
        public readonly static DependencyProperty HeaderStyleProperty;

        /// <summary>
        /// Identifies the SelectionMode dependency property.
        /// </summary>
        public readonly static DependencyProperty SelectionModeProperty;

        /// <summary> 
        /// Identifies the SelectedItems dependency property.
        /// </summary>
        public readonly static DependencyProperty SelectedItemsProperty;

        /// <summary>
        /// Identifies the IsItemsSizeInPercentages dependency property.
        /// </summary>
        public readonly static DependencyProperty IsItemsSizeInPercentagesProperty;

        private readonly static DependencyPropertyKey SelectedItemsPropertyKey;

        internal bool CanDragItems
        {
            get
            {
                if (this.MaximizedItem != null)
                {
                    return false;
                }
                return this.IsItemDraggingEnabled;
            }
        }

        internal int Columns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ColumnsCount property.
        /// </summary>
        public int ColumnsCount
        {
            get
            {
                return (int)base.GetValue(RadTileView.ColumnsCountProperty);
            }
            set
            {
                base.SetValue(RadTileView.ColumnsCountProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the ColumnWidth property. 
        /// </summary>
        public GridLength ColumnWidth
        {
            get
            {
                return (GridLength)base.GetValue(RadTileView.ColumnWidthProperty);
            }
            set
            {
                base.SetValue(RadTileView.ColumnWidthProperty, value);
            }
        }

        internal RadTileViewItem ContainerToFocus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the DataTemplate that will be used as a ContentTemplate for all items that do not
        /// have a ContentTemplate. This is a dependency property.
        /// </summary>
        [Telerik.Windows.Controls.SRCategory("AppearanceCategory")]
        public DataTemplate ContentTemplate
        {
            get
            {
                return (DataTemplate)base.GetValue(RadTileView.ContentTemplateProperty);
            }
            set
            {
                base.SetValue(RadTileView.ContentTemplateProperty, (object)value);
            }
        }

        /// <summary>
        /// Gets or sets the DataTemplateSelector that will be used to select a DataTemplate for the items
        /// that do not have a ContentTemplate set. This is a dependency property.
        /// </summary>
        [Telerik.Windows.Controls.SRCategory("AppearanceCategory")]
        public DataTemplateSelector ContentTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)base.GetValue(RadTileView.ContentTemplateSelectorProperty);
            }
            set
            {
                base.SetValue(RadTileView.ContentTemplateSelectorProperty, value);
            }
        }

        internal RadTileViewItem DraggingCandidate
        {
            get;
            set;
        }

        internal RadTileViewItem DraggingItem
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the DragMode property.
        /// </summary>
        public TileViewDragMode DragMode
        {
            get
            {
                return (TileViewDragMode)base.GetValue(RadTileView.DragModeProperty);
            }
            set
            {
                base.SetValue(RadTileView.DragModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the HeaderStyle property. 
        /// </summary>
        public System.Windows.Style HeaderStyle
        {
            get
            {
                return (System.Windows.Style)base.GetValue(RadTileView.HeaderStyleProperty);
            }
            set
            {
                base.SetValue(RadTileView.HeaderStyleProperty, (object)value);
            }
        }

        /// <summary>
        /// Gets or sets the IsAutoScrollingEnabled property.
        /// </summary>
        public bool IsAutoScrollingEnabled
        {
            get
            {
                return (bool)base.GetValue(RadTileView.IsAutoScrollingEnabledProperty);
            }
            set
            {
                base.SetValue(RadTileView.IsAutoScrollingEnabledProperty, value);
            }
        }

        internal bool IsClickOutside
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsDockingEnabled property.
        /// </summary>
        public bool IsDockingEnabled
        {
            get
            {
                return (bool)base.GetValue(RadTileView.IsDockingEnabledProperty);
            }
            set
            {
                base.SetValue(RadTileView.IsDockingEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a drag operation is in progress.
        /// </summary>
        public bool IsDragging
        {
            get
            {
                return this.DraggingItem != null;
            }
        }

        internal bool IsHandledPositionChange
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether item dragging is enabled.
        /// </summary>
        /// <value>
        /// 	<c>True</c> if item dragging is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsItemDraggingEnabled
        {
            get
            {
                return (bool)base.GetValue(RadTileView.IsItemDraggingEnabledProperty);
            }
            set
            {
                base.SetValue(RadTileView.IsItemDraggingEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether items animation is enabled.
        /// </summary>
        /// <value>
        /// 	<c>True</c> if items animation is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsItemsAnimationEnabled
        {
            get
            {
                if (!(bool)base.GetValue(RadTileView.IsItemsAnimationEnabledProperty))
                {
                    return false;
                }
                return AnimationManager.GetIsAnimationEnabled(this);
            }
            set
            {
                base.SetValue(RadTileView.IsItemsAnimationEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the IsItemsSizeInPercentages property.
        /// </summary>
        public bool IsItemsSizeInPercentages
        {
            get
            {
                return (bool)base.GetValue(RadTileView.IsItemsSizeInPercentagesProperty);
            }
            set
            {
                base.SetValue(RadTileView.IsItemsSizeInPercentagesProperty, value);
            }
        }

        internal bool IsPositionChanging
        {
            get;
            set;
        }

        internal bool IsPreviewTilesStateRaised
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsSelectionEnabled property.
        /// </summary>
        public bool IsSelectionEnabled
        {
            get
            {
                return (bool)base.GetValue(RadTileView.IsSelectionEnabledProperty);
            }
            set
            {
                base.SetValue(RadTileView.IsSelectionEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the is virtualized property.
        /// </summary>
        /// <value><c>True</c> if virtualization is enabled; otherwise, <c>false</c>.</value>
        public bool IsVirtualizing
        {
            get
            {
                return (bool)base.GetValue(RadTileView.IsVirtualizingProperty);
            }
            set
            {
                base.SetValue(RadTileView.IsVirtualizingProperty, value);
            }
        }

        internal object ItemForBringing
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum number of columns. 0 for no maximum. Max rows takes priority over max columns.
        /// </summary>
        /// <value>The maximum number of columns.</value>
        public int MaxColumns
        {
            get
            {
                return (int)base.GetValue(RadTileView.MaxColumnsProperty);
            }
            set
            {
                base.SetValue(RadTileView.MaxColumnsProperty, value);
            }
        }

        internal RadTileViewItem MaximizedContainer
        {
            get
            {
                return this.GetContainerFromItem(this.MaximizedItem);
            }
        }

        /// <summary>
        /// Gets or sets the currently maximized item.
        /// </summary>
        public object MaximizedItem
        {
            get
            {
                return base.GetValue(RadTileView.MaximizedItemProperty);
            }
            set
            {
                base.SetValue(RadTileView.MaximizedItemProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximize mode.
        /// </summary>
        /// <value>The maximize mode.</value>
        public TileViewMaximizeMode MaximizeMode
        {
            get
            {
                return (TileViewMaximizeMode)base.GetValue(RadTileView.MaximizeModeProperty);
            }
            set
            {
                base.SetValue(RadTileView.MaximizeModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of rows. 0 for no maximum. Max rows takes priority over max columns.
        /// </summary>
        /// <value>The maximum number of rows.</value>
        public int MaxRows
        {
            get
            {
                return (int)base.GetValue(RadTileView.MaxRowsProperty);
            }
            set
            {
                base.SetValue(RadTileView.MaxRowsProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the width for the minimized column.
        /// </summary>
        public GridLength MinimizedColumnWidth
        {
            get
            {
                return (GridLength)base.GetValue(RadTileView.MinimizedColumnWidthProperty);
            }
            set
            {
                base.SetValue(RadTileView.MinimizedColumnWidthProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the position of the minimized items.
        /// </summary>
        public Dock MinimizedItemsPosition
        {
            get
            {
                return (Dock)base.GetValue(RadTileView.MinimizedItemsPositionProperty);
            }
            set
            {
                base.SetValue(RadTileView.MinimizedItemsPositionProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the height for the minimized row.
        /// </summary>
        public GridLength MinimizedRowHeight
        {
            get
            {
                return (GridLength)base.GetValue(RadTileView.MinimizedRowHeightProperty);
            }
            set
            {
                base.SetValue(RadTileView.MinimizedRowHeightProperty, value);
            }
        }

        internal PositionToIndexDictionary PositionToIndexMap
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the PossibleDockingPosition property.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Dock? PossibleDockingPosition
        {
            get
            {
                return (Dock?)base.GetValue(RadTileView.PossibleDockingPositionProperty);
            }
            set
            {
                base.SetValue(RadTileView.PossibleDockingPositionProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the PreservePositionWhenMaximized property.
        /// </summary>
        public bool PreservePositionWhenMaximized
        {
            get
            {
                return (bool)base.GetValue(RadTileView.PreservePositionWhenMaximizedProperty);
            }
            set
            {
                base.SetValue(RadTileView.PreservePositionWhenMaximizedProperty, value);
            }
        }

        internal RadTileViewItem PreviousMaximizedContainer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the duration of the item reordering.
        /// </summary>
        /// <value>The duration of the item reordering.</value>
        public Duration ReorderingDuration
        {
            get
            {
                return (Duration)base.GetValue(RadTileView.ReorderingDurationProperty);
            }
            set
            {
                base.SetValue(RadTileView.ReorderingDurationProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the items reordering easing.
        /// </summary>
        public IEasingFunction ReorderingEasing
        {
            get
            {
                return (IEasingFunction)base.GetValue(RadTileView.ReorderingEasingProperty);
            }
            set
            {
                base.SetValue(RadTileView.ReorderingEasingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the duration of the item resizing.
        /// </summary>
        /// <value>The duration of the item resizing.</value>
        public Duration ResizingDuration
        {
            get
            {
                return (Duration)base.GetValue(RadTileView.ResizingDurationProperty);
            }
            set
            {
                base.SetValue(RadTileView.ResizingDurationProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the items resizing easing.
        /// </summary>
        public IEasingFunction ResizingEasing
        {
            get
            {
                return (IEasingFunction)base.GetValue(RadTileView.ResizingEasingProperty);
            }
            set
            {
                base.SetValue(RadTileView.ResizingEasingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the RowHeight property. 
        /// </summary>
        public GridLength RowHeight
        {
            get
            {
                return (GridLength)base.GetValue(RadTileView.RowHeightProperty);
            }
            set
            {
                base.SetValue(RadTileView.RowHeightProperty, value);
            }
        }

        internal int Rows
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the RowsCount property.
        /// </summary>
        public int RowsCount
        {
            get
            {
                return (int)base.GetValue(RadTileView.RowsCountProperty);
            }
            set
            {
                base.SetValue(RadTileView.RowsCountProperty, value);
            }
        }

        /// <summary>
        /// Gets a collection containing the items that are currently selected.
        /// </summary>
        public ObservableCollection<object> SelectedItems
        {
            get
            {
                return (ObservableCollection<object>)base.GetValue(RadTileView.SelectedItemsProperty);
            }
            private set
            {
                this.SetValue(RadTileView.SelectedItemsPropertyKey, value);
            }
        }

        /// <summary>
        /// Gets or sets the SelectionMode property.
        /// </summary>
        public Telerik.Windows.Controls.SelectionMode SelectionMode
        {
            get
            {
                return (Telerik.Windows.Controls.SelectionMode)base.GetValue(RadTileView.SelectionModeProperty);
            }
            set
            {
                base.SetValue(RadTileView.SelectionModeProperty, value);
            }
        }

        internal List<RadTileViewItem> SwappingItems
        {
            get;
            set;
        }

        internal RadTileViewItem TileStateChangeCandidate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tile state change trigger.
        /// </summary>
        /// <value>The tile state change trigger.</value>
        public Telerik.Windows.Controls.TileStateChangeTrigger TileStateChangeTrigger
        {
            get
            {
                return (Telerik.Windows.Controls.TileStateChangeTrigger)base.GetValue(RadTileView.TileStateChangeTriggerProperty);
            }
            set
            {
                base.SetValue(RadTileView.TileStateChangeTriggerProperty, value);
            }
        }

        internal List<RadTileViewItem> VisibleContainers
        {
            get;
            set;
        }

        static RadTileView()
        {
            RadTileView.PreviewTileDragStartedEvent = EventManager.RegisterRoutedEvent("PreviewTileDragStarted", RoutingStrategy.Tunnel, typeof(EventHandler<TileViewDragEventArgs>), typeof(RadTileView));
            RadTileView.TileDragStartedEvent = EventManager.RegisterRoutedEvent("TileDragStarted", RoutingStrategy.Bubble, typeof(EventHandler<TileViewDragEventArgs>), typeof(RadTileView));
            RadTileView.TileDragEndedEvent = EventManager.RegisterRoutedEvent("TileDragEnded", RoutingStrategy.Bubble, typeof(EventHandler<TileViewDragEventArgs>), typeof(RadTileView));
            RadTileView.TilesStateChangedEvent = EventManager.RegisterRoutedEvent("TilesStateChanged", RoutingStrategy.Bubble, typeof(EventHandler<RadRoutedEventArgs>), typeof(RadTileView));
            RadTileView.TilesPositionChangedEvent = EventManager.RegisterRoutedEvent("TilesPositionChanged", RoutingStrategy.Bubble, typeof(EventHandler<RadRoutedEventArgs>), typeof(RadTileView));
            RadTileView.PreviewTilesStateChangedEvent = EventManager.RegisterRoutedEvent("PreviewTilesStateChanged", RoutingStrategy.Bubble, typeof(EventHandler<RadRoutedEventArgs>), typeof(RadTileView));
            RadTileView.PreviewTilesSelectionChangedEvent = EventManager.RegisterRoutedEvent("PreviewTilesSelectionChanged", RoutingStrategy.Tunnel, typeof(Telerik.Windows.Controls.SelectionChangedEventHandler), typeof(RadTileView));
            RadTileView.TilesSelectionChangedEvent = EventManager.RegisterRoutedEvent("TilesSelectionChanged", RoutingStrategy.Bubble, typeof(Telerik.Windows.Controls.SelectionChangedEventHandler), typeof(RadTileView));
            RadTileView.IsItemDraggingEnabledProperty = DependencyProperty.Register("IsItemDraggingEnabled", typeof(bool), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(true, new PropertyChangedCallback(RadTileView.OnIsItemDraggingEnabledPropertyChanged)));
            RadTileView.IsItemsAnimationEnabledProperty = DependencyProperty.Register("IsItemsAnimationEnabled", typeof(bool), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(true));
            RadTileView.MaxColumnsProperty = DependencyProperty.Register("MaxColumns", typeof(int), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)2147483647, new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.MaxRowsProperty = DependencyProperty.Register("MaxRows", typeof(int), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)2147483647, new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.MaximizedItemProperty = DependencyProperty.Register("MaximizedItem", typeof(object), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(null, new PropertyChangedCallback(RadTileView.OnMaximizedItemChanged)));
            RadTileView.MinimizedColumnWidthProperty = DependencyProperty.Register("MinimizedColumnWidth", typeof(GridLength), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)(new GridLength(1, GridUnitType.Star)), new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.MinimizedItemsPositionProperty = DependencyProperty.Register("MinimizedItemsPosition", typeof(Dock), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)Dock.Right, new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.MinimizedRowHeightProperty = DependencyProperty.Register("MinimizedRowHeight", typeof(GridLength), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)(new GridLength(1, GridUnitType.Star)), new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.ReorderingDurationProperty = DependencyProperty.Register("ReorderingDuration", typeof(Duration), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)(new Duration(TimeSpan.FromMilliseconds(400)))));
            RadTileView.ResizingDurationProperty = DependencyProperty.Register("ResizingDuration", typeof(Duration), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)(new Duration(TimeSpan.FromMilliseconds(400)))));
            RadTileView.ReorderingEasingProperty = DependencyProperty.Register("ReorderingEasing", typeof(IEasingFunction), typeof(RadTileView), null);
            RadTileView.ResizingEasingProperty = DependencyProperty.Register("ResizingEasing", typeof(IEasingFunction), typeof(RadTileView), null);
            RadTileView.MaximizeModeProperty = DependencyProperty.Register("MaximizeMode", typeof(TileViewMaximizeMode), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)TileViewMaximizeMode.ZeroOrOne, new PropertyChangedCallback(RadTileView.OnMaximizedModeChanged)));
            RadTileView.ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(RadTileView), null);
            RadTileView.ContentTemplateSelectorProperty = DependencyProperty.Register("ContentTemplateSelector", typeof(DataTemplateSelector), typeof(RadTileView), null);
            RadTileView.TileStateChangeTriggerProperty = DependencyProperty.Register("TileStateChangeTrigger", typeof(Telerik.Windows.Controls.TileStateChangeTrigger), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)Telerik.Windows.Controls.TileStateChangeTrigger.DoubleClick));
            RadTileView.RowHeightProperty = DependencyProperty.Register("RowHeight", typeof(GridLength), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)(new GridLength(1, GridUnitType.Star)), new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.ColumnWidthProperty = DependencyProperty.Register("ColumnWidth", typeof(GridLength), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)(new GridLength(1, GridUnitType.Star)), new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.PreservePositionWhenMaximizedProperty = DependencyProperty.Register("PreservePositionWhenMaximized", typeof(bool), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(false));
            RadTileView.ColumnsCountProperty = DependencyProperty.Register("ColumnsCount", typeof(int), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)-1, new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.RowsCountProperty = DependencyProperty.Register("RowsCount", typeof(int), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)-1, new PropertyChangedCallback(RadTileView.OnInternalLayoutChanged)));
            RadTileView.PossibleDockingPositionProperty = DependencyProperty.Register("PossibleDockingPosition", typeof(Dock?), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(null, new PropertyChangedCallback(RadTileView.OnPossibleDockingPositionChanged)));
            RadTileView.IsDockingEnabledProperty = DependencyProperty.Register("IsDockingEnabled", typeof(bool), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(false));
            RadTileView.IsAutoScrollingEnabledProperty = DependencyProperty.RegisterAttached("IsAutoScrollingEnabled", typeof(bool), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(false));
            RadTileView.IsVirtualizingProperty = DependencyProperty.RegisterAttached("IsVirtualizing", typeof(bool), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(false, new PropertyChangedCallback(RadTileView.OnIsVirtualizingChanged)));
            RadTileView.IsSelectionEnabledProperty = DependencyProperty.RegisterAttached("IsSelectionEnabled", typeof(bool), typeof(RadTileView), null);
            RadTileView.DragModeProperty = DependencyProperty.Register("DragMode", typeof(TileViewDragMode), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)TileViewDragMode.Slide));
            RadTileView.HeaderStyleProperty = DependencyProperty.Register("HeaderStyle", typeof(System.Windows.Style), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(new PropertyChangedCallback(RadTileView.OnHeaderStylePropertyChanged)));
            RadTileView.SelectionModeProperty = DependencyProperty.RegisterAttached("SelectionMode", typeof(Telerik.Windows.Controls.SelectionMode), typeof(RadTileView), new Telerik.Windows.PropertyMetadata((object)Telerik.Windows.Controls.SelectionMode.Single, new PropertyChangedCallback(RadTileView.OnSelectionModeChanged)));
            RadTileView.IsItemsSizeInPercentagesProperty = DependencyProperty.RegisterAttached("IsItemsSizeInPercentages", typeof(bool), typeof(RadTileView), new Telerik.Windows.PropertyMetadata(false));
            RadTileView.SelectedItemsPropertyKey = DependencyPropertyExtensions.RegisterReadOnly("SelectedItems", typeof(ObservableCollection<object>), typeof(RadTileView), null);
            RadTileView.SelectedItemsProperty = RadTileView.SelectedItemsPropertyKey.DependencyProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Telerik.Windows.Controls.RadTileView" /> class.
        /// </summary>
        /// <remarks>
        /// 	Use this constructor to create and initialize a new instance of the RadTileView
        /// 	control.
        /// </remarks>
        public RadTileView()
        {
            base.DefaultStyleKey = typeof(RadTileView);
            this.VisibleContainers = new List<RadTileViewItem>();
            this.visibilityHelpers = new Dictionary<RadTileViewItem, VisibilityHelper>();
            this.PositionToIndexMap = new PositionToIndexDictionary();
            this.SwappingItems = new List<RadTileViewItem>();
            this.itemsToUnselect = new List<object>();
            this.itemsToSelect = new List<object>();
            this.IsClickOutside = true;
            this.ItemForBringing = null;
            this.selectionChanger = new Telerik.Windows.Controls.SelectionChanger<object>(new Func<object, bool>(this.IsItemValidForSelection), new Func<object, bool>(this.IsItemValidForDeselection), new Func<System.Windows.Controls.SelectionChangedEventArgs, bool>(this.IsSelectionChangePossible));
            this.selectionChanger.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.OnSelectionChangerSelectionChanged);
            this.SelectedItems = this.selectionChanger;
        }

        private void AddVisibilityChangeListener(RadTileViewItem container)
        {
            VisibilityHelper visibilityHelper1 = new VisibilityHelper()
            {
                TileViewItem = container,
                ContainerVisibility = container.Visibility,
                ContainerVisibilityChangeCallback = new Action<RadTileViewItem>(this.OnVisibilityChanged)
            };
            VisibilityHelper visibilityHelper = visibilityHelper1;
            Binding binding = new Binding()
            {
                Path = new PropertyPath("Visibility", new object[0]),
                Mode = BindingMode.TwoWay,
                Source = container
            };
            visibilityHelper.SetBinding(VisibilityHelper.ContainerVisibilityProperty, binding);
            if (!this.visibilityHelpers.ContainsKey(container))
            {
                this.visibilityHelpers.Add(container, visibilityHelper);
            }
        }

        private void AnimateItemPosition(RadTileViewItem item)
        {
            double endKeyTime = this.ReorderingDuration.TimeSpan.TotalSeconds;
            AnimationExtensions.AnimationContext animationContext = AnimationExtensions.Create();
            FrameworkElement[] layoutRoot = new FrameworkElement[] { item.LayoutRoot };
            AnimationExtensions.AnimationContext animationContext1 = animationContext.Animate(layoutRoot).EnsureDefaultTransforms();
            double[] numArray = new double[] { endKeyTime, default(double) };
            AnimationExtensions.AnimationContext animationContext2 = animationContext1.MoveX(numArray).EaseAll(this.ReorderingEasing);
            double[] numArray1 = new double[] { endKeyTime, default(double) };
            Storyboard instance = animationContext2.MoveY(numArray1).EaseAll(this.ReorderingEasing).AdjustSpeed().Instance;
            instance.Completed += new EventHandler((object _, EventArgs __) =>
            {
                this.SwappingItems.Remove(item);
                this.TryRaiseTilesPositionChanged();
                instance = null;
            });
            instance.Begin();
        }

        /// <summary>
        /// Brings an Item into the viewable area.
        /// </summary>
        public void BringIntoView(object item)
        {
            if (item != null)
            {
                if (this.RequestBringItemIntoView != null)
                {
                    this.RequestBringItemIntoView(this, new TileViewBringIntoViewArgs(base.Items.IndexOf(item)));
                    return;
                }
                this.ItemForBringing = item;
            }
        }

        internal void ChangePosition(RadTileViewItem item, int oldValue, int newValue)
        {
            if (this.IsPositionChanging && !item.CancelPositionChanged)
            {
                return;
            }
            if (this.VisibleContainers != null && this.VisibleContainers.Count > 0 && this.VisibleContainers.Contains(item))
            {
                this.PositionToIndexMap.ChangePosition(oldValue, newValue);
                this.UpdatePositions();
                if (this.MaximizedItem != null && !this.PreservePositionWhenMaximized)
                {
                    this.MaximizedItem = (this.PositionToIndexMap[0].VisibleItem >= 0 ? base.Items[this.PositionToIndexMap[0].VisibleItem] : null);
                }
                RadTileViewItem beforeItem = this.VisibleContainers.FirstOrDefault<RadTileViewItem>((RadTileViewItem i) =>
                {
                    if (i == item)
                    {
                        return false;
                    }
                    return i.Position == oldValue;
                });
                if (beforeItem != null)
                {
                    this.SwapItems(beforeItem, item);
                }
            }
        }

        internal void ChangeTileState(RadTileViewItem container)
        {
            if (container.TileState == TileViewItemState.Maximized)
            {
                int maxZIndex = RadTileViewItem.MaxZIndex + 1;
                RadTileViewItem.MaxZIndex = maxZIndex;
                Canvas.SetZIndex(container, maxZIndex);
                object itemCandidate = this.GetItemFromContainer(container);
                if (base.Items.IndexOf(itemCandidate) >= 0)
                {
                    this.MaximizedItem = itemCandidate;
                    container.Position = (this.PreservePositionWhenMaximized ? container.Position : 0);
                    return;
                }
            }
            else if (container.TileState == TileViewItemState.Restored)
            {
                if (this.PreviousMaximizedContainer == null)
                {
                    this.PreviousMaximizedContainer = this.MaximizedContainer;
                }
                this.MaximizedItem = null;
            }
        }

        private void CheckForMaximization()
        {
            if (this.MaximizeMode == TileViewMaximizeMode.Zero)
            {
                return;
            }
            this.shouldMaximizeDraggedItem = true;
            Point position = this.mouseArgs.GetPosition(this);
            if (position.Y < 20)
            {
                this.PossibleDockingPosition = new Dock?(Dock.Bottom);
                return;
            }
            if (position.Y + 20 > base.ActualHeight)
            {
                this.PossibleDockingPosition = new Dock?(Dock.Top);
                return;
            }
            if (position.X < 20)
            {
                this.PossibleDockingPosition = new Dock?(Dock.Right);
                return;
            }
            if (position.X + 20 > base.ActualWidth)
            {
                this.PossibleDockingPosition = new Dock?(Dock.Left);
                return;
            }
            this.shouldMaximizeDraggedItem = false;
            this.PossibleDockingPosition = null;
        }

        /// <summary>
        /// Clean up RadTileView item.
        /// </summary>
        /// <param name="element">The source RadTileView item.</param>
        /// <param name="item">The source item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            RadTileViewItem container = element as RadTileViewItem;
            if (container != null)
            {
                container.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(container.HandleMouseLeftButtonUp));
                if (container == this.DraggingItem)
                {
                    this.FinishDrag();
                }
                if (container == this.PreviousMaximizedContainer)
                {
                    this.PreviousMaximizedContainer = null;
                }
                this.VisibleContainers.Remove(container);
                if (this.visibilityHelpers.ContainsKey(container))
                {
                    this.visibilityHelpers[container].ContainerVisibilityChangeCallback = null;
                    this.visibilityHelpers.Remove(container);
                }
                if (item == this.MaximizedItem)
                {
                    if (container.Visibility == System.Windows.Visibility.Visible && this.PositionToIndexMap.ContainsKey(container.Position))
                    {
                        this.PositionToIndexMap.RemoveIndex(this.PositionToIndexMap[container.Position].VisibleItem);
                        this.shouldRemoveIndex = false;
                    }
                    this.MaximizedItem = null;
                }
                if (this.IsVirtualizing)
                {
                    RadTileView.ClearUserControls(container);
                }
            }
        }

        private static void ClearUserControls(RadTileViewItem container)
        {
            foreach (UserControl userControl in container.ChildrenOfType<UserControl>())
            {
                ContentControl contentControl = userControl.ParentOfType<ContentControl>();
                ContentPresenter contentPresenter = userControl.ParentOfType<ContentPresenter>();
                if (contentPresenter != null)
                {
                    contentPresenter.Content = null;
                }
                if (contentControl == null)
                {
                    continue;
                }
                contentControl.Content = null;
            }
        }

        private void DragTiles(Telerik.Windows.DragDrop.DragEventArgs e, Point currentMousePosition)
        {
            if (this.CanDragItems)
            {
                this.TileStateChangeCandidate = null;
                double offsetX = currentMousePosition.X - this.lastMousePosition.X;
                double offsetY = currentMousePosition.Y - this.lastMousePosition.Y;
                if (!this.IsDragging && this.DraggingCandidate != null)
                {
                    this.DraggingItem = this.DraggingCandidate;
                    this.hasItemsSwapped = false;
                    RadTileViewItem draggingItem = this.DraggingItem;
                    int maxZIndex = RadTileViewItem.MaxZIndex + 1;
                    RadTileViewItem.MaxZIndex = maxZIndex;
                    Canvas.SetZIndex(draggingItem, maxZIndex);
                    this.RaiseEvent(new TileViewDragEventArgs(this.DraggingItem, RadTileView.TileDragStartedEvent, this));
                    this.DraggingItem.Opacity = 0;
                    return;
                }
                if (this.IsDragging)
                {
                    this.lastMousePosition = currentMousePosition;
                    this.mouseArgs = e;
                    this.OnMoveDraggingItem(this, e);
                    this.MoveDraggingItem(offsetX, offsetY);
                }
            }
        }

        private void ExecuteRangeSelection(int rangeStart, int rangeEnd)
        {
            int startPos = Math.Min(rangeStart, rangeEnd);
            int endPos = Math.Max(rangeStart, rangeEnd);
            this.isRangeSelection = true;
            List<object> addedItems = new List<object>();
            List<object> removedItems = new List<object>();
            for (int i = this.SelectedItems.Count - 1; i >= 0; i--)
            {
                int position = this.PositionToIndexMap.GetPositionFromIndex(base.Items.IndexOf(this.SelectedItems[i]));
                if (position < startPos || position > endPos)
                {
                    removedItems.Add(this.SelectedItems[i]);
                }
            }
            for (int i = startPos; i <= endPos; i++)
            {
                if (this.PositionToIndexMap.ContainsKey(i))
                {
                    int index = this.PositionToIndexMap[i].VisibleItem;
                    if (!this.SelectedItems.Contains(base.Items[index]))
                    {
                        addedItems.Add(base.Items[index]);
                    }
                }
            }
            if (removedItems.Count + addedItems.Count > 0)
            {
                Telerik.Windows.Controls.SelectionChangedEventArgs args = new Telerik.Windows.Controls.SelectionChangedEventArgs(RadTileView.PreviewTilesSelectionChangedEvent, removedItems, addedItems);
                this.OnPreviewTilesSelectionChanged(args);
                if (!args.Handled)
                {
                    foreach (object item in removedItems)
                    {
                        this.SelectedItems.Remove(item);
                    }
                    foreach (object item in addedItems)
                    {
                        this.SelectedItems.Add(item);
                    }
                    if (this.PositionToIndexMap.ContainsKey(rangeEnd))
                    {
                        this.BringIntoView(base.Items[this.PositionToIndexMap[rangeEnd].VisibleItem]);
                        this.lastSelecteIndex = this.PositionToIndexMap[rangeEnd].VisibleItem;
                    }
                    this.RaiseSelectionChangedEvent(removedItems, addedItems);
                }
                this.isRangeSelection = false;
            }
        }

        private RadTileViewItem FindItemToSwap()
        {
            IEnumerable<UIElement> items = VisualTreeHelper.FindElementsInHostCoordinates(this.lastMousePosition, this);
            return items.OfType<RadTileViewItem>().Where<RadTileViewItem>((RadTileViewItem i) =>
            {
                if (i == null || i == this.DraggingItem)
                {
                    return false;
                }
                return i.ParentTileView == this;
            }).FirstOrDefault<RadTileViewItem>();
        }

        private void FinishDrag()
        {
            this.DraggingCandidate = null;
            this.mouseArgs = null;
            if (this.IsDragging && this.DraggingItem != null)
            {
                bool isDockingMaximization = (!this.shouldMaximizeDraggedItem ? false : this.PossibleDockingPosition.HasValue);
                if (this.DragMode == TileViewDragMode.Swap && this.containerToSwap != null)
                {
                    this.containerToSwap.IsMouseOverDragging = false;
                    if (!isDockingMaximization)
                    {
                        this.SwappingItems.Add(this.DraggingItem);
                        this.SwapWithDraggingItem(this.containerToSwap);
                    }
                }
                VisualStateManager.GoToState(this, "HideMaximizeArea", false);
                if (base.ItemContainerGenerator.IndexFromContainer(this.DraggingItem) != -1)
                {
                    this.OnTileDragEnded(new TileViewDragEventArgs(this.DraggingItem, RadTileView.TileDragEndedEvent, this));
                }
                if (this.DraggingItem != null)
                {
                    this.DraggingItem.Opacity = 1;
                    if (!isDockingMaximization)
                    {
                        TranslateTransform moveTransform = this.DraggingItem.MoveTransform;
                        double num = 0;
                        double num1 = num;
                        this.DraggingItem.MoveTransform.Y = num;
                        moveTransform.X = num1;
                        this.DraggingItem = null;
                    }
                    else
                    {
                        this.MinimizedItemsPosition = this.PossibleDockingPosition.Value;
                        this.PossibleDockingPosition = null;
                        RadTileViewItem tmp = this.DraggingItem;
                        TranslateTransform translateTransform = this.DraggingItem.MoveTransform;
                        double num2 = 0;
                        double num3 = num2;
                        this.DraggingItem.MoveTransform.Y = num2;
                        translateTransform.X = num3;
                        this.DraggingItem = null;
                        tmp.TileState = TileViewItemState.Maximized;
                    }
                }
                if (this.hasItemsSwapped)
                {
                    this.TryRaiseTilesPositionChanged();
                }
            }
            this.hasItemsSwapped = false;
        }

        private void FixMaximizedItem()
        {
            if (this.MaximizeMode == TileViewMaximizeMode.Zero)
            {
                this.MaximizedItem = null;
                return;
            }
            if (this.MaximizeMode == TileViewMaximizeMode.One)
            {
                if (base.Items.Count > 0 && this.MaximizedItem == null)
                {
                    this.MaximizedItem = base.Items[0];
                    return;
                }
                if (base.Items.Count == 0)
                {
                    this.MaximizedItem = null;
                }
            }
        }

        /// <summary>
        /// Returns a new RadTileView.
        /// </summary>
        /// <returns>A new RadTileView.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RadTileViewItem();
        }

        private RadTileViewItem GetContainerFromItem(object item)
        {
            object obj;
            if (this.IsItemItsOwnContainerOverride(item))
            {
                obj = item;
            }
            else
            {
                obj = base.ItemContainerGenerator.ContainerFromItem(item);
            }
            return obj as RadTileViewItem;
        }

        private int GetFirstAvailablePosition(int setPosition, RadTileViewItem container, int firstAvailablePosition)
        {
            int num;
            if (setPosition >= 0 && setPosition < base.Items.Count - this.PositionToIndexMap.InvisibleIndexes.Count)
            {
                if (this.VisibleContainers.FirstOrDefault<RadTileViewItem>((RadTileViewItem c) =>
                {
                    if (c.Position != setPosition)
                    {
                        return false;
                    }
                    return c != container;
                }) == null)
                {
                    return setPosition;
                }
                return this.GetFirstAvailablePosition(-1, container, firstAvailablePosition);
            }
            using (IEnumerator<RadTileViewItem> enumerator = (
                from c in this.VisibleContainers
                orderby c.Position
                select c).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (firstAvailablePosition != enumerator.Current.Position)
                    {
                        continue;
                    }
                    num = this.GetFirstAvailablePosition(-1, container, firstAvailablePosition + 1);
                    return num;
                }
                return firstAvailablePosition;
            }
            return num;
        }

        private int GetIndexFromContainer(RadTileViewItem container)
        {
            return base.ItemContainerGenerator.IndexFromContainer(container);
        }

        private object GetItemFromContainer(RadTileViewItem container)
        {
            return base.ItemContainerGenerator.ItemFromContainer(container);
        }

        private RadTileViewItem GetMinPositionContainer()
        {
            if (this.VisibleContainers == null || this.VisibleContainers.Count <= 0)
            {
                return null;
            }
            int num = this.VisibleContainers.Min<RadTileViewItem>((RadTileViewItem i) => i.Position);
            RadTileViewItem minPositionItem = this.VisibleContainers.FirstOrDefault<RadTileViewItem>((RadTileViewItem i) => i.Position == num);
            return minPositionItem;
        }

        private void HandleArrowKeySelection(Key key)
        {
            int nextPosition;
            if (!this.IsSelectionEnabled)
            {
                return;
            }
            if (base.SelectedItem != null)
            {
                int selectedPosition = this.PositionToIndexMap.GetPositionFromIndex(base.SelectedIndex);
                int lastSelectedPosition = (this.lastSelecteIndex >= 0 ? this.PositionToIndexMap.GetPositionFromIndex(this.lastSelecteIndex) : -1);
                nextPosition = (lastSelectedPosition < 0 || lastSelectedPosition >= base.Items.Count ? selectedPosition : lastSelectedPosition);
                switch (key)
                {
                    case Key.Left:
                        {
                            nextPosition--;
                            break;
                        }
                    case Key.Up:
                        {
                            if (this.MaximizedItem != null)
                            {
                                nextPosition--;
                                break;
                            }
                            else
                            {
                                nextPosition = nextPosition - this.Columns;
                                break;
                            }
                        }
                    case Key.Right:
                        {
                            nextPosition++;
                            break;
                        }
                    case Key.Down:
                        {
                            if (this.MaximizedItem != null)
                            {
                                nextPosition++;
                                break;
                            }
                            else
                            {
                                nextPosition = nextPosition + this.Columns;
                                break;
                            }
                        }
                }
                if (nextPosition < 0 || nextPosition >= base.Items.Count)
                {
                    nextPosition = (lastSelectedPosition < 0 || lastSelectedPosition >= base.Items.Count ? selectedPosition : lastSelectedPosition);
                }
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    this.ExecuteRangeSelection(selectedPosition, nextPosition);
                    return;
                }
                int nextIndex = this.PositionToIndexMap[nextPosition].VisibleItem;
                this.SetSelectedItem(base.Items[nextIndex]);
            }
        }

        internal void HandleKeyDown(KeyEventArgs e)
        {
            if (!this.IsSelectionEnabled || this.SelectionMode != Telerik.Windows.Controls.SelectionMode.Extended || this.DraggingItem != null)
            {
                return;
            }
            Key key = e.Key;
            if (key > Key.Escape)
            {
                switch (key)
                {
                    case Key.Left:
                    case Key.Up:
                    case Key.Right:
                    case Key.Down:
                        {
                            if (base.SelectedItem == null)
                            {
                                break;
                            }
                            this.HandleArrowKeySelection(e.Key);
                            this.shouldHandleTab = true;
                            break;
                        }
                    default:
                        {
                            if (key == Key.A)
                            {
                                bool ctrKeyPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                                if (base.SelectedItem == null || !ctrKeyPressed)
                                {
                                    break;
                                }
                                this.ExecuteRangeSelection(0, base.Items.Count - 1);
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                }
            }
            else if (key == Key.Tab)
            {
                this.HandleTabSelection();
            }
            else if (key == Key.Escape)
            {
                this.UnselectAllItems();
            }
            e.Handled = this.shouldHandleTab;
            this.shouldHandleTab = false;
        }

        private void HandleTabSelection()
        {
            if (!this.IsSelectionEnabled)
            {
                return;
            }
            this.shouldHandleTab = true;
            if (base.SelectedItem == null)
            {
                RadTileViewItem container = (
                    from vc in this.VisibleContainers
                    orderby vc.Position
                    select vc).FirstOrDefault<RadTileViewItem>();
                this.SelectedItems.Add(base.ItemContainerGenerator.ItemFromContainer(container));
                return;
            }
            bool shiftKeyPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            int selectedPosition = this.PositionToIndexMap.GetPositionFromIndex(base.SelectedIndex);
            int offset = 0;
            if (shiftKeyPressed)
            {
                if (selectedPosition <= 0)
                {
                    this.shouldHandleTab = false;
                }
                else
                {
                    offset = -1;
                }
            }
            else if (selectedPosition >= base.Items.Count - 1)
            {
                this.shouldHandleTab = false;
            }
            else
            {
                offset = 1;
            }
            int nextIndex = this.PositionToIndexMap[selectedPosition + offset].VisibleItem;
            this.SetSelectedItem(base.Items[nextIndex]);
        }

        internal void HandleUISelection(RadTileViewItem radTileViewItem)
        {
            if (!radTileViewItem.IsEnabled || !this.IsSelectionEnabled)
            {
                return;
            }
            this.itemToSelect = null;
            bool controlKeyPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool shiftKeyPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            object item = base.ItemContainerGenerator.ItemFromContainer(radTileViewItem);
            if (this.SelectionMode != Telerik.Windows.Controls.SelectionMode.Extended)
            {
                this.ToggleItemSelection(item);
                return;
            }
            if (!shiftKeyPressed)
            {
                if (controlKeyPressed)
                {
                    this.ToggleItemSelection(item);
                    return;
                }
                if (this.SelectedItems.Count <= 1)
                {
                    this.SetSelectedItem(item);
                    return;
                }
                this.itemToSelect = item;
                return;
            }
            if (base.SelectedItem == null)
            {
                this.SetSelectedItem(item);
                return;
            }
            int selectedIndex = base.Items.IndexOf(base.SelectedItem);
            int selectedItemPosition = this.PositionToIndexMap.GetPositionFromIndex(selectedIndex);
            this.ExecuteRangeSelection(selectedItemPosition, radTileViewItem.Position);
        }

        private void InitializeDragAndDrop()
        {
            DragDropManager.RemoveDragInitializeHandler(this, new DragInitializeEventHandler(this.OnDragInitialized));
            DragDropManager.RemoveDragOverHandler(this, new Telerik.Windows.DragDrop.DragEventHandler(this.OnDragOver));
            DragDropManager.RemoveDragDropCompletedHandler(this, new DragDropCompletedEventHandler(this.OnElementDragDropCompleted));
            DragDropManager.RemoveDropHandler(this, new Telerik.Windows.DragDrop.DragEventHandler(this.OnDropCompleted));
            DragDropManager.RemoveGiveFeedbackHandler(this, new GiveFeedbackEventHandler(this.OnGiveFeedback));
            DragDropManager.AddDragInitializeHandler(this, new DragInitializeEventHandler(this.OnDragInitialized));
            DragDropManager.AddDragOverHandler(this, new Telerik.Windows.DragDrop.DragEventHandler(this.OnDragOver));
            DragDropManager.AddDragDropCompletedHandler(this, new DragDropCompletedEventHandler(this.OnElementDragDropCompleted));
            DragDropManager.AddDropHandler(this, new Telerik.Windows.DragDrop.DragEventHandler(this.OnDropCompleted));
            DragDropManager.AddGiveFeedbackHandler(this, new GiveFeedbackEventHandler(this.OnGiveFeedback));
        }

        /// <summary>
        /// Returns true is item is a RadTileView.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if item is a RadTileView.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is RadTileViewItem;
        }

        private bool IsItemValidForDeselection(object item)
        {
            return this.ValidateSelectionChange(item, false);
        }

        private bool IsItemValidForSelection(object item)
        {
            return this.ValidateSelectionChange(item, true);
        }

        private bool IsSelectionChangePossible(System.Windows.Controls.SelectionChangedEventArgs e)
        {
            return false;
        }

        internal void MoveDraggingItem(double offsetX, double offsetY)
        {
            if (this.IsDragging)
            {
                if (this.IsDockingEnabled)
                {
                    this.CheckForMaximization();
                }
                TranslateTransform moveTransform = this.DraggingItem.MoveTransform;
                moveTransform.Y = moveTransform.Y + offsetY;
                if (base.FlowDirection != System.Windows.FlowDirection.LeftToRight)
                {
                    TranslateTransform x = this.DraggingItem.MoveTransform;
                    x.X = x.X - offsetX;
                }
                else
                {
                    TranslateTransform translateTransform = this.DraggingItem.MoveTransform;
                    translateTransform.X = translateTransform.X + offsetX;
                }
                RadTileViewItem itemToSwap = this.FindItemToSwap();
                if (this.DragMode == TileViewDragMode.Slide)
                {
                    this.SwapWithDraggingItem(itemToSwap);
                    return;
                }
                if (this.containerToSwap != null && this.containerToSwap != itemToSwap)
                {
                    this.containerToSwap.IsMouseOverDragging = false;
                }
                this.containerToSwap = itemToSwap;
                if (this.containerToSwap != null)
                {
                    this.containerToSwap.IsMouseOverDragging = true;
                }
            }
        }

        /// <summary>
        /// Applies the template for the RadTileView.
        /// </summary>
        public override void OnApplyTemplate()
        {
            this.InitializeDragAndDrop();
            this.PositionToIndexMap.Reset(base.Items.Count);
            base.OnApplyTemplate();
            base.Dispatcher.BeginInvoke(() =>
            {
                if (this.MaximizedItem != null || !TileViewPanel.GetIsVirtualized(this) && this.VisibleContainers.Count != base.Items.Count)
                {
                    this.OnInternalLayoutChanged();
                }
            });
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
            return new Telerik.Windows.Automation.Peers.RadTileViewAutomationPeer(this);
        }

        private void OnDragInitialized(object sender, DragInitializeEventArgs args)
        {
            if (!this.CanDragItems || this.DraggingCandidate == null)
            {
                if (args.Data == null)
                {
                    args.Cancel = true;
                    this.TileStateChangeCandidate = null;
                }
                return;
            }
            WriteableBitmap writeableBitmap = new WriteableBitmap(this.DraggingCandidate, null);
            args.DragVisual = new Image()
            {
                Source = writeableBitmap
            };
            args.Handled = true;
        }

        private void OnDragOver(object sender, Telerik.Windows.DragDrop.DragEventArgs args)
        {
            Point currentMousePosition = args.GetPosition(ApplicationHelper.GetRootVisual(this));
            if (this == sender && this.DraggingCandidate != null && this.lastMousePosition != currentMousePosition)
            {
                this.DragTiles(args, currentMousePosition);
                args.Handled = true;
            }
        }

        private void OnDropCompleted(object sender, Telerik.Windows.DragDrop.DragEventArgs args)
        {
            this.FinishDrag();
            args.Handled = true;
        }

        private void OnElementDragDropCompleted(object sender, DragDropCompletedEventArgs e)
        {
            this.FinishDrag();
            e.Handled = true;
        }

        private void OnGiveFeedback(object sender, GiveFeedbackEventArgs args)
        {
            if (this.DraggingCandidate != null)
            {
                args.Handled = true;
                args.UseDefaultCursors = false;
                args.SetCursor(Cursors.Arrow);
            }
        }

        /// <summary>
        /// Invoked when the HeaderStyle property changes.
        /// </summary>
        protected virtual void OnHeaderStyleChanged(System.Windows.Style oldValue, System.Windows.Style newValue)
        {
            foreach (RadTileViewItem container in this.VisibleContainers)
            {
                container.HeaderStyle = newValue;
            }
        }

        private static void OnHeaderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadTileView tileView = d as RadTileView;
            if (tileView != null)
            {
                System.Windows.Style newValue = (System.Windows.Style)e.NewValue;
                tileView.OnHeaderStyleChanged((System.Windows.Style)e.OldValue, newValue);
            }
        }

        internal void OnInternalLayoutChanged()
        {
            if (this.InternalLayoutChanged != null)
            {
                this.InternalLayoutChanged(this, EventArgs.Empty);
            }
        }

        private static void OnInternalLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadTileView tileView = d as RadTileView;
            if (tileView != null)
            {
                tileView.OnInternalLayoutChanged();
            }
        }

        private void OnIsItemDraggingEnabledChanged()
        {
            foreach (RadTileViewItem container in this.VisibleContainers)
            {
                DragDropManager.SetAllowDrag(container, this.IsItemDraggingEnabled);
            }
        }

        private static void OnIsItemDraggingEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadTileView tileView = d as RadTileView;
            if (tileView != null)
            {
                tileView.OnIsItemDraggingEnabledChanged();
            }
        }

        private static void OnIsVirtualizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TileViewPanel.SetIsVirtualized(d, (bool)e.NewValue);
        }

        /// <summary>
        /// Invoked when the Items property changes.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> that contains the event data.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        this.PositionToIndexMap.AddIndex(e.NewStartingIndex);
                        this.UpdatePositions();
                        this.FixMaximizedItem();
                        return;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        this.SelectedItems.Remove(e.OldItems[0]);
                        if (this.shouldRemoveIndex)
                        {
                            this.PositionToIndexMap.RemoveIndex(e.OldStartingIndex);
                        }
                        this.shouldRemoveIndex = true;
                        this.UpdatePositions();
                        this.FixMaximizedItem();
                        return;
                    }
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    {
                        this.PositionToIndexMap.Reset(base.Items.Count);
                        base.Dispatcher.BeginInvoke(() =>
                        {
                            if (this.MaximizedItem != null || !TileViewPanel.GetIsVirtualized(this) && this.VisibleContainers.Count != base.Items.Count)
                            {
                                this.OnInternalLayoutChanged();
                            }
                        });
                        this.shouldRemoveIndex = true;
                        this.UpdatePositions();
                        this.FixMaximizedItem();
                        return;
                    }
                case NotifyCollectionChangedAction.Remove | NotifyCollectionChangedAction.Replace:
                    {
                        this.UpdatePositions();
                        this.FixMaximizedItem();
                        return;
                    }
                default:
                    {
                        this.UpdatePositions();
                        this.FixMaximizedItem();
                        return;
                    }
            }
        }

        /// <summary>
        /// Invoked after a key has been pressed.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            this.HandleKeyDown(e);
        }

        private static void OnMaximizedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadTileView tileView = d as RadTileView;
            if (tileView != null)
            {
                tileView.OnMaximizedItemChanged(e.OldValue);
            }
        }

        private void OnMaximizedItemChanged(object previousMaxItem)
        {
            this.FixMaximizedItem();
            this.TileStateChangeCandidate = null;
            if (this.MaximizedItem == null)
            {
                this.VisibleContainers.ForEach((RadTileViewItem c) => c.TileState = TileViewItemState.Restored);
            }
            else if (this.MaximizedContainer != null)
            {
                this.MaximizedContainer.TileState = TileViewItemState.Maximized;
                this.PreviousMaximizedContainer = null;
                List<RadTileViewItem> items = (
                    from i in this.VisibleContainers
                    where i != this.MaximizedContainer
                    select i).ToList<RadTileViewItem>();
                foreach (RadTileViewItem container in items)
                {
                    if (container.TileState == TileViewItemState.Maximized)
                    {
                        this.PreviousMaximizedContainer = container;
                    }
                    container.TileState = TileViewItemState.Minimized;
                }
            }
            else if (previousMaxItem != null)
            {
                this.PreviousMaximizedContainer = base.ItemContainerGenerator.ContainerFromItem(previousMaxItem) as RadTileViewItem;
            }
            this.TryRaiseTilesStateChanged();
        }

        private static void OnMaximizedModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadTileView tileView = d as RadTileView;
            if (tileView != null)
            {
                tileView.FixMaximizedItem();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Invoked on mouse left button up.
        /// </summary>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            this.DraggingCandidate = null;
            if (!this.IsDragging)
            {
                if (!this.IsClickOutside)
                {
                    if (this.SelectedItems.Count > 1 && this.itemToSelect != null)
                    {
                        this.SetSelectedItem(this.itemToSelect);
                    }
                    if (this.ContainerToFocus != null && this.SelectionMode == Telerik.Windows.Controls.SelectionMode.Extended)
                    {
                        this.ContainerToFocus.Focus();
                    }
                }
                else
                {
                    bool controlKeyPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                    bool shiftKeyPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                    if (!controlKeyPressed && !shiftKeyPressed)
                    {
                        this.UnselectAllItems();
                    }
                }
            }
            this.itemToSelect = null;
            this.IsClickOutside = true;
            this.ContainerToFocus = null;
        }

        private static void OnPossibleDockingPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadTileView tileView = d as RadTileView;
            Dock? newValue = (Dock?)(e.NewValue as Dock?);
            Dock valueOrDefault = newValue.GetValueOrDefault();
            if (!newValue.HasValue)
            {
                VisualStateManager.GoToState(tileView, "HideMaximizeArea", false);
                return;
            }
            switch (valueOrDefault)
            {
                case Dock.Left:
                    {
                        VisualStateManager.GoToState(tileView, "DockRight", false);
                        return;
                    }
                case Dock.Top:
                    {
                        VisualStateManager.GoToState(tileView, "DockBottom", false);
                        return;
                    }
                case Dock.Right:
                    {
                        VisualStateManager.GoToState(tileView, "DockLeft", false);
                        return;
                    }
                case Dock.Bottom:
                    {
                        VisualStateManager.GoToState(tileView, "DockTop", false);
                        return;
                    }
                default:
                    {
                        return;
                    }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:PreviewTileDragStarted" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:Telerik.Windows.Controls.TileViewDragEventArgs" /> instance containing the event data.</param>
        protected internal virtual void OnPreviewTileDragStarted(TileViewDragEventArgs e)
        {
            this.RaiseEvent(e);
        }

        /// <summary>
        /// Raises the <see cref="E:PreviewSelectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:Telerik.Windows.RadRoutedEventArgs" /> instance containing the event data.</param>
        protected internal virtual bool OnPreviewTilesSelectionChanged(Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.RaiseEvent(e);
            return e.Handled;
        }

        /// <summary>
        /// Raises the <see cref="E:PreviewTilesStateChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:Telerik.Windows.RadRoutedEventArgs" /> instance containing the event data.</param>
        protected internal virtual void OnPreviewTilesStateChanged(RadRoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        /// <summary>
        /// Invoked after the SelectionChanges has finished changes.
        /// </summary>
        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            bool isMultyChange = e.AddedItems.Count + e.RemovedItems.Count > 1;
            Telerik.Windows.Controls.SelectionChangedEventArgs args = null;
            if (isMultyChange)
            {
                if (!this.isRangeSelection)
                {
                    args = new Telerik.Windows.Controls.SelectionChangedEventArgs(RadTileView.PreviewTilesSelectionChangedEvent, e.RemovedItems, e.AddedItems);
                    this.OnPreviewTilesSelectionChanged(args);
                }
                this.isRangeSelection = true;
            }
            if (args == null || args != null && !args.Handled)
            {
                for (int i = 0; i < e.AddedItems.Count; i++)
                {
                    object item = e.AddedItems[i];
                    this.SelectedItems.Add(item);
                }
                for (int i = 0; i < e.RemovedItems.Count; i++)
                {
                    object item = e.RemovedItems[i];
                    this.SelectedItems.Remove(item);
                }
                if (isMultyChange)
                {
                    this.isRangeSelection = false;
                    if (e.AddedItems.Count > 0)
                    {
                        this.lastSelecteIndex = base.Items.IndexOf(e.AddedItems[e.AddedItems.Count - 1]);
                    }
                    else if (this.SelectedItems.Count <= 0)
                    {
                        this.lastSelecteIndex = -1;
                    }
                    else
                    {
                        this.lastSelecteIndex = base.Items.IndexOf(this.SelectedItems[this.SelectedItems.Count - 1]);
                    }
                    this.BringIntoView(base.SelectedItem);
                }
                if (!this.isRangeSelection)
                {
                    this.RaiseSelectionChangedEvent(e.RemovedItems, e.AddedItems);
                    return;
                }
            }
            else
            {
                this.SelectedItems.Clear();
                foreach (object item in e.RemovedItems)
                {
                    this.SelectedItems.Add(item);
                }
            }
        }

        private void OnSelectionChangerSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (object item in e.RemovedItems)
            {
                this.UnselectItem(item);
                if (base.SelectedItem == null || !base.SelectedItem.Equals(item))
                {
                    continue;
                }
                base.SelectedItem = this.SelectedItems.FirstOrDefault<object>((object i) => !base.SelectedItem.Equals(i));
            }
            foreach (object item in e.AddedItems)
            {
                this.SelectItem(item);
            }
            if (base.SelectedItem == null && this.SelectedItems.Count >= 1)
            {
                base.SelectedItem = this.SelectedItems[0];
            }
        }

        private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadTileView tileView = d as RadTileView;
            if (tileView != null)
            {
                tileView.CanSelectMultiple = (Telerik.Windows.Controls.SelectionMode)e.NewValue != Telerik.Windows.Controls.SelectionMode.Single;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:TileDragEnded" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:Telerik.Windows.Controls.TileViewDragEventArgs" /> instance containing the event data.</param>
        protected internal virtual void OnTileDragEnded(TileViewDragEventArgs e)
        {
            this.RaiseEvent(e);
        }

        /// <summary>
        /// Raises the <see cref="E:TileDragStarted" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:Telerik.Windows.Controls.TileViewDragEventArgs" /> instance containing the event data.</param>
        protected internal virtual void OnTileDragStarted(TileViewDragEventArgs e)
        {
            this.RaiseEvent(e);
        }

        /// <summary>
        /// Raises the <see cref="E:TilePositionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:Telerik.Windows.RadRoutedEventArgs" /> instance containing the event data.</param>
        protected internal virtual void OnTilePositionChanged(RadRoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        /// <summary>
        /// Raises the <see cref="E:TilesSelectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:Telerik.Windows.RadRoutedEventArgs" /> instance containing the event data.</param>
        protected internal virtual void OnTilesSelectionChanged(Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        /// <summary>
        /// </summary>
        protected internal virtual void OnTilesStateChanged(RadRoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        internal void OnVisibilityChanged(RadTileViewItem container)
        {
            if (this.DraggingItem != null && this.DraggingItem == container)
            {
                return;
            }
            if (this.IsVirtualizing || base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                this.VisibleContainers = this.GetContainers<RadTileViewItem>().Where<RadTileViewItem>((RadTileViewItem i) =>
                {
                    if (i == null)
                    {
                        return false;
                    }
                    return i.Visibility == System.Windows.Visibility.Visible;
                }).ToList<RadTileViewItem>();
            }
            bool isVisible = container.Visibility == base.Visibility;
            int index = base.Items.IndexOf(this.GetItemFromContainer(container));
            int realPosition = (TileViewPanel.GetIsVirtualized(this) ? this.PositionToIndexMap.GetPositionFromIndex(index) : container.Position);
            if (isVisible)
            {
                if (container.TileState == TileViewItemState.Maximized || this.MaximizeMode == TileViewMaximizeMode.One && this.VisibleContainers.Count == 1)
                {
                    this.MaximizedItem = base.ItemContainerGenerator.ItemFromContainer(container);
                    realPosition = (this.PreservePositionWhenMaximized ? container.Position : 0);
                }
                else if (this.MaximizedItem == null)
                {
                    container.TileState = TileViewItemState.Restored;
                }
                else if (this.MaximizedItem != base.ItemContainerGenerator.ItemFromContainer(container))
                {
                    container.TileState = TileViewItemState.Minimized;
                }
            }
            if (!this.PreservePositionWhenMaximized && this.MaximizedItem != null && realPosition == 0 && this.MaximizedContainer.Visibility == System.Windows.Visibility.Visible)
            {
                this.MaximizedItem = base.ItemContainerGenerator.ItemFromContainer(container);
            }
            this.PositionToIndexMap.ChangeVisibility(index, realPosition, isVisible);
            this.UpdatePositions();
            if (!isVisible && container.TileState == TileViewItemState.Maximized)
            {
                if (this.MaximizeMode == TileViewMaximizeMode.ZeroOrOne)
                {
                    this.MaximizedItem = null;
                    return;
                }
                if (this.VisibleContainers.Count > 0)
                {
                    this.MaximizedItem = (
                        from c in this.VisibleContainers
                        orderby c.Position
                        select c).FirstOrDefault<RadTileViewItem>();
                }
            }
        }

        /// <summary>
        /// Prepares a TileView item.
        /// </summary>
        /// <param name="element">The TileView item.</param>
        /// <param name="item">The source item.</param>	
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (this.ItemForBringing != null)
            {
                this.BringIntoView(this.ItemForBringing);
                this.ItemForBringing = null;
            }
            RadTileViewItem container = element as RadTileViewItem;
            base.PrepareContainerForItemOverride(element, item);
            if (container != null)
            {
                container.ParentTileView = this;
                this.AddVisibilityChangeListener(container);
                bool isVisibile = container.Visibility == System.Windows.Visibility.Visible;
                int index = base.Items.IndexOf(item);
                if (index < 0)
                {
                    index = this.TryToFindItemInCollection(item);
                }
                if ((container.IsSelected || this.itemsToSelect.Contains(item)) && !this.itemsToUnselect.Contains(item))
                {
                    this.SelectedItems.Add(item);
                    this.itemsToSelect.Remove(item);
                }
                else if (this.itemsToUnselect.Contains(item))
                {
                    bool shouldRaiseSelectionChange = false;
                    if (this.SelectedItems.Contains(item) && !this.itemsToUnselect.Contains(item))
                    {
                        shouldRaiseSelectionChange = true;
                    }
                    this.SelectedItems.Remove(item);
                    if (shouldRaiseSelectionChange)
                    {
                        Telerik.Windows.RoutedEvent tilesSelectionChangedEvent = RadTileView.TilesSelectionChangedEvent;
                        List<object> objs = new List<object>()
						{
							item
						};
                        this.OnTilesSelectionChanged(new Telerik.Windows.Controls.SelectionChangedEventArgs(tilesSelectionChangedEvent, objs, new List<object>()));
                    }
                    this.itemsToUnselect.Remove(item);
                }
                if (TileViewPanel.GetIsVirtualized(this))
                {
                    container.IsSelected = this.SelectedItems.Contains(item);
                    container.Position = this.PositionToIndexMap.GetPositionFromIndex(index);
                    if (!isVisibile && !this.PositionToIndexMap.InvisibleIndexes.Contains(index) || isVisibile && this.PositionToIndexMap.InvisibleIndexes.Contains(index))
                    {
                        this.OnVisibilityChanged(container);
                    }
                }
                else
                {
                    GeneratorStatus status = GeneratorStatus.GeneratingContainers;
                    int posiblePosition = this.GetFirstAvailablePosition(container.Position, container, 0);
                    if (!isVisibile)
                    {
                        int maxSetPosition = (this.VisibleContainers.Count > 0 ? this.VisibleContainers.Max<RadTileViewItem>((RadTileViewItem vc) => vc.Position) : 0);
                        container.Position = (posiblePosition > maxSetPosition ? (base.ItemContainerGenerator.Status == status ? index - this.PositionToIndexMap.InvisibleIndexes.Count : index) : posiblePosition);
                        this.OnVisibilityChanged(container);
                        this.PositionToIndexMap.RemoveLostVisibleIndex(index);
                    }
                    else if (index >= posiblePosition || (container.Position == posiblePosition || this.MaximizedItem != null) && base.ItemContainerGenerator.Status == status)
                    {
                        container.Position = posiblePosition;
                    }
                    else
                    {
                        container.Position = index;
                    }
                    if (this.PositionToIndexMap.ContainsKey(container.Position))
                    {
                        this.PositionToIndexMap[container.Position].Add(index, isVisibile);
                    }
                }
                if (isVisibile)
                {
                    if (!this.VisibleContainers.Contains(container))
                    {
                        this.VisibleContainers.Add(container);
                    }
                    if (this.MaximizeMode != TileViewMaximizeMode.Zero && container.TileState == TileViewItemState.Maximized)
                    {
                        this.ChangeTileState(container);
                    }
                    if (this.MaximizedItem == null)
                    {
                        container.TileState = TileViewItemState.Restored;
                    }
                    else
                    {
                        container.TileState = (this.MaximizedItem.Equals(item) ? TileViewItemState.Maximized : TileViewItemState.Minimized);
                    }
                }
                if (this.HeaderStyle != null && !container.IsLocalValueSet(RadTileViewItem.HeaderStyleProperty))
                {
                    container.HeaderStyle = this.HeaderStyle;
                }
                DragDropManager.SetAllowDrag(container, this.IsItemDraggingEnabled);
                this.SetContent(container, item);
                StyleManager.SetThemeFromParent(container, this);
            }
        }

        private void RaiseSelectionChangedEvent(IList removedItems, IList addedItems)
        {
            if (this.hasSelectionChanged)
            {
                this.OnTilesSelectionChanged(new Telerik.Windows.Controls.SelectionChangedEventArgs(RadTileView.TilesSelectionChangedEvent, removedItems, addedItems));
            }
            this.hasSelectionChanged = false;
        }

        private bool RaiseSelectionEvents(object item, bool newValue)
        {
            bool result = false;
            List<object> addItems = (newValue ? new List<object>()
			{
				item
			} : new List<object>());
            List<object> removeItems = (newValue ? new List<object>() : new List<object>()
			{
				item
			});
            if (!this.isRangeSelection)
            {
                Telerik.Windows.Controls.SelectionChangedEventArgs args = new Telerik.Windows.Controls.SelectionChangedEventArgs(RadTileView.PreviewTilesSelectionChangedEvent, removeItems, addItems);
                result = this.OnPreviewTilesSelectionChanged(args);
            }
            if (!result)
            {
                Telerik.Windows.Controls.SelectionChangedEventArgs routedArgs = new Telerik.Windows.Controls.SelectionChangedEventArgs(RadTileViewItem.PreviewSelectionChangedEvent, removeItems, addItems);
                this.RaiseEvent(routedArgs);
                result = routedArgs.Handled;
            }
            if (!result)
            {
                this.RaiseEvent(new Telerik.Windows.Controls.SelectionChangedEventArgs(RadTileViewItem.SelectionChangedEvent, removeItems, addItems));
                this.hasSelectionChanged = true;
            }
            return result;
        }

        private void SelectItem(object item)
        {
            RadTileViewItem containerItem = item as RadTileViewItem ?? base.ItemContainerGenerator.ContainerFromItem(item) as RadTileViewItem;
            if (!this.isRangeSelection)
            {
                this.BringIntoView(item);
            }
            if (containerItem == null)
            {
                this.itemsToUnselect.Remove(item);
                this.itemsToSelect.Add(item);
            }
            if (containerItem != null)
            {
                containerItem.IsSelected = true;
            }
        }

        private void SetContent(RadTileViewItem container, object item)
        {
            if (container.ContentTemplate == null)
            {
                if (this.ContentTemplate != null)
                {
                    container.ContentTemplate = this.ContentTemplate;
                    return;
                }
                if (this.ContentTemplateSelector != null)
                {
                    container.ContentTemplate = this.ContentTemplateSelector.SelectTemplate(item, container);
                }
            }
        }

        private void SetSelectedItem(object item)
        {
            int position = this.PositionToIndexMap.GetPositionFromIndex(base.Items.IndexOf(item));
            this.ExecuteRangeSelection(position, position);
            if (base.SelectedItem == null && this.SelectedItems.Count > 0)
            {
                base.SelectedItem = this.SelectedItems[this.SelectedItems.Count - 1];
            }
        }

        private void SwapItems(RadTileViewItem first, RadTileViewItem second)
        {
            if (first != null && second != null && first.TileState != TileViewItemState.Maximized && second.TileState != TileViewItemState.Maximized && first.MoveTransform != null && second.MoveTransform != null)
            {
                try
                {
                    this.hasItemsSwapped = true;
                    Point firstPos = first.TransformToVisual(this).Transform(new Point());
                    Point secondPos = second.TransformToVisual(this).Transform(new Point());
                    double offsetX = firstPos.X - secondPos.X;
                    double offsetY = firstPos.Y - secondPos.Y;
                    if (!this.IsItemsAnimationEnabled)
                    {
                        base.Dispatcher.BeginInvoke(() => this.SwappingItems.Remove(first));
                        this.TryRaiseTilesPositionChanged();
                    }
                    else
                    {
                        TranslateTransform moveTransform = second.MoveTransform;
                        moveTransform.X = moveTransform.X - offsetX;
                        TranslateTransform y = second.MoveTransform;
                        y.Y = y.Y - offsetY;
                        TranslateTransform x = first.MoveTransform;
                        x.X = x.X + offsetX;
                        TranslateTransform translateTransform = first.MoveTransform;
                        translateTransform.Y = translateTransform.Y + offsetY;
                        this.AnimateItemPosition(first);
                    }
                }
                catch
                {
                }
                if ((!this.IsDragging || this.DragMode == TileViewDragMode.Swap) && this.IsItemsAnimationEnabled)
                {
                    this.AnimateItemPosition(second);
                }
            }
        }

        private void SwapWithDraggingItem(RadTileViewItem item)
        {
            if (item != null && !this.SwappingItems.Contains(item))
            {
                if (this.DraggingItem.Position != item.Position)
                {
                    this.SwappingItems.Add(item);
                }
                this.DraggingItem.Position = item.Position;
            }
        }

        private void ToggleItemSelection(object item)
        {
            if (this.SelectedItems.Contains(item))
            {
                this.SelectedItems.Remove(item);
                this.RaiseSelectionChangedEvent(new List<object>()
				{
					item
				}, new List<object>());
                return;
            }
            this.SelectedItems.Add(item);
            this.lastSelecteIndex = base.Items.IndexOf(item);
            this.RaiseSelectionChangedEvent(new List<object>(), new List<object>()
			{
				item
			});
        }

        private void TryRaiseTilesPositionChanged()
        {
            if (this.SwappingItems.Count == 0 && !this.IsDragging)
            {
                this.RaiseEvent(new RadRoutedEventArgs(RadTileView.TilesPositionChangedEvent));
            }
        }

        internal void TryRaiseTilesStateChanged()
        {
            if (this.IsPreviewTilesStateRaised)
            {
                this.OnTilesStateChanged(new RadRoutedEventArgs(RadTileView.TilesStateChangedEvent));
                this.IsPreviewTilesStateRaised = false;
            }
        }

        internal void TryStartDragging(RadTileViewItem item, Point startPoint)
        {
            TileViewDragEventArgs e = new TileViewDragEventArgs(item, RadTileView.PreviewTileDragStartedEvent, this);
            this.OnPreviewTileDragStarted(e);
            if (e.Handled)
            {
                return;
            }
            if (this.CanDragItems)
            {
                this.lastMousePosition = startPoint;
                this.DraggingCandidate = item;
            }
        }

        private int TryToFindItemInCollection(object item)
        {
            for (int i = 0; i < base.Items.Count; i++)
            {
                if (base.Items[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }

        private void UnselectAllItems()
        {
            this.ExecuteRangeSelection(-1, -1);
        }

        private void UnselectItem(object item)
        {
            RadTileViewItem containerItem = item as RadTileViewItem ?? base.ItemContainerGenerator.ContainerFromItem(item) as RadTileViewItem;
            if (containerItem == null)
            {
                this.itemsToSelect.Remove(item);
                this.itemsToUnselect.Add(item);
            }
            if (containerItem != null)
            {
                containerItem.IsSelected = false;
            }
        }

        private void UpdateIsEnabledState()
        {
            if (base.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Normal", false);
                return;
            }
            VisualStateManager.GoToState(this, "Disabled", false);
        }

        internal void UpdatePositions()
        {
            if (this.IsPositionChanging)
            {
                return;
            }
            this.IsPositionChanging = true;
            for (int i = 0; i < this.VisibleContainers.Count; i++)
            {
                RadTileViewItem item = this.VisibleContainers[i];
                int index = this.GetIndexFromContainer(item);
                if (index >= 0 && index < base.Items.Count)
                {
                    item.Position = this.PositionToIndexMap.GetPositionFromIndex(this.GetIndexFromContainer(item));
                }
                if (this.IsHandledPositionChange)
                {
                    i = -1;
                    this.IsHandledPositionChange = false;
                    RadTileViewItem prevContainer = this.VisibleContainers.FirstOrDefault<RadTileViewItem>((RadTileViewItem c) =>
                    {
                        if (c.Position != item.Position)
                        {
                            return false;
                        }
                        return c != item;
                    });
                    if (prevContainer != null)
                    {
                        prevContainer.IsPositionReverted = true;
                    }
                }
            }
            this.IsPositionChanging = false;
        }

        private bool ValidateSelectionChange(object item, bool newValue)
        {
            if (!this.IsSelectionEnabled)
            {
                return false;
            }
            RadTileViewItem container = base.ItemContainerGenerator.ContainerFromItem(item) as RadTileViewItem;
            bool result = this.RaiseSelectionEvents(item, newValue);
            if (result && container != null)
            {
                container.RevertSelection(!newValue);
            }
            return !result;
        }

        internal event EventHandler InternalLayoutChanged;

        internal event EventHandler<Telerik.Windows.DragDrop.DragEventArgs> OnMoveDraggingItem;

        /// <summary>
        /// Occurs before a drag operation has started.
        /// </summary>
        public event EventHandler<TileViewDragEventArgs> PreviewTileDragStarted
        {
            add
            {
                this.AddHandler(RadTileView.PreviewTileDragStartedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileView.PreviewTileDragStartedEvent, value);
            }
        }

        /// <summary>
        /// Occurs before when the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.Position">Position</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is changed.
        /// </summary>
        /// <remarks>
        /// 	Use this event to detect when the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.Position">Position</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is going changed.
        /// </remarks>
        public event EventHandler<RadRoutedEventArgs> PreviewTilePositionChanged
        {
            add
            {
                this.AddHandler(RadTileViewItem.PreviewPositionChangedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileViewItem.PreviewPositionChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs before the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">Selection</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is changed.
        /// </summary>
        /// <remarks>
        /// 	Use this event to detect when the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">Selection</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is about to be changed changed.
        /// 	In cases when you need to prevent the Selection from changing, you can handle this event.
        /// </remarks>
        public event Telerik.Windows.Controls.SelectionChangedEventHandler PreviewTileSelectionChanged
        {
            add
            {
                this.AddHandler(RadTileViewItem.PreviewSelectionChangedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileViewItem.PreviewSelectionChangedEvent, value);
            }
        }

        /// <summary>
        /// Raised just before the selected items collection has changed.
        /// </summary>
        public event Telerik.Windows.Controls.SelectionChangedEventHandler PreviewTilesSelectionChanged
        {
            add
            {
                this.AddHandler(RadTileView.PreviewTilesSelectionChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(RadTileView.PreviewTilesSelectionChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs before all <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItems</see> have changed their <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see>.
        /// </summary>
        public event EventHandler<RadRoutedEventArgs> PreviewTilesStateChanged
        {
            add
            {
                this.AddHandler(RadTileView.PreviewTilesStateChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(RadTileView.PreviewTilesStateChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs before the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is changed.
        /// </summary>
        /// <remarks>
        /// 	Use this event to detect when the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is about to be changed changed.
        /// 	In cases when you need to prevent the TileState from changing, you can handle this event.
        /// </remarks>
        public event EventHandler<PreviewTileStateChangedEventArgs> PreviewTileStateChanged
        {
            add
            {
                this.AddHandler(RadTileViewItem.PreviewTileStateChangedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileViewItem.PreviewTileStateChangedEvent, value);
            }
        }

        internal event EventHandler<TileViewBringIntoViewArgs> RequestBringItemIntoView;

        /// <summary>
        /// Occurs after a drag operation has ended.
        /// </summary>
        public event EventHandler<TileViewDragEventArgs> TileDragEnded
        {
            add
            {
                this.AddHandler(RadTileView.TileDragEndedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileView.TileDragEndedEvent, value);
            }
        }

        /// <summary>
        /// Occurs after a drag operation has started.
        /// </summary>
        public event EventHandler<TileViewDragEventArgs> TileDragStarted
        {
            add
            {
                this.AddHandler(RadTileView.TileDragStartedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileView.TileDragStartedEvent, value);
            }
        }

        /// <summary>
        /// Occurs when the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.Position">Position</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is changed.
        /// </summary>
        /// <remarks>
        /// 	Use this event to detect when the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.Position">Position</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is changed.
        /// </remarks>
        public event EventHandler<RadRoutedEventArgs> TilePositionChanged
        {
            add
            {
                this.AddHandler(RadTileViewItem.PositionChangedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileViewItem.PositionChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs after some<see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItems</see> have changed their Selection.
        /// </summary>
        public event Telerik.Windows.Controls.SelectionChangedEventHandler TileSelectionChanged
        {
            add
            {
                this.AddHandler(RadTileViewItem.SelectionChangedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileViewItem.SelectionChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs after some<see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItems</see> have changed their Positions.
        /// </summary>
        public event EventHandler<RadRoutedEventArgs> TilesPositionChanged
        {
            add
            {
                this.AddHandler(RadTileView.TilesPositionChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(RadTileView.TilesPositionChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs after some<see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItems</see> have changed their Selection.
        /// </summary>
        public event Telerik.Windows.Controls.SelectionChangedEventHandler TilesSelectionChanged
        {
            add
            {
                this.AddHandler(RadTileView.TilesSelectionChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(RadTileView.TilesSelectionChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs after all <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItems</see> have changed their <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see>.
        /// </summary>
        public event EventHandler<RadRoutedEventArgs> TilesStateChanged
        {
            add
            {
                this.AddHandler(RadTileView.TilesStateChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(RadTileView.TilesStateChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs when the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is changed.
        /// </summary>
        /// <remarks>
        /// 	Use this event to detect when the <see cref="P:Telerik.Windows.Controls.RadTileViewItem.TileState">TileState</see> of a <see cref="T:Telerik.Windows.Controls.RadTileViewItem">RadTileViewItem</see> is changed.
        /// </remarks>
        public event EventHandler<RadRoutedEventArgs> TileStateChanged
        {
            add
            {
                this.AddHandler(RadTileViewItem.TileStateChangedEvent, value, false);
            }
            remove
            {
                this.RemoveHandler(RadTileViewItem.TileStateChangedEvent, value);
            }
        }
    }
}
