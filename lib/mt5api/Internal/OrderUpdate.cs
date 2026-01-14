using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Details of order update
    /// </summary>
    public class OrderUpdate
    {
        public TransactionInfo Trans;
        public OrderInternal OrderInternal;
        public DealInternal Deal;
        public DealInternal OppositeDeal;
        public Order Order;
        public UpdateType Type;
        public long CloseByTicket;

        public static bool AreEqual(OrderUpdate a, OrderUpdate b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            return Equals(a.Trans, b.Trans) &&
                   Equals(a.OrderInternal, b.OrderInternal) &&
                   Equals(a.Deal, b.Deal) &&
                   Equals(a.OppositeDeal, b.OppositeDeal) &&
                   Equals(a.Order, b.Order) &&
                   a.Type == b.Type &&
                   a.CloseByTicket == b.CloseByTicket;
        }

        private static bool Equals(TransactionInfo a, TransactionInfo b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            return a.UpdateId == b.UpdateId &&
                   a.TicketNumber == b.TicketNumber &&
                   a.s58 == b.s58 &&
                   a.OrderState == b.OrderState &&
                   a.ExpirationType == b.ExpirationType &&
                   a.ExpirationTime == b.ExpirationTime &&
                   a.OrderPrice == b.OrderPrice;
        }

        private static bool Equals(OrderInternal a, OrderInternal b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            return a.TicketNumber == b.TicketNumber &&
                   a.Ticket == b.Ticket &&
                   a.HistoryTime == b.HistoryTime &&
                   a.OpenTime == b.OpenTime &&
                   a.ExpirationTime == b.ExpirationTime &&
                   a.ExecutionTime == b.ExecutionTime &&
                   a.Type == b.Type &&
                   a.ExpirationType == b.ExpirationType &&
                   a.PlacedType == b.PlacedType &&
                   a.State == b.State &&
                   a.ExpertId == b.ExpertId &&
                   a.DealTicket == b.DealTicket &&
                   a.OpenTimeMs == b.OpenTimeMs;
        }

        private static bool Equals(DealInternal a, DealInternal b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            return a.TicketNumber == b.TicketNumber &&
                   a.HistoryTime == b.HistoryTime &&
                   a.OrderTicket == b.OrderTicket &&
                   a.OpenTime == b.OpenTime &&
                   a.Type == b.Type &&
                   a.ExpertId == b.ExpertId &&
                   a.PositionTicket == b.PositionTicket &&
                   a.OpenTimeMs == b.OpenTimeMs &&
                   a.PlacedType == b.PlacedType;
        }

        private static bool Equals(Order a, Order b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            return a.Ticket == b.Ticket &&
                   a.OpenPrice == b.ClosePrice &&
                   a.OpenTime == b.CloseTime &&
                   a.Lots == b.Lots &&
                   a.Comment == b.Comment &&
                   a.ClosePrice == b.ClosePrice &&
                   a.CloseTime == b.CloseTime &&
                   a.CloseLots == b.CloseLots &&
                   a.CloseComment == b.CloseComment &&
                   a.ExpertId == b.ExpertId &&
                   a.PlacedType == b.PlacedType &&
                   a.OrderType == b.OrderType &&
                   a.DealType == b.DealType &&
                   a.State == b.State &&
                   Equals(a.DealInternalIn, b.DealInternalIn) &&
                   Equals(a.DealInternalOut, b.DealInternalOut) &&
                   Equals(a.OrderInternal, b.OrderInternal) &&
                   Equals(a.PartialCloseDeals, b.PartialCloseDeals) &&
                   a.ExpirationType == b.ExpirationType &&
                   a.ExpirationTime == b.ExpirationTime &&
                   a.FillPolicy == b.FillPolicy &&
                   a.CloseTimestampUTC == b.CloseTimestampUTC;
        }

        private static bool Equals(DealInternal[] a, DealInternal[] b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (!Equals(a[i], b[i]))
                    return false;
            }

            return true;
        }
    }


    /// <summary>
    /// Order update type
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        ///  Internal use
        /// </summary>
        Unknown,
        /// <summary>
        /// Pending order cancel
        /// </summary>
        PendingClose,
        /// <summary>
        /// Market order open
        /// </summary>
        MarketOpen,
        /// <summary>
        /// Pending order open
        /// </summary>
        PendingOpen,
        /// <summary>
        /// Close of market order, but not by SL or TP
        /// </summary>
        MarketClose,
        /// <summary>
        /// Partial close of market order
        /// </summary>
        PartialClose,
        /// <summary>
        /// Internal use
        /// </summary>
        Started,
        /// <summary>
        /// Internal use
        /// </summary>
        Filled,
        /// <summary>
        /// Internal use
        /// </summary>
        Cancelling,
        /// <summary>
        /// Modify parameters of marlket order like sl, tp
        /// </summary>
        MarketModify,
        /// <summary>
        /// Modify parameters of pending order like sl, tp, expiration
        /// </summary>
        PendingModify,
        /// <summary>
        /// Market closed by SL
        /// </summary>
        OnStopLoss,
        /// <summary>
        /// Market closed by TP
        /// </summary>
        OnTakeProfit,
        /// <summary>
        /// Market order closed by margin call 
        /// </summary>
        OnStopOut,
        /// <summary>
        /// Balance transaction
        /// </summary>
        Balance,
        /// <summary>
        /// Pending order expiration
        /// </summary>
        Expired,
        /// <summary>
        /// Internal use
        /// </summary>
        Rejected,
        /// <summary>
        /// Market order closed by other
        /// </summary>
        MarketCloseBy,
        /// <summary>
        /// Represents a second or subsequent partial fill for an open order.
        /// </summary>
        PartialFill
    }
}
