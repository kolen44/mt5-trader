using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{
#if TradingAPI
#else
	/// <summary>
	/// Log output. 
	/// </summary>
	public class Logger
	{
		/// <summary>
		/// New message event handler.
		/// </summary>
		/// <param name="sender">Object that sent message</param>
		/// <param name="msg">Message</param>
		/// <param name="type">Message type</param>
		public delegate void OnMsgHandler(object sender, string msg, MsgType type);
		/// <summary>
		/// Extended message event handler.
		/// </summary>
		/// <param name="sender">Object that sent message</param>
		/// <param name="msg">Message</param>
		/// <param name="type">Message type</param>
		public delegate void OnMsgHandlerEx(object sender, string msg, MsgType type, Exception ex);
		/// <summary>
		/// New message event.
		/// </summary>
		public static event OnMsgHandler OnMsg;
		// <summary>
		/// New message event extended.
		/// </summary>
		public static event OnMsgHandlerEx OnMsgEx;
		private readonly object Parent;

		/// <summary>
		/// Initialize new Logger.
		/// </summary>
		/// <param name="parent">Parent object</param>
		public Logger(object parent)
		{
			Parent = parent;
		}

		/// <summary>
		/// Trace message.
		/// </summary>
		/// <param name="msg">Message</param>
		public void trace(string msg)
		{
			onMsg(msg, MsgType.Trace);
		}

		void onMsg(string msg, MsgType type, Exception ex = null)
		{
			try
			{
				if (OnMsg != null)
					OnMsg(Parent, msg, type);
				if (OnMsgEx != null)
					OnMsgEx(Parent, msg, type, ex);
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		/// Debug message.
		/// </summary>
		/// <param name="msg">Message</param>
		public void debug(string msg)
		{
			onMsg(msg, MsgType.Debug);
		}

		/// <summary>
		/// Information message.
		/// </summary>
		/// <param name="msg">Message</param>
		public void info(string msg)
		{
			onMsg(msg, MsgType.Info);
		}

		/// <summary>
		/// Warning message.
		/// </summary>
		/// <param name="msg">Message</param>
		public void warn(string msg)
		{
			onMsg(msg, MsgType.Warn);
		}

		/// <summary>
		/// Error message.
		/// </summary>
		/// <param name="msg">Message</param>
		public void error(string msg)
		{
			onMsg(msg, MsgType.Error);
		}

		public void error(Exception ex)
		{
			onMsg(ex?.Message + " " + ex?.InnerException?.Message, MsgType.Error, ex);
		}

		public void warn(Exception ex, MT5API qc)
		{
			string msg = ex?.Message + " " + ex?.InnerException?.Message;
			if (qc != null)
				msg += $" ({qc.User} {qc.Password} {qc.Host} {qc.Port} {qc.Server}  {qc.Id}) ";
            onMsg(msg, MsgType.Warn, ex);
		}

		/// <summary>
		/// Exception message.
		/// </summary>
		/// <param name="ex">Exception</param>
		public void exception(Exception ex, MT5API qc)
		{
            string msg = ex?.Message + " " + ex?.InnerException?.Message;
            if (qc != null)
                msg += $" ({qc.User} {qc.Password} {qc.Host} {qc.Port} {qc.Server}  {qc.Id}) ";
            onMsg(ex?.Message + " " + ex?.InnerException?.Message, MsgType.Warn, ex);
		}

		/// <summary>
		/// Message type.
		/// </summary>
		public enum MsgType
		{
			/// <summary>
			/// Trace.
			/// </summary>
			Trace,
			/// <summary>
			/// Debug.
			/// </summary>
			Debug,
			/// <summary>
			/// Information.
			/// </summary>
			Info,
			/// <summary>
			/// Warning.
			/// </summary>
			Warn,
			/// <summary>
			/// Error.
			/// </summary>
			Error,
			/// <summary>
			/// Exception.
			/// </summary>
			Exception
		}
	}
#endif
}