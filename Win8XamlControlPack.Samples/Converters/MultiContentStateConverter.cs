using System;
using Windows.UI.Xaml.Data;
using Win8XamlControlPack.Samples.ViewModels;

namespace Win8XamlControlPack.Samples.Converters
{
    public class MultiContentStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var contentState = (ContentState)value;
            switch (contentState)
            {
                case ContentState.SmallContent:
                    return MultiContentControlState.Small;
                case ContentState.NormalContent:
                    return MultiContentControlState.Normal;
                case ContentState.LargeContent:
                    return MultiContentControlState.Large;
                default:
                    return MultiContentControlState.Normal;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var state = (MultiContentControlState)value;
            switch (state)
            {
                case MultiContentControlState.Small:
                    return ContentState.SmallContent;
                case MultiContentControlState.Normal:
                    return ContentState.NormalContent;
                case MultiContentControlState.Large:
                    return ContentState.LargeContent;
                default:
                    return ContentState.NormalContent;
            }
        }
    }
}
