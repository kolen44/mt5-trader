using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace mtapi.mt5
{
	internal static class ExceptionExtensions
	{
		internal static void SetMessage(this Exception exception, string message)
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			var type = typeof(Exception);
			var flags = BindingFlags.Instance | BindingFlags.NonPublic;
			var fieldInfo = type.GetField("_message", flags);
			fieldInfo.SetValue(exception, message);
		}
	}
}
