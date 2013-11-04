using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Win8XamlControlPack.Primitives
{
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

	public abstract class Selector : ItemsControl
	{

        //static Selector()
        //{
        //    Selector.SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(Selector), new Telerik.Windows.PropertyMetadata(new PropertyChangedCallback(Selector.OnSelectedValuePathChanged)));
        //    Selector.SelectedValueProperty = DependencyPropertyExtensions.Register("SelectedValue", typeof(object), typeof(Selector), new Telerik.Windows.PropertyMetadata(null, new PropertyChangedCallback(Selector.OnSelectedValueChanged), new CoerceValueCallback(Selector.OnCoerceSelectedValue)));
        //    Selector.IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(Selector), new Telerik.Windows.PropertyMetadata(new PropertyChangedCallback(Selector.OnIsSelectedChanged)));
        //    Selector.SelectedIndexProperty = DependencyPropertyExtensions.Register("SelectedIndex", typeof(int), typeof(Selector), new Telerik.Windows.PropertyMetadata((object)-1, new PropertyChangedCallback(Selector.OnSelectedIndexChanged), new CoerceValueCallback(Selector.CoerceSelectedIndex)), new ValidateValueCallback(Selector.ValidateSelectedIndex));
        //    Selector.SelectedItemProperty = DependencyPropertyExtensions.Register("SelectedItem", typeof(object), typeof(Selector), new Telerik.Windows.PropertyMetadata(null, new PropertyChangedCallback(Selector.OnSelectedItemChanged), new CoerceValueCallback(Selector.CoerceSelectedItem)));
        //    Selector.SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selector));
        //    Selector.SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(Telerik.Windows.Controls.SelectionChangedEventHandler), typeof(Selector));
        //    Selector.UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selector));
        //    EventManager.RegisterClassHandler(typeof(Selector), Selector.SelectedEvent, new RoutedEventHandler(Selector.OnSelected));
        //    EventManager.RegisterClassHandler(typeof(Selector), Selector.UnselectedEvent, new RoutedEventHandler(Selector.OnUnselected));
        //}
        public readonly static DependencyProperty SelectedValuePathProperty;
		public readonly static DependencyProperty SelectedValueProperty;
		public readonly static DependencyProperty IsSelectedProperty;
		public readonly static DependencyProperty SelectedIndexProperty;
		public readonly static DependencyProperty SelectedItemProperty;
		public readonly static RoutedEvent SelectedEvent;
		public readonly static RoutedEvent SelectionChangedEvent;
		public readonly static RoutedEvent UnselectedEvent;
		private int deferredSelectedIndex = -1;
		private SelectionChanger<object> selectedItems;
		private bool skipCoerceSelectedItemCheck;
		private bool selectingItemWithValue;
		internal bool CanSelectMultiple { get; set; }

		internal int InternalSelectedIndex
		{
			get { return selectedItems.Count == 0 ? -1 : Items.IndexOf(selectedItems[0]); }
		}

		internal object InternalSelectedItem
		{
			get { return selectedItems.Count == 0 ? null : selectedItems[0]; }
		}

		private object InternalSelectedValue
		{
			get
			{
				object internalSelectedItem = InternalSelectedItem;
				if (internalSelectedItem == null)
				{
					return null;
				}
				if (SelectedValuePath == null)
				{
					return internalSelectedItem;
				}
				return BindingExpressionHelper.GetValue(internalSelectedItem, SelectedValuePath);
			}
		}

		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
		}

		public object SelectedItem
		{
			get
			{
				return base.GetValue(Selector.SelectedItemProperty);
			}
			set
			{
				base.SetValue(Selector.SelectedItemProperty, value);
			}
		}

		public object SelectedValue
		{
			get
			{
				return base.GetValue(Selector.SelectedValueProperty);
			}
			set
			{
				base.SetValue(Selector.SelectedValueProperty, value);
			}
		}

		public string SelectedValuePath
		{
			get
			{
				return (string)base.GetValue(Selector.SelectedValuePathProperty);
			}
			set
			{
				base.SetValue(Selector.SelectedValuePathProperty, value);
			}
		}

		internal Selector.SelectionChanger<object> SelectionChange
		{
			get
			{
				return this.selectedItems;
			}
		}



		internal Selector()
		{
			this.SelectedIndex = -1;
			this.selectedItems = new Selector.SelectionChanger<object>(this);
			this.selectedItems.SelectionChanged += this.OnSelectionChanged;
		}

		public static void AddSelectedHandler(DependencyObject element, RoutedEventHandler handler)
		{
			element.AddHandler(Selector.SelectedEvent, handler);
		}

		public static void AddUnselectedHandler(DependencyObject element, RoutedEventHandler handler)
		{
			element.AddHandler(Selector.UnselectedEvent, handler);
		}

		private static object CoerceSelectedIndex(DependencyObject d, object value)
		{
			Selector selector = (Selector)d;
			if (!(value is int) || (int)value < selector.Items.Count)
			{
				selector.deferredSelectedIndex = -1;
				return value;
			}
			selector.deferredSelectedIndex = (int)value;
			return -1;
		}

		private static object CoerceSelectedItem(DependencyObject d, object value)
		{
			Selector selector = (Selector)d;
			if (value != null && !selector.skipCoerceSelectedItemCheck)
			{
				int selectedIndex = selector.SelectedIndex;
				if (selectedIndex > -1 && selectedIndex < selector.Items.Count && selector.Items[selectedIndex] == value)
				{
					return value;
				}
				if (!selector.Items.Contains(value))
				{
					return null;
				}
			}
			return value;
		}

		private object CoerceSelectedValue(object value)
		{
			if (this.SelectionChange.IsActive)
			{
				this.selectingItemWithValue = false;
				return value;
			}
			if (this.SelectItemWithValue(value) == null && base.HasItems)
			{
				value = null;
			}
			return value;
		}

		private static bool ContainerGetIsSelected(DependencyObject container, object item)
		{
			if (container != null)
			{
				return (bool)container.GetValue(Selector.IsSelectedProperty);
			}
			DependencyObject obj2 = item as DependencyObject;
			if (obj2 == null)
			{
				return false;
			}
			return (bool)obj2.GetValue(Selector.IsSelectedProperty);
		}

		private object FindItemWithValue(object value)
		{
			ItemSearch itemSearch = new ItemSearch(base.Items, this.SelectedValuePath, null, (object i) => true);
			return itemSearch.FindItem((object itemValue) => object.Equals(itemValue, value));
		}

		/// <summary>
		/// Gets the value of the IsSelected attached property that indicates whether an item is selected.
		/// </summary>
		/// <param name="element">Object to query concerning the IsSelected property.</param>
		/// <returns>Boolean value, true if the IsSelected property is true.</returns>
		public static bool GetIsSelected(DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return (bool)element.GetValue(Selector.IsSelectedProperty);
		}

		private static object GetItemOrContainerFromContainer(DependencyObject container)
		{
			object itemOrContainer = container;
			Telerik.Windows.Controls.ItemsControl owner = Telerik.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(container);
			if (owner != null)
			{
				itemOrContainer = owner.ItemContainerGenerator.ItemFromContainer(container);
			}
			return itemOrContainer;
		}

		private bool IndexGetIsSelected(int index, object item)
		{
			return Selector.ContainerGetIsSelected(base.ItemContainerGenerator.ContainerFromIndex(index), item);
		}

		internal static bool ItemGetIsSelectable(object item)
		{
			return item != null;
		}

		private bool ItemGetIsSelected(object item)
		{
			if (item == null)
			{
				return false;
			}
			return Selector.ContainerGetIsSelected(base.ItemContainerGenerator.ContainerFromItem(item), item);
		}

		private void ItemSetIsSelected(object item, bool value)
		{
			if (item != null)
			{
				DependencyObject element = base.ItemContainerGenerator.ContainerFromItem(item) ?? item as DependencyObject;
				if (element != null && Selector.GetIsSelected(element) != value)
				{
					element.SetValue(Selector.IsSelectedProperty, value);
				}
			}
		}

		internal void NotifyIsSelectedChanged(FrameworkElement container, bool selected, RadRoutedEventArgs e)
		{
			if (this.SelectionChange.IsActive)
			{
				e.Handled = true;
			}
			else if (container != null)
			{
				object itemOrContainerFromContainer = Selector.GetItemOrContainerFromContainer(container);
				if (itemOrContainerFromContainer != DependencyProperty.UnsetValue)
				{
					this.SetSelectedHelper(itemOrContainerFromContainer, selected);
					e.Handled = true;
					return;
				}
			}
		}

		private static object OnCoerceSelectedValue(DependencyObject sender, object newValue)
		{
			return (sender as Selector).CoerceSelectedValue(newValue);
		}

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ISelectable selectable = d as ISelectable;
			if (selectable != null)
			{
				selectable.IsSelected = (bool)e.NewValue;
				if ((bool)e.NewValue)
				{
					selectable.OnSelected(new RadRoutedEventArgs(Selector.SelectedEvent, d));
					return;
				}
				selectable.OnUnselected(new RadRoutedEventArgs(Selector.UnselectedEvent, d));
			}
		}

		/// <summary>
		/// Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property changes.
		/// </summary>
		/// <param name="e">Information about the change.</param>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			object[] action;
			base.OnItemsChanged(e);
			if (this.deferredSelectedIndex != -1)
			{
				this.SelectedIndex = this.deferredSelectedIndex;
			}
			if (this.SelectedValue != null && this.SelectedIndex == -1 && !object.Equals(this.SelectedValue, this.InternalSelectedValue))
			{
				this.SelectItemWithValue(this.SelectedValue);
			}
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.SelectionChange.Begin();
					try
					{
						object item = e.NewItems[0];
						if (this.ItemGetIsSelected(item))
						{
							this.SelectionChange.Add(item);
						}
						break;
					}
					finally
					{
						this.SelectionChange.End();
					}
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.SelectionChange.Begin();
					try
					{
						object item = e.OldItems[0];
						if (this.selectedItems.Contains(item))
						{
							this.SelectionChange.Remove(item);
						}
						break;
					}
					finally
					{
						this.SelectionChange.End();
					}
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				{
					this.SelectionChange.Begin();
					try
					{
						object oldItem = e.OldItems[0];
						if (this.selectedItems.Contains(oldItem))
						{
							this.SelectionChange.Remove(oldItem);
						}
						object newItem = e.NewItems[0];
						if (this.ItemGetIsSelected(newItem))
						{
							this.SelectionChange.Add(newItem);
						}
						break;
					}
					finally
					{
						this.SelectionChange.End();
					}
					break;
				}
				case NotifyCollectionChangedAction.Remove | NotifyCollectionChangedAction.Replace:
				{
					action = new object[] { e.Action };
					throw new NotSupportedException(Telerik.Windows.Controls.SR.Get("UnexpectedCollectionChangeAction", action));
				}
				case NotifyCollectionChangedAction.Reset:
				{
					if (base.Items.Count == 0)
					{
						this.SelectionChange.Clear();
					}
					this.SelectionChange.Begin();
					try
					{
						for (int i = 0; i < this.selectedItems.Count; i++)
						{
							object item = this.selectedItems[i];
							if (!base.Items.Contains(item))
							{
								this.SelectionChange.Remove(item);
							}
						}
						if (base.ItemsSource == null)
						{
							for (int j = 0; j < base.Items.Count; j++)
							{
								object item2 = base.Items[j];
								if (this.IndexGetIsSelected(j, item2) && !this.selectedItems.Contains(item2))
								{
									this.SelectionChange.Add(item2);
								}
							}
						}
						break;
					}
					finally
					{
						this.SelectionChange.End();
					}
					break;
				}
				default:
				{
					action = new object[] { e.Action };
					throw new NotSupportedException(Telerik.Windows.Controls.SR.Get("UnexpectedCollectionChangeAction", action));
				}
			}
		}

		private static void OnSelected(object sender, RoutedEventArgs e)
		{
			RadRoutedEventArgs args = e as RadRoutedEventArgs;
			if (args != null)
			{
				((Selector)sender).NotifyIsSelectedChanged(args.OriginalSource as FrameworkElement, true, args);
			}
		}

		private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			object obj;
			Selector selector = (Selector)d;
			if (selector.SelectionChange != null && !selector.SelectionChange.IsActive)
			{
				int newValue = (int)d.GetValue(e.Property);
				obj = (newValue <= -1 || newValue >= selector.Items.Count ? null : selector.Items[newValue]);
				selector.SelectionChange.SelectJustThisItem(obj);
			}
		}

		private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Selector selector = (Selector)d;
			if (!selector.SelectionChange.IsActive)
			{
				selector.SelectionChange.SelectJustThisItem(d.GetValue(e.Property));
			}
		}

		/// <summary>
		/// Called when the SelectedValue property is changed.
		/// </summary>
		/// <param name="oldValue">The old value of the SelectedValue property.</param>
		/// <param name="newValue">The new value of the SelectedValue property.</param>
		protected virtual void OnSelectedValueChanged(object oldValue, object newValue)
		{
		}

		private static void OnSelectedValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			(sender as Selector).OnSelectedValueChanged(e.OldValue, sender.GetValue(e.Property));
		}

		/// <summary>
		/// Called when the SelectedValuePath property is changed.
		/// </summary>
		/// <param name="oldValue">The old value of the SelectedValuePath property.</param>
		/// <param name="newValue">The new value of the SelectedValuePath property.</param>
		protected virtual void OnSelectedValuePathChanged(string oldValue, string newValue)
		{
			if (this.SelectedValue != null)
			{
				this.SelectItemWithValue(this.SelectedValue);
			}
		}

		private static void OnSelectedValuePathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			(sender as Selector).OnSelectedValuePathChanged((string)e.OldValue, (string)e.NewValue);
		}

		/// <summary>
		/// Called when the selection changes.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected virtual void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.RaiseEvent(new Telerik.Windows.Controls.SelectionChangedEventArgs(Selector.SelectionChangedEvent, e.RemovedItems, e.AddedItems));
		}

		private void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.OnSelectionChanged(e);
		}

		private static void OnUnselected(object sender, RoutedEventArgs e)
		{
			RadRoutedEventArgs args = e as RadRoutedEventArgs;
			if (args != null)
			{
				((Selector)sender).NotifyIsSelectedChanged(args.OriginalSource as FrameworkElement, false, args);
			}
		}

		/// <summary>
		/// Removes a handler for the Selected attached event.
		/// </summary>
		/// <param name="element">Element that listens to this event.</param>
		/// <param name="handler">Event handler to remove.</param>
		public static void RemoveSelectedHandler(DependencyObject element, RoutedEventHandler handler)
		{
			element.RemoveHandler(Selector.SelectedEvent, handler);
		}

		/// <summary>
		/// Removes a handler for the Unselected attached event.
		/// </summary>
		/// <param name="element">Element that listens to this event.</param>
		/// <param name="handler">Event handler to remove.</param>
		public static void RemoveUnselectedHandler(DependencyObject element, RoutedEventHandler handler)
		{
			element.RemoveHandler(Selector.UnselectedEvent, handler);
		}

		private object SelectItemWithValue(object value)
		{
			this.selectingItemWithValue = true;
			object unsetValue = null;
			if (value != null)
			{
				unsetValue = this.FindItemWithValue(value);
			}
			if (this.SelectedItem != unsetValue)
			{
				this.SelectionChange.SelectJustThisItem(unsetValue);
			}
			if (unsetValue != null || base.HasItems)
			{
				this.selectingItemWithValue = false;
			}
			return unsetValue;
		}

		/// <summary>
		/// Sets a property value that indicates whether an item in a Selector is selected.
		/// </summary>
		/// <param name="element">Object on which to set the property.</param>
		/// <param name="isSelected">Value to set.</param>
		public static void SetIsSelected(DependencyObject element, bool isSelected)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			element.SetValue(Selector.IsSelectedProperty, isSelected);
		}

		private void SetSelectedHelper(object item, bool selected)
		{
			if (!Selector.ItemGetIsSelectable(item) && selected)
			{
				throw new InvalidOperationException(Telerik.Windows.Controls.SR.Get("CannotSelectNotSelectableItem", new object[0]));
			}
			this.SelectionChange.Begin();
			try
			{
				if (!selected)
				{
					this.SelectionChange.Remove(item);
				}
				else
				{
					this.SelectionChange.Add(item);
				}
			}
			finally
			{
				this.SelectionChange.End();
			}
		}

		internal static bool UIGetIsSelectable(DependencyObject item)
		{
			if (item != null)
			{
				if (!Selector.ItemGetIsSelectable(item))
				{
					return false;
				}
				Telerik.Windows.Controls.ItemsControl control = Telerik.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(item);
				if (control != null)
				{
					object itemFromContainer = control.ItemContainerGenerator.ItemFromContainer(item);
					if (itemFromContainer == item)
					{
						return true;
					}
					return Selector.ItemGetIsSelectable(itemFromContainer);
				}
			}
			return false;
		}

		internal virtual void UpdatePublicSelectionProperties()
		{
			int oldSelectedIndex = this.SelectedIndex;
			if (oldSelectedIndex > base.Items.Count - 1 || oldSelectedIndex == -1 && this.selectedItems.Count > 0 || oldSelectedIndex > -1 && this.selectedItems.Count == 0)
			{
				this.deferredSelectedIndex = oldSelectedIndex;
			}
			if (this.SelectedIndex != this.InternalSelectedIndex)
			{
				base.SetValue(Selector.SelectedIndexProperty, this.InternalSelectedIndex);
			}
			if (this.SelectedItem != this.InternalSelectedItem)
			{
				try
				{
					this.skipCoerceSelectedItemCheck = true;
					base.SetValue(Selector.SelectedItemProperty, this.InternalSelectedItem);
				}
				finally
				{
					this.skipCoerceSelectedItemCheck = false;
				}
			}
			if (!this.selectingItemWithValue && this.SelectedValue != this.InternalSelectedValue)
			{
				base.SetValue(Selector.SelectedValueProperty, this.InternalSelectedValue);
			}
		}

		private static bool ValidateSelectedIndex(object o)
		{
			return (int)o >= -1;
		}

		/// <summary>
		/// Occurs when the selection of a Selector changes.
		/// </summary>
		[Telerik.Windows.Controls.SRCategory("Behavior")]
		public event Telerik.Windows.Controls.SelectionChangedEventHandler SelectionChanged
		{
			add
			{
				this.AddHandler(Selector.SelectionChangedEvent, value);
			}
			remove
			{
				this.RemoveHandler(Selector.SelectionChangedEvent, value);
			}
		}

		internal class SelectionChanger<T> : ObservableCollection<T>
		{
			private Func<T, bool> isItemValidForSelection;

			private List<T> itemsToSelect;

			private List<T> itemsToUnselect;

			private ObservableCollection<T> internalSelection;

			private bool isActive;

			private Selector selector;

			internal bool IsActive
			{
				get
				{
					return this.isActive;
				}
			}

			public SelectionChanger(Selector owner, Func<T, bool> isItemValidForSelection) : this(owner)
			{
				this.isItemValidForSelection = isItemValidForSelection;
			}

			public SelectionChanger(Selector owner)
			{
				this.selector = owner;
				this.itemsToSelect = new List<T>(1);
				this.itemsToUnselect = new List<T>(1);
				this.internalSelection = new ObservableCollection<T>();
				this.InitFlags();
			}

			private bool AddToSelection(T item)
			{
				bool result = true;
				if (this.isItemValidForSelection != null)
				{
					result = this.isItemValidForSelection(item);
				}
				if (result && this.ItemIsSelectable(item))
				{
					this.itemsToSelect.Add(item);
				}
				return result;
			}

			internal void Begin()
			{
				this.isActive = true;
				this.itemsToSelect.Clear();
				this.itemsToUnselect.Clear();
			}

			internal void Cancel()
			{
				this.InitFlags();
				this.itemsToSelect.Clear();
				this.itemsToUnselect.Clear();
			}

			protected override void ClearItems()
			{
				if (this.isActive)
				{
					this.InternalClear();
					return;
				}
				this.Begin();
				this.InternalClear();
				this.End();
			}

			internal void End()
			{
				this.SynchronizeInternalSelection();
				this.selector.UpdatePublicSelectionProperties();
				this.InitFlags();
				if (this.itemsToUnselect.Count > 0 || this.itemsToSelect.Count > 0)
				{
					this.InvokeSelectionChangedEvent();
				}
				this.itemsToSelect.Clear();
				this.itemsToUnselect.Clear();
			}

			private void InitFlags()
			{
				this.isActive = false;
			}

			protected override void InsertItem(int index, T item)
			{
				if (this.isActive)
				{
					this.Select(index, item);
					return;
				}
				this.Begin();
				this.Select(index, item);
				this.End();
			}

			protected void InternalClear()
			{
				base.ClearItems();
				this.itemsToSelect.Clear();
				this.itemsToUnselect.AddRange(this.internalSelection);
				this.internalSelection.Clear();
			}

			private void InvokeSelectionChangedEvent()
			{
				if (this.SelectionChanged != null)
				{
					System.Windows.Controls.SelectionChangedEventArgs e = new System.Windows.Controls.SelectionChangedEventArgs(this.itemsToUnselect, this.itemsToSelect);
					this.SelectionChanged(this.selector, e);
				}
			}

			private bool ItemIsSelectable(T item)
			{
				if (item == null || (object)item == DependencyProperty.UnsetValue)
				{
					return false;
				}
				return this.selector.Items.Contains(item);
			}

			protected override void RemoveItem(int index)
			{
				T changedItem = base[index];
				if (this.isActive)
				{
					this.Unselect(index, changedItem);
					return;
				}
				this.Begin();
				this.Unselect(index, changedItem);
				this.End();
			}

			private void Select(int index, T item)
			{
				if (this.itemsToUnselect.Remove(item))
				{
					if (!base.Items.Contains(item))
					{
						base.InsertItem(index, item);
						this.internalSelection.Insert(index, item);
					}
					return;
				}
				if (this.itemsToSelect.Contains(item))
				{
					return;
				}
				if (!this.internalSelection.Contains(item))
				{
					this.AddToSelection(item);
				}
				if (!this.selector.CanSelectMultiple && this.itemsToSelect.Count > 0)
				{
					this.itemsToUnselect.AddRange(this.internalSelection);
				}
			}

			internal void SelectJustThisItem(T item)
			{
				this.Begin();
				if (base.Items.Count > 0)
				{
					base.Clear();
				}
				try
				{
					if (this.ItemIsSelectable(item))
					{
						if (!this.itemsToUnselect.Contains(item))
						{
							this.Select(0, item);
						}
						else
						{
							if (!base.Items.Contains(item))
							{
								base.InsertItem(0, item);
								this.internalSelection.Insert(0, item);
							}
							this.itemsToUnselect.Remove(item);
						}
					}
				}
				finally
				{
					this.End();
				}
			}

			protected override void SetItem(int index, T item)
			{
				bool newItemIsSelected = this.internalSelection.Contains(item);
				if (item == null || newItemIsSelected)
				{
					base.RemoveAt(index);
					return;
				}
				this.Begin();
				T oldItem = base[index];
				bool added = this.AddToSelection(item);
				if (added)
				{
					this.internalSelection[index] = item;
					base.SetItem(index, item);
				}
				if (!base.Items.Contains(oldItem))
				{
					this.itemsToUnselect.Add(oldItem);
				}
				this.SynchronizeInternalSelection(!added);
				this.selector.UpdatePublicSelectionProperties();
				if (this.itemsToSelect.Count > 0 || this.itemsToUnselect.Count > 0)
				{
					this.InvokeSelectionChangedEvent();
				}
				this.Cancel();
			}

			private void SynchronizeInternalSelection(bool updateInternalSelection)
			{
				foreach (T item in this.itemsToUnselect)
				{
					this.selector.ItemSetIsSelected(item, false);
					if (!updateInternalSelection)
					{
						continue;
					}
					int index = base.IndexOf(item);
					if (index > -1)
					{
						base.RemoveItem(index);
					}
					this.internalSelection.Remove(item);
				}
				foreach (T item in this.itemsToSelect)
				{
					this.selector.ItemSetIsSelected(item, true);
					if (!updateInternalSelection)
					{
						continue;
					}
					base.InsertItem(base.Items.Count, item);
					this.internalSelection.Add(item);
				}
			}

			private void SynchronizeInternalSelection()
			{
				this.SynchronizeInternalSelection(true);
			}

			private void Unselect(int index, T item)
			{
				if (this.itemsToSelect.Remove(item))
				{
					if (base.Items.Contains(item))
					{
						base.RemoveItem(index);
						this.internalSelection.RemoveAt(index);
					}
					return;
				}
				if (this.itemsToUnselect.Contains(item))
				{
					return;
				}
				if (this.internalSelection.Contains(item))
				{
					this.itemsToUnselect.Add(item);
				}
			}

			public event System.Windows.Controls.SelectionChangedEventHandler SelectionChanged;
		}
	}
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Telerik.Windows.Controls
{
	/// <summary>
	/// Represents a control that allows a user to select items from among its child elements.
	/// </summary>
	public abstract class Selector : Telerik.Windows.Controls.ItemsControl
	{
		/// <summary>
		/// Identifies the SelectedValuePath dependency property.
		/// </summary>
		public readonly static DependencyProperty SelectedValuePathProperty;

		/// <summary>
		/// Identifies the SelectedValue dependency property.
		/// </summary>
		public readonly static DependencyProperty SelectedValueProperty;

		/// <summary>
		/// Identifies the IsSelected attached property. 
		/// </summary>
		public readonly static DependencyProperty IsSelectedProperty;

		/// <summary>
		/// Identifies the SelectedIndex dependency property. 
		/// </summary>
		public readonly static DependencyProperty SelectedIndexProperty;

		/// <summary>
		/// Identifies the SelectedItem dependency property. 
		/// </summary>
		public readonly static DependencyProperty SelectedItemProperty;

		/// <summary>
		/// Identifies the Selected routed event.
		/// </summary>
		public readonly static Telerik.Windows.RoutedEvent SelectedEvent;

		/// <summary>
		/// Identifies the SelectionChanged routed event. 
		/// </summary>
		public readonly static Telerik.Windows.RoutedEvent SelectionChangedEvent;

		/// <summary>
		/// Identifies the Unselected routed event. 
		/// </summary>
		public readonly static Telerik.Windows.RoutedEvent UnselectedEvent;

		private int deferredSelectedIndex = -1;

		private Selector.SelectionChanger<object> selectedItems;

		private bool skipCoerceSelectedItemCheck;

		private bool selectingItemWithValue;

		internal bool CanSelectMultiple
		{
			get;
			set;
		}

		internal int InternalSelectedIndex
		{
			get
			{
				if (this.selectedItems.Count == 0)
				{
					return -1;
				}
				return base.Items.IndexOf(this.selectedItems[0]);
			}
		}

		internal object InternalSelectedItem
		{
			get
			{
				if (this.selectedItems.Count == 0)
				{
					return null;
				}
				return this.selectedItems[0];
			}
		}

		private object InternalSelectedValue
		{
			get
			{
				object internalSelectedItem = this.InternalSelectedItem;
				if (internalSelectedItem == null)
				{
					return null;
				}
				if (this.SelectedValuePath == null)
				{
					return internalSelectedItem;
				}
				return BindingExpressionHelper.GetValue(internalSelectedItem, this.SelectedValuePath);
			}
		}

		/// <summary>
		/// Gets or sets the index of the first item in the current selection or returns negative one (-1) if the selection is empty. This is a dependency property.
		/// </summary>
		[Telerik.Windows.Controls.SRCategory("Appearance")]
		public int SelectedIndex
		{
			get
			{
				return (int)base.GetValue(Selector.SelectedIndexProperty);
			}
			set
			{
				base.SetValue(Selector.SelectedIndexProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the first item in the current selection or returns null if the selection is empty. This is a dependency property.
		/// </summary>
		[Telerik.Windows.Controls.SRCategory("Appearance")]
		public object SelectedItem
		{
			get
			{
				return base.GetValue(Selector.SelectedItemProperty);
			}
			set
			{
				base.SetValue(Selector.SelectedItemProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets ... This is a dependency property.
		/// </summary>
		public object SelectedValue
		{
			get
			{
				return base.GetValue(Selector.SelectedValueProperty);
			}
			set
			{
				base.SetValue(Selector.SelectedValueProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets ... This is a dependency property.
		/// </summary>
		public string SelectedValuePath
		{
			get
			{
				return (string)base.GetValue(Selector.SelectedValuePathProperty);
			}
			set
			{
				base.SetValue(Selector.SelectedValuePathProperty, value);
			}
		}

		internal Selector.SelectionChanger<object> SelectionChange
		{
			get
			{
				return this.selectedItems;
			}
		}

		static Selector()
		{
			Selector.SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(Selector), new Telerik.Windows.PropertyMetadata(new PropertyChangedCallback(Selector.OnSelectedValuePathChanged)));
			Selector.SelectedValueProperty = DependencyPropertyExtensions.Register("SelectedValue", typeof(object), typeof(Selector), new Telerik.Windows.PropertyMetadata(null, new PropertyChangedCallback(Selector.OnSelectedValueChanged), new CoerceValueCallback(Selector.OnCoerceSelectedValue)));
			Selector.IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(Selector), new Telerik.Windows.PropertyMetadata(new PropertyChangedCallback(Selector.OnIsSelectedChanged)));
			Selector.SelectedIndexProperty = DependencyPropertyExtensions.Register("SelectedIndex", typeof(int), typeof(Selector), new Telerik.Windows.PropertyMetadata((object)-1, new PropertyChangedCallback(Selector.OnSelectedIndexChanged), new CoerceValueCallback(Selector.CoerceSelectedIndex)), new ValidateValueCallback(Selector.ValidateSelectedIndex));
			Selector.SelectedItemProperty = DependencyPropertyExtensions.Register("SelectedItem", typeof(object), typeof(Selector), new Telerik.Windows.PropertyMetadata(null, new PropertyChangedCallback(Selector.OnSelectedItemChanged), new CoerceValueCallback(Selector.CoerceSelectedItem)));
			Selector.SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selector));
			Selector.SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(Telerik.Windows.Controls.SelectionChangedEventHandler), typeof(Selector));
			Selector.UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selector));
			EventManager.RegisterClassHandler(typeof(Selector), Selector.SelectedEvent, new RoutedEventHandler(Selector.OnSelected));
			EventManager.RegisterClassHandler(typeof(Selector), Selector.UnselectedEvent, new RoutedEventHandler(Selector.OnUnselected));
		}

		internal Selector()
		{
			this.SelectedIndex = -1;
			this.selectedItems = new Selector.SelectionChanger<object>(this);
			this.selectedItems.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.OnSelectionChanged);
		}

		/// <summary>
		/// Adds a handler for the Selected attached event. 
		/// </summary>
		/// <param name="element">Element that listens to this event.</param>
		/// <param name="handler">Event handler to add.</param>
		public static void AddSelectedHandler(DependencyObject element, RoutedEventHandler handler)
		{
			element.AddHandler(Selector.SelectedEvent, handler);
		}

		/// <summary>
		/// Adds a handler for the Unselected attached event. 
		/// </summary>
		/// <param name="element">Element that listens to this event.</param>
		/// <param name="handler">Event handler to add.</param>
		public static void AddUnselectedHandler(DependencyObject element, RoutedEventHandler handler)
		{
			element.AddHandler(Selector.UnselectedEvent, handler);
		}

		private static object CoerceSelectedIndex(DependencyObject d, object value)
		{
			Selector selector = (Selector)d;
			if (!(value is int) || (int)value < selector.Items.Count)
			{
				selector.deferredSelectedIndex = -1;
				return value;
			}
			selector.deferredSelectedIndex = (int)value;
			return -1;
		}

		private static object CoerceSelectedItem(DependencyObject d, object value)
		{
			Selector selector = (Selector)d;
			if (value != null && !selector.skipCoerceSelectedItemCheck)
			{
				int selectedIndex = selector.SelectedIndex;
				if (selectedIndex > -1 && selectedIndex < selector.Items.Count && selector.Items[selectedIndex] == value)
				{
					return value;
				}
				if (!selector.Items.Contains(value))
				{
					return null;
				}
			}
			return value;
		}

		private object CoerceSelectedValue(object value)
		{
			if (this.SelectionChange.IsActive)
			{
				this.selectingItemWithValue = false;
				return value;
			}
			if (this.SelectItemWithValue(value) == null && base.HasItems)
			{
				value = null;
			}
			return value;
		}

		private static bool ContainerGetIsSelected(DependencyObject container, object item)
		{
			if (container != null)
			{
				return (bool)container.GetValue(Selector.IsSelectedProperty);
			}
			DependencyObject obj2 = item as DependencyObject;
			if (obj2 == null)
			{
				return false;
			}
			return (bool)obj2.GetValue(Selector.IsSelectedProperty);
		}

		private object FindItemWithValue(object value)
		{
			ItemSearch itemSearch = new ItemSearch(base.Items, this.SelectedValuePath, null, (object i) => true);
			return itemSearch.FindItem((object itemValue) => object.Equals(itemValue, value));
		}

		/// <summary>
		/// Gets the value of the IsSelected attached property that indicates whether an item is selected.
		/// </summary>
		/// <param name="element">Object to query concerning the IsSelected property.</param>
		/// <returns>Boolean value, true if the IsSelected property is true.</returns>
		public static bool GetIsSelected(DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return (bool)element.GetValue(Selector.IsSelectedProperty);
		}

		private static object GetItemOrContainerFromContainer(DependencyObject container)
		{
			object itemOrContainer = container;
			Telerik.Windows.Controls.ItemsControl owner = Telerik.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(container);
			if (owner != null)
			{
				itemOrContainer = owner.ItemContainerGenerator.ItemFromContainer(container);
			}
			return itemOrContainer;
		}

		private bool IndexGetIsSelected(int index, object item)
		{
			return Selector.ContainerGetIsSelected(base.ItemContainerGenerator.ContainerFromIndex(index), item);
		}

		internal static bool ItemGetIsSelectable(object item)
		{
			return item != null;
		}

		private bool ItemGetIsSelected(object item)
		{
			if (item == null)
			{
				return false;
			}
			return Selector.ContainerGetIsSelected(base.ItemContainerGenerator.ContainerFromItem(item), item);
		}

		private void ItemSetIsSelected(object item, bool value)
		{
			if (item != null)
			{
				DependencyObject element = base.ItemContainerGenerator.ContainerFromItem(item) ?? item as DependencyObject;
				if (element != null && Selector.GetIsSelected(element) != value)
				{
					element.SetValue(Selector.IsSelectedProperty, value);
				}
			}
		}

		internal void NotifyIsSelectedChanged(FrameworkElement container, bool selected, RadRoutedEventArgs e)
		{
			if (this.SelectionChange.IsActive)
			{
				e.Handled = true;
			}
			else if (container != null)
			{
				object itemOrContainerFromContainer = Selector.GetItemOrContainerFromContainer(container);
				if (itemOrContainerFromContainer != DependencyProperty.UnsetValue)
				{
					this.SetSelectedHelper(itemOrContainerFromContainer, selected);
					e.Handled = true;
					return;
				}
			}
		}

		private static object OnCoerceSelectedValue(DependencyObject sender, object newValue)
		{
			return (sender as Selector).CoerceSelectedValue(newValue);
		}

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ISelectable selectable = d as ISelectable;
			if (selectable != null)
			{
				selectable.IsSelected = (bool)e.NewValue;
				if ((bool)e.NewValue)
				{
					selectable.OnSelected(new RadRoutedEventArgs(Selector.SelectedEvent, d));
					return;
				}
				selectable.OnUnselected(new RadRoutedEventArgs(Selector.UnselectedEvent, d));
			}
		}

		/// <summary>
		/// Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property changes.
		/// </summary>
		/// <param name="e">Information about the change.</param>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			object[] action;
			base.OnItemsChanged(e);
			if (this.deferredSelectedIndex != -1)
			{
				this.SelectedIndex = this.deferredSelectedIndex;
			}
			if (this.SelectedValue != null && this.SelectedIndex == -1 && !object.Equals(this.SelectedValue, this.InternalSelectedValue))
			{
				this.SelectItemWithValue(this.SelectedValue);
			}
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.SelectionChange.Begin();
					try
					{
						object item = e.NewItems[0];
						if (this.ItemGetIsSelected(item))
						{
							this.SelectionChange.Add(item);
						}
						break;
					}
					finally
					{
						this.SelectionChange.End();
					}
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.SelectionChange.Begin();
					try
					{
						object item = e.OldItems[0];
						if (this.selectedItems.Contains(item))
						{
							this.SelectionChange.Remove(item);
						}
						break;
					}
					finally
					{
						this.SelectionChange.End();
					}
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				{
					this.SelectionChange.Begin();
					try
					{
						object oldItem = e.OldItems[0];
						if (this.selectedItems.Contains(oldItem))
						{
							this.SelectionChange.Remove(oldItem);
						}
						object newItem = e.NewItems[0];
						if (this.ItemGetIsSelected(newItem))
						{
							this.SelectionChange.Add(newItem);
						}
						break;
					}
					finally
					{
						this.SelectionChange.End();
					}
					break;
				}
				case NotifyCollectionChangedAction.Remove | NotifyCollectionChangedAction.Replace:
				{
					action = new object[] { e.Action };
					throw new NotSupportedException(Telerik.Windows.Controls.SR.Get("UnexpectedCollectionChangeAction", action));
				}
				case NotifyCollectionChangedAction.Reset:
				{
					if (base.Items.Count == 0)
					{
						this.SelectionChange.Clear();
					}
					this.SelectionChange.Begin();
					try
					{
						for (int i = 0; i < this.selectedItems.Count; i++)
						{
							object item = this.selectedItems[i];
							if (!base.Items.Contains(item))
							{
								this.SelectionChange.Remove(item);
							}
						}
						if (base.ItemsSource == null)
						{
							for (int j = 0; j < base.Items.Count; j++)
							{
								object item2 = base.Items[j];
								if (this.IndexGetIsSelected(j, item2) && !this.selectedItems.Contains(item2))
								{
									this.SelectionChange.Add(item2);
								}
							}
						}
						break;
					}
					finally
					{
						this.SelectionChange.End();
					}
					break;
				}
				default:
				{
					action = new object[] { e.Action };
					throw new NotSupportedException(Telerik.Windows.Controls.SR.Get("UnexpectedCollectionChangeAction", action));
				}
			}
		}

		private static void OnSelected(object sender, RoutedEventArgs e)
		{
			RadRoutedEventArgs args = e as RadRoutedEventArgs;
			if (args != null)
			{
				((Selector)sender).NotifyIsSelectedChanged(args.OriginalSource as FrameworkElement, true, args);
			}
		}

		private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			object obj;
			Selector selector = (Selector)d;
			if (selector.SelectionChange != null && !selector.SelectionChange.IsActive)
			{
				int newValue = (int)d.GetValue(e.Property);
				obj = (newValue <= -1 || newValue >= selector.Items.Count ? null : selector.Items[newValue]);
				selector.SelectionChange.SelectJustThisItem(obj);
			}
		}

		private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Selector selector = (Selector)d;
			if (!selector.SelectionChange.IsActive)
			{
				selector.SelectionChange.SelectJustThisItem(d.GetValue(e.Property));
			}
		}

		/// <summary>
		/// Called when the SelectedValue property is changed.
		/// </summary>
		/// <param name="oldValue">The old value of the SelectedValue property.</param>
		/// <param name="newValue">The new value of the SelectedValue property.</param>
		protected virtual void OnSelectedValueChanged(object oldValue, object newValue)
		{
		}

		private static void OnSelectedValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			(sender as Selector).OnSelectedValueChanged(e.OldValue, sender.GetValue(e.Property));
		}

		/// <summary>
		/// Called when the SelectedValuePath property is changed.
		/// </summary>
		/// <param name="oldValue">The old value of the SelectedValuePath property.</param>
		/// <param name="newValue">The new value of the SelectedValuePath property.</param>
		protected virtual void OnSelectedValuePathChanged(string oldValue, string newValue)
		{
			if (this.SelectedValue != null)
			{
				this.SelectItemWithValue(this.SelectedValue);
			}
		}

		private static void OnSelectedValuePathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			(sender as Selector).OnSelectedValuePathChanged((string)e.OldValue, (string)e.NewValue);
		}

		/// <summary>
		/// Called when the selection changes.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected virtual void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.RaiseEvent(new Telerik.Windows.Controls.SelectionChangedEventArgs(Selector.SelectionChangedEvent, e.RemovedItems, e.AddedItems));
		}

		private void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.OnSelectionChanged(e);
		}

		private static void OnUnselected(object sender, RoutedEventArgs e)
		{
			RadRoutedEventArgs args = e as RadRoutedEventArgs;
			if (args != null)
			{
				((Selector)sender).NotifyIsSelectedChanged(args.OriginalSource as FrameworkElement, false, args);
			}
		}

		/// <summary>
		/// Removes a handler for the Selected attached event.
		/// </summary>
		/// <param name="element">Element that listens to this event.</param>
		/// <param name="handler">Event handler to remove.</param>
		public static void RemoveSelectedHandler(DependencyObject element, RoutedEventHandler handler)
		{
			element.RemoveHandler(Selector.SelectedEvent, handler);
		}

		/// <summary>
		/// Removes a handler for the Unselected attached event.
		/// </summary>
		/// <param name="element">Element that listens to this event.</param>
		/// <param name="handler">Event handler to remove.</param>
		public static void RemoveUnselectedHandler(DependencyObject element, RoutedEventHandler handler)
		{
			element.RemoveHandler(Selector.UnselectedEvent, handler);
		}

		private object SelectItemWithValue(object value)
		{
			this.selectingItemWithValue = true;
			object unsetValue = null;
			if (value != null)
			{
				unsetValue = this.FindItemWithValue(value);
			}
			if (this.SelectedItem != unsetValue)
			{
				this.SelectionChange.SelectJustThisItem(unsetValue);
			}
			if (unsetValue != null || base.HasItems)
			{
				this.selectingItemWithValue = false;
			}
			return unsetValue;
		}

		/// <summary>
		/// Sets a property value that indicates whether an item in a Selector is selected.
		/// </summary>
		/// <param name="element">Object on which to set the property.</param>
		/// <param name="isSelected">Value to set.</param>
		public static void SetIsSelected(DependencyObject element, bool isSelected)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			element.SetValue(Selector.IsSelectedProperty, isSelected);
		}

		private void SetSelectedHelper(object item, bool selected)
		{
			if (!Selector.ItemGetIsSelectable(item) && selected)
			{
				throw new InvalidOperationException(Telerik.Windows.Controls.SR.Get("CannotSelectNotSelectableItem", new object[0]));
			}
			this.SelectionChange.Begin();
			try
			{
				if (!selected)
				{
					this.SelectionChange.Remove(item);
				}
				else
				{
					this.SelectionChange.Add(item);
				}
			}
			finally
			{
				this.SelectionChange.End();
			}
		}

		internal static bool UIGetIsSelectable(DependencyObject item)
		{
			if (item != null)
			{
				if (!Selector.ItemGetIsSelectable(item))
				{
					return false;
				}
				Telerik.Windows.Controls.ItemsControl control = Telerik.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(item);
				if (control != null)
				{
					object itemFromContainer = control.ItemContainerGenerator.ItemFromContainer(item);
					if (itemFromContainer == item)
					{
						return true;
					}
					return Selector.ItemGetIsSelectable(itemFromContainer);
				}
			}
			return false;
		}

		internal virtual void UpdatePublicSelectionProperties()
		{
			int oldSelectedIndex = this.SelectedIndex;
			if (oldSelectedIndex > base.Items.Count - 1 || oldSelectedIndex == -1 && this.selectedItems.Count > 0 || oldSelectedIndex > -1 && this.selectedItems.Count == 0)
			{
				this.deferredSelectedIndex = oldSelectedIndex;
			}
			if (this.SelectedIndex != this.InternalSelectedIndex)
			{
				base.SetValue(Selector.SelectedIndexProperty, this.InternalSelectedIndex);
			}
			if (this.SelectedItem != this.InternalSelectedItem)
			{
				try
				{
					this.skipCoerceSelectedItemCheck = true;
					base.SetValue(Selector.SelectedItemProperty, this.InternalSelectedItem);
				}
				finally
				{
					this.skipCoerceSelectedItemCheck = false;
				}
			}
			if (!this.selectingItemWithValue && this.SelectedValue != this.InternalSelectedValue)
			{
				base.SetValue(Selector.SelectedValueProperty, this.InternalSelectedValue);
			}
		}

		private static bool ValidateSelectedIndex(object o)
		{
			return (int)o >= -1;
		}

		/// <summary>
		/// Occurs when the selection of a Selector changes.
		/// </summary>
		[Telerik.Windows.Controls.SRCategory("Behavior")]
		public event Telerik.Windows.Controls.SelectionChangedEventHandler SelectionChanged
		{
			add
			{
				this.AddHandler(Selector.SelectionChangedEvent, value);
			}
			remove
			{
				this.RemoveHandler(Selector.SelectionChangedEvent, value);
			}
		}

		internal class SelectionChanger<T> : ObservableCollection<T>
		{
			private Func<T, bool> isItemValidForSelection;

			private List<T> itemsToSelect;

			private List<T> itemsToUnselect;

			private ObservableCollection<T> internalSelection;

			private bool isActive;

			private Selector selector;

			internal bool IsActive
			{
				get
				{
					return this.isActive;
				}
			}

			public SelectionChanger(Selector owner, Func<T, bool> isItemValidForSelection) : this(owner)
			{
				this.isItemValidForSelection = isItemValidForSelection;
			}

			public SelectionChanger(Selector owner)
			{
				this.selector = owner;
				this.itemsToSelect = new List<T>(1);
				this.itemsToUnselect = new List<T>(1);
				this.internalSelection = new ObservableCollection<T>();
				this.InitFlags();
			}

			private bool AddToSelection(T item)
			{
				bool result = true;
				if (this.isItemValidForSelection != null)
				{
					result = this.isItemValidForSelection(item);
				}
				if (result && this.ItemIsSelectable(item))
				{
					this.itemsToSelect.Add(item);
				}
				return result;
			}

			internal void Begin()
			{
				this.isActive = true;
				this.itemsToSelect.Clear();
				this.itemsToUnselect.Clear();
			}

			internal void Cancel()
			{
				this.InitFlags();
				this.itemsToSelect.Clear();
				this.itemsToUnselect.Clear();
			}

			protected override void ClearItems()
			{
				if (this.isActive)
				{
					this.InternalClear();
					return;
				}
				this.Begin();
				this.InternalClear();
				this.End();
			}

			internal void End()
			{
				this.SynchronizeInternalSelection();
				this.selector.UpdatePublicSelectionProperties();
				this.InitFlags();
				if (this.itemsToUnselect.Count > 0 || this.itemsToSelect.Count > 0)
				{
					this.InvokeSelectionChangedEvent();
				}
				this.itemsToSelect.Clear();
				this.itemsToUnselect.Clear();
			}

			private void InitFlags()
			{
				this.isActive = false;
			}

			protected override void InsertItem(int index, T item)
			{
				if (this.isActive)
				{
					this.Select(index, item);
					return;
				}
				this.Begin();
				this.Select(index, item);
				this.End();
			}

			protected void InternalClear()
			{
				base.ClearItems();
				this.itemsToSelect.Clear();
				this.itemsToUnselect.AddRange(this.internalSelection);
				this.internalSelection.Clear();
			}

			private void InvokeSelectionChangedEvent()
			{
				if (this.SelectionChanged != null)
				{
					System.Windows.Controls.SelectionChangedEventArgs e = new System.Windows.Controls.SelectionChangedEventArgs(this.itemsToUnselect, this.itemsToSelect);
					this.SelectionChanged(this.selector, e);
				}
			}

			private bool ItemIsSelectable(T item)
			{
				if (item == null || (object)item == DependencyProperty.UnsetValue)
				{
					return false;
				}
				return this.selector.Items.Contains(item);
			}

			protected override void RemoveItem(int index)
			{
				T changedItem = base[index];
				if (this.isActive)
				{
					this.Unselect(index, changedItem);
					return;
				}
				this.Begin();
				this.Unselect(index, changedItem);
				this.End();
			}

			private void Select(int index, T item)
			{
				if (this.itemsToUnselect.Remove(item))
				{
					if (!base.Items.Contains(item))
					{
						base.InsertItem(index, item);
						this.internalSelection.Insert(index, item);
					}
					return;
				}
				if (this.itemsToSelect.Contains(item))
				{
					return;
				}
				if (!this.internalSelection.Contains(item))
				{
					this.AddToSelection(item);
				}
				if (!this.selector.CanSelectMultiple && this.itemsToSelect.Count > 0)
				{
					this.itemsToUnselect.AddRange(this.internalSelection);
				}
			}

			internal void SelectJustThisItem(T item)
			{
				this.Begin();
				if (base.Items.Count > 0)
				{
					base.Clear();
				}
				try
				{
					if (this.ItemIsSelectable(item))
					{
						if (!this.itemsToUnselect.Contains(item))
						{
							this.Select(0, item);
						}
						else
						{
							if (!base.Items.Contains(item))
							{
								base.InsertItem(0, item);
								this.internalSelection.Insert(0, item);
							}
							this.itemsToUnselect.Remove(item);
						}
					}
				}
				finally
				{
					this.End();
				}
			}

			protected override void SetItem(int index, T item)
			{
				bool newItemIsSelected = this.internalSelection.Contains(item);
				if (item == null || newItemIsSelected)
				{
					base.RemoveAt(index);
					return;
				}
				this.Begin();
				T oldItem = base[index];
				bool added = this.AddToSelection(item);
				if (added)
				{
					this.internalSelection[index] = item;
					base.SetItem(index, item);
				}
				if (!base.Items.Contains(oldItem))
				{
					this.itemsToUnselect.Add(oldItem);
				}
				this.SynchronizeInternalSelection(!added);
				this.selector.UpdatePublicSelectionProperties();
				if (this.itemsToSelect.Count > 0 || this.itemsToUnselect.Count > 0)
				{
					this.InvokeSelectionChangedEvent();
				}
				this.Cancel();
			}

			private void SynchronizeInternalSelection(bool updateInternalSelection)
			{
				foreach (T item in this.itemsToUnselect)
				{
					this.selector.ItemSetIsSelected(item, false);
					if (!updateInternalSelection)
					{
						continue;
					}
					int index = base.IndexOf(item);
					if (index > -1)
					{
						base.RemoveItem(index);
					}
					this.internalSelection.Remove(item);
				}
				foreach (T item in this.itemsToSelect)
				{
					this.selector.ItemSetIsSelected(item, true);
					if (!updateInternalSelection)
					{
						continue;
					}
					base.InsertItem(base.Items.Count, item);
					this.internalSelection.Add(item);
				}
			}

			private void SynchronizeInternalSelection()
			{
				this.SynchronizeInternalSelection(true);
			}

			private void Unselect(int index, T item)
			{
				if (this.itemsToSelect.Remove(item))
				{
					if (base.Items.Contains(item))
					{
						base.RemoveItem(index);
						this.internalSelection.RemoveAt(index);
					}
					return;
				}
				if (this.itemsToUnselect.Contains(item))
				{
					return;
				}
				if (this.internalSelection.Contains(item))
				{
					this.itemsToUnselect.Add(item);
				}
			}

			public event System.Windows.Controls.SelectionChangedEventHandler SelectionChanged;
		}
	}
}