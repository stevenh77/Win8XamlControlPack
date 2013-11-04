using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Win8XamlControlPack.Samples.Common;

namespace Win8XamlControlPack.Samples.ViewModels
{
    public class MyViewModel : NotifyingObject
    {
        public string Header { get; set; }

        private ContentState contentState;
        public ContentState ContentState
        {
            get { return contentState; }
            set
            {
                if (contentState == value) return;
                contentState = value;
                OnPropertyChanged("ContentState");
            }
        }

        public static IList<object> GenerateItems()
        {
            var result = new ObservableCollection<object>();
            foreach (var num in Enumerable.Range(1, 5))
            {
                var vm1 = new MyViewModel()
                {
                    Header = String.Format("Item {0}", num),
                    ContentState = ContentState.SmallContent
                };
                vm1.ContentState  = ContentState.LargeContent;
                result.Add(vm1);
                result.Add(new MyViewModel() { Header = String.Format("Item {0}", num), ContentState = ContentState.NormalContent });
                result.Add(new MyViewModel() { Header = String.Format("Item {0}", num), ContentState = ContentState.LargeContent });
            }
            return result;
        }
    }
}