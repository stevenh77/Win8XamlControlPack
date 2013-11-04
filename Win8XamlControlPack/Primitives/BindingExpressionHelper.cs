using System;
using System.Linq.Expressions;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Win8XamlControlPack.Primitives.Extensions;

namespace Win8XamlControlPack.Primitives
{
	public class BindingExpressionHelper : FrameworkElement
	{
		private readonly static DependencyProperty ValueProperty;

		static BindingExpressionHelper()
		{
			ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(BindingExpressionHelper), null);
		}

		public static Func<object, object> CreateGetValueFunc(Type itemType, string propertyPath)
		{
			Expression get;
			LambdaExpression lambda;
			Delegate compiled;
			MethodInfo methodInfo;
			Func<object, object> func;
			ParameterExpression[] parameterExpressionArray;
			Type[] typeArray;
			object[] objArray;
			MethodInfo method;
			if (propertyPath != null && propertyPath.IndexOfAny(new [] { '.', '[', ']', '(', ')', '@' }) > -1)
			{
				return (item) => GetValueThroughBinding(item, propertyPath);
			}
			ParameterExpression parameter = Expression.Parameter(itemType, "item");
			if (!string.IsNullOrEmpty(propertyPath))
			{
				try
				{
					get = Expression.PropertyOrField(parameter, propertyPath);
					parameterExpressionArray = new [] { parameter };
					lambda = Expression.Lambda(get, parameterExpressionArray);
					compiled = lambda.Compile();
					method = typeof(BindingExpressionHelper).GetMethod("ToUntypedFunc");
					typeArray = new Type[] { itemType, lambda.Body.Type };
					methodInfo = method.MakeGenericMethod(typeArray);
					objArray = new object[] { compiled };
					return (Func<object, object>)methodInfo.Invoke(null, objArray);
				}
				catch (ArgumentException argumentException)
				{
					func = (object p) => null;
				}
				return func;
			}
			else
			{
				get = parameter;
			}
			parameterExpressionArray = new [] { parameter };
			lambda = Expression.Lambda(get, parameterExpressionArray);
			compiled = lambda.Compile();
			method = typeof(BindingExpressionHelper).GetMethod("ToUntypedFunc");
			typeArray = new [] { itemType, lambda.Body.Type };
			methodInfo = method.MakeGenericMethod(typeArray);
			objArray = new object[] { compiled };
			return (Func<object, object>)methodInfo.Invoke(null, objArray);
		}

		public static object GetValue(object item, string propertyPath)
		{
		    return item == null ? null : CreateGetValueFunc(item.GetType(), propertyPath)(item);
		}

		public static object GetValue(object item, Binding binding)
		{
			return GetValueThroughBinding(item, binding);
		}

		private static object GetValueThroughBinding(object item, Binding binding)
		{
			object value;
			var helper = new BindingExpressionHelper();
			try
			{
				helper.DataContext = item;
				BindingOperations.SetBinding(helper, ValueProperty, binding);
				value = helper.GetValue(ValueProperty);
			}
			finally
			{
				helper.ClearValue(ValueProperty);
			}
			return value;
		}

		private static object GetValueThroughBinding(object item, string propertyPath)
		{
			return GetValueThroughBinding(item, new Binding() { Path = new PropertyPath(propertyPath ?? ".")});
		}

		private static Func<object, object> ToUntypedFunc<T, TResult>(Func<T, TResult> func)
		{
			return (object item) => func((T)item);
		}
	}
}