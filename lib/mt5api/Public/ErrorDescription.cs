using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	class ErrorDescription
	{
		private static Dictionary<int, int> keys;
		private static Dictionary<int, string> strings;

		static ErrorDescription()
		{
			keys = new Dictionary<int, int>();
            keys.Add(0x0,  0x572608); 
			keys.Add(0x1,  0x572604); keys.Add(0x2,  0x5725F4); keys.Add(0x3,  0x5725E0); keys.Add(0x4,  0x5725D0);
            keys.Add(0x5,  0x5725C4); keys.Add(0x6,  0x5725B4); keys.Add(0x7,  0x5725A0); keys.Add(0x8,  0x572588);
            keys.Add(0xD,  0x572580); keys.Add(0xE,  0x57257C); 
            keys.Add(0x40, 0x572574); keys.Add(0x41, 0x572564); keys.Add(0x80, 0x572554); keys.Add(0x81, 0x572544);
            keys.Add(0x82, 0x572530); keys.Add(0x83, 0x572520); keys.Add(0x84, 0x57250C); keys.Add(0x85, 0x5724F8);
            keys.Add(0x86, 0x5724E4); keys.Add(0x87, 0x5724D0); keys.Add(0x88, 0x5724C4); keys.Add(0x89, 0x5724B4);
            keys.Add(0x8A, 0x5724AC); keys.Add(0x8B, 0x57249C); keys.Add(0x8C, 0x57247C); keys.Add(0x8D, 0x572468);
            keys.Add(0x8E, 0x572454); keys.Add(0x8F, 0x572440); keys.Add(0x90, 0x572424); keys.Add(0x91, 0x5723DC);
            keys.Add(0x92, 0x57240C); keys.Add(0x93, 0x5723B0); keys.Add(0x94, 0x5723A0); keys.Add(0x95, 0x572390);
            keys.Add(0x96, 0x572380);
			strings = new Dictionary<int, string>();
            strings.Add(0x00572380, "Prohibited by FIFO rule");
            strings.Add(0x00572390, "Hedge is prohibited");
            strings.Add(0x005723A0, "Too many open orders");
            strings.Add(0x005723B0, "Expiration for pending orders is disabled");
            strings.Add(0x0057240C, "Trade context is busy");
            strings.Add(0x005723DC, "Modification denied. Order too close to market");
			strings.Add(0x00572424, "Request canceled by client");
			strings.Add(0x00572440, "Order is in process");
			strings.Add(0x00572454, "Order is accepted");
			strings.Add(0x00572468, "Too many requests");
			strings.Add(0x0057247C, "Only long positions are allowed");
			strings.Add(0x0057249C, "Order is locked");
			strings.Add(0x005724AC, "Requote");
			strings.Add(0x005724B4, "Broker is busy");
			strings.Add(0x005724C4, "Off quotes");
			strings.Add(0x005724D0, "Price is changed");
			strings.Add(0x005724E4, "Not enough money");
			strings.Add(0x005724F8, "Trade is disabled");
			strings.Add(0x0057250C, "Market is closed");
			strings.Add(0x00572520, "Invalid volume");
			strings.Add(0x00572530, "Invalid S/L or T/P");
			strings.Add(0x00572544, "Invalid prices");
			strings.Add(0x00572554, "Trade timeout");
			strings.Add(0x00572564, "Invalid account");
			strings.Add(0x00572574, "Account disabled");
            strings.Add(0x0057257C, "Invalid one-time password");
            strings.Add(0x00572580, "Secret key for one-time password is required");
            strings.Add(0x00572588, "Too frequent requests");
			strings.Add(0x005725A0, "Not enough rights");
			strings.Add(0x005725B4, "No connection");
			strings.Add(0x005725C4, "Old version");
			strings.Add(0x005725D0, "Server is busy");
			strings.Add(0x005725E0, "Invalid parameters");
			strings.Add(0x005725F4, "Common error");
			strings.Add(0x00572604, "OK");
            strings.Add(0x00572608, "Done");
        }

		public static string get(int code)
		{
			try
			{
				int key = keys[code];
				return strings[key];
			}
			catch (Exception)
			{
				return "Unknown server reply " + code.ToString("X");
			}
		}
	}

    internal enum MT4Status
    {
        OK_ANSWER = 0,
        OK_REQUEST = 1,
        COMMON_ERROR = 2,
        INVALID_PARAM = 3,
        SERVER_BUSY = 4,
        OLD_VERSION = 5,
        NO_CONNECT = 6,
        NOT_ENOUGH_RIGHTS = 7,
        TOO_FREQUENT_REQUEST = 8,
        SECRET_KEY_REQUIRED = 0xD,
        INVALID_ONETIME_PASSWORD = 0xE,
        ACCOUNT_DISABLED = 0x40,
        INVALID_ACCOUNT = 0x41,
        TRADE_TIMEOUT = 0x80,
        INVALID_PRICES = 0x81,
        INVALID_SL_TP = 0x82,
        INVALID_VOLUME = 0x83,
        MARKET_CLOSED = 0x84,
        TRADE_DISABLED = 0x85,
        NOT_MONEY = 0x86,
        PRICE_CHANGED = 0x87,
        OFF_QUOTES = 0x88,
        BROKER_BUSY = 0x89,
        REQUOTE = 0x8A,
        ORDER_LOCKED = 0x8B,
        LONG_POS_ALLOWED = 0x8C,
        TOO_MANY_REQUESTS = 0x8D,
        ORDER_ACCEPTED = 0x8E,
        ORDER_IN_PROCESS = 0x8F,
        REQUEST_CANCELLED = 0x90,
        MODIFICATIONS_DENIED = 0x91,
        TRADE_CONTEXT_BUSY = 0x92,
        EXPIRATION_DISABLED = 0x93,
        TOO_MANY_ORDERS = 0x94,
        HEDGE_PROHIBITED = 0x95,
        RPROHIBITED_FIFO = 0x96
    }
}
