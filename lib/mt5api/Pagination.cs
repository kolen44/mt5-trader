using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	/// <summary>
	/// Orers pagination
	/// </summary>
	public class Pagination
	{
		/// <summary>
		/// Local creation time
		/// </summary>
		public DateTime Created = DateTime.Now;
		/// <summary>
		/// Orders number per page
		/// </summary>
		public int OrdersPerPage { get; private set; }
		/// <summary>
		/// Orders list
		/// </summary>
		public List<Order> Orders { get; private set; }
		/// <summary>
		/// Main cinstructor
		/// </summary>
		/// <param name="orders">Orders list</param>
		/// <param name="ordersPerPage">Orders number per page</param>
		public Pagination(List<Order> orders, int ordersPerPage)
		{
			Orders = orders;
			OrdersPerPage = ordersPerPage;
		}

		/// <summary>
		/// Total pages number
		/// </summary>
		/// <returns></returns>
		public int PagesCount()
		{
			if (Orders.Count % OrdersPerPage == 0)
				return Orders.Count / OrdersPerPage;
			else
				return Orders.Count / OrdersPerPage + 1;
		}

		/// <summary>
		/// Get page by index. Starting from zero.  
		/// </summary>
		/// <param name="pageIndex"></param>
		/// <returns></returns>
		public List<Order> GetPage(int pageIndex)
		{
			var res = new List<Order>();
			int i = 0;
			while (res.Count < OrdersPerPage)
			{
				if (pageIndex * OrdersPerPage + i >= Orders.Count)
					break;
				res.Add(Orders[pageIndex * OrdersPerPage + i]);
				i++;
			}
			return res;
		}
	}
}
