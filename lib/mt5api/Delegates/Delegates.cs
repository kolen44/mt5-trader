using mtapi.mt5.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	/// <summary>
	/// Desirializer
	/// </summary>
	public abstract class FromBufReader
	{
		internal abstract object ReadFromBuf(InBuf buf);
        
        internal static string GetString(byte[] buf)
		{
			int count = 0;
			for (int i = 0; i < buf.Length; i+=2)
			{
				if (buf[i] == 0 && buf[i+1] == 0)
					break;
				count++;
			}
			byte[] res = new byte[count*2];
			for (int i = 0; i < count*2; i++)
				res[i] = buf[i];
			string result = Encoding.Unicode.GetString(res);
			return result;
		}

        internal static byte[] GetBytes(string str, int size)
        {
            byte[] strBytes = Encoding.Unicode.GetBytes(str ?? "");
            byte[] result = new byte[size];
            int copyLength = Math.Min(strBytes.Length, size - 2); // reserve space for null terminator
            Array.Copy(strBytes, result, copyLength);
            // null terminator already exists as default in `result`
            return result;
        }
    }



	/// <summary>
	/// New quote
	/// </summary>
	/// <param name="api">Sender</param>
	/// <param name="quote">Quote</param>
	public delegate void OnQuote(MT5API api, Quote quote);
    /// <summary>
    /// Order update notification from server
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="update">Update details</param>
    public delegate void OnOrderUpdate(MT5API sender, OrderUpdate update);
    /// <summary>
    /// Order update notification from server
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="update">Update details</param>
    public delegate void OnSymbolUpdate(MT5API sender, SymbolUpdate update);

    /// <summary>
    /// Trade progress
    /// </summary>
    /// <param name="sender">Sender</param>
    /// /// <param name="progress">Progress details</param>
    public delegate void OnOrderProgress(MT5API sender, OrderProgress progress);
    /// <summary>
	/// Connect progress.
	/// </summary>
    /// <param name="sender">Object that sent event</param>
    /// <param name="args">Event arguments</param>
	public delegate void OnConnectProgress(MT5API sender, ConnectEventArgs args);
	/// <summary>
	/// Symbols update.
	/// </summary>
	/// <param name="sender">Object that sent event</param>
	/// <param name="args">Event arguments</param>
	public delegate void OnSymbolsUpdate(MT5API sender);

	/// <summary>
	/// Order history
	/// </summary>
	/// <param name="sender">Sender</param>
	/// <param name="orders">Hi level orders</param>
	/// <param name="internal_deals">Low level deals</param>
	/// <param name="internal_orders">Low level orders</param>
	public delegate void OnOrderHistory(MT5API sender, OrderHistoryEventArgs args);
	/// <summary>
	/// Quote history event. Use RequestQuoteHistory to request history. 
	/// </summary>
	public delegate void OnQuoteHistory(MT5API sender, QuoteHistoryEventArgs progress);
	/// <summary>
	/// Quote history event. Use RequestQuoteHistory to request history. 
	/// </summary>
	public delegate void OnTickHistory(MT5API sender, TickHistoryEventArgs progress);
	/// <summary>
	/// Mail inbox. 
	/// </summary>
	public delegate void OnMail(MT5API sender, MailMessage msg);
	/// <summary>
	/// Market depth. 
	/// </summary>
	public delegate void OnOrderBook(MT5API sender, SymbolBook args);
}
