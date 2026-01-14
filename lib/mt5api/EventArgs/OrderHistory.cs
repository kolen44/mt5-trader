using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    public class OrderHistoryEventArgs
    {
        public List<Order> Orders;
        public List<DealInternal> InternalDeals;
        public List<OrderInternal> InternalOrders;
        public int Action;
        public bool PartialResponse => Action == 14;
    }

    public class OrderHistoryEventArgsInternal
    {
        public ConcurrentDictionary<long, Order> Orders;
        public ConcurrentDictionary<long, DealInternal> InternalDeals;
        public ConcurrentDictionary<long, OrderInternal> InternalOrders;
        public int Action;
    }
}
