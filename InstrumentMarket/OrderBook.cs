using System;
using System.Collections.Generic;
using System.Linq;
using API;
using Common;
using System.Text;

namespace Core
{
    public class OrderBook //: IOrderBook
    {
        private readonly SortedDictionary<decimal, List<Order>> _bids;
        private readonly SortedDictionary<decimal, List<Order>> _asks;

        private readonly Dictionary<string, Order> _orderByIdList;

        protected IEnumerable<Order> BidOrders => _bids.SelectMany(p => p.Value);
        protected IEnumerable<Order> AskOrders => _asks.SelectMany(p => p.Value);

        protected IEnumerable<Limit> BidLimits
        {
            get
            {
                foreach (var pair in _bids.Reverse().Take(10))
                {
                    var bidOrders = pair.Value;
                    if (bidOrders != null && bidOrders.Count != 0)
                    {
                        var firstOrder = bidOrders.First();
                        yield return new Limit(bidOrders.Sum(p => p.Qty), firstOrder.Price, firstOrder.Side, bidOrders.Count);
                    }
                }
            }
        }

        protected IEnumerable<Limit> AskLimits
        {
            get
            {
                foreach (var pair in _asks.Take(10))
                {
                    var askOrders = pair.Value;
                    if (askOrders != null && askOrders.Count != 0)
                    {
                        var firstOrder = askOrders.First();
                        yield return new Limit(askOrders.Sum(p => p.Qty), firstOrder.Price, firstOrder.Side, askOrders.Count);
                    }
                }
            }
        }


        protected OrderBook()
        {
            _bids = new SortedDictionary<decimal, List<Order>>();
            _asks = new SortedDictionary<decimal, List<Order>>();

            _orderByIdList = new Dictionary<string, Order>();
        }

        protected bool Create(Order order)
        {
            CheckMatch(ref order);
            if (order.Filled) return true;

            if (_orderByIdList.ContainsKey(order.OrderId))
            {
                return false;
            }

            _orderByIdList.Add(order.OrderId, order);

            if (order.Side == Side.Bid)
            {
                List<Order> orderList;
                if (!_bids.ContainsKey(order.Price))
                {
                    orderList = new List<Order>();
                    _bids.Add(order.Price, orderList);
                }
                else
                {
                    orderList = _bids[order.Price];
                }
                orderList.Add(order);
            }
            else if (order.Side == Side.Ask)
            {
                List<Order> orderList;
                if (!_asks.ContainsKey(order.Price))
                {
                    orderList = new List<Order>();
                    _asks.Add(order.Price, orderList);
                }
                else
                {
                    orderList = _asks[order.Price];
                }
                orderList.Add(order);
            }

            return true;
        }

        private void CheckMatch(ref Order other)
        {
            if (other.Side == Side.Ask)
            {
                foreach (var pair in _bids.Reverse())
                {
                    if (pair.Key >= other.Price)
                    {
                        var orderList = pair.Value;
                        for (int i = orderList.Count - 1; i >= 0; i--)
                        {
                            var order = orderList[i];
                            var execQty = Math.Min(order.Qty, other.Qty);
                            var exec = new Exec(execQty, other.Price);

                            other.AddExec(exec);
                            order.AddExec(exec);

                            if (order.Filled) orderList.RemoveAt(i);
                            if (other.Filled) return;
                        }
                    }

                }
            }
            else if (other.Side == Side.Bid)
            {
                foreach (var pair in _asks)
                {
                    if (pair.Key <= other.Price)
                    {
                        var orderList = pair.Value;
                        for (int i = orderList.Count - 1; i >= 0; i--)
                        {
                            var order = orderList[i];
                            var execQty = Math.Min(order.Qty, other.Qty);
                            var exec = new Exec(execQty, other.Price);

                            other.AddExec(exec);
                            order.AddExec(exec);

                            if (order.Filled) orderList.RemoveAt(i);
                            if (other.Filled) return;
                        }
                    }

                }
            }
        }

        protected bool Update(Order order)
        {
            if (order.Qty <= 0) return false;

            Order orderInList;
            // TODO Simple update right now
            if (_orderByIdList.TryGetValue(order.OrderId, out orderInList))
            {
                // Side different ?
                if (orderInList.Side != order.Side)
                {
                    return false;
                }

                // Price different ?
                if (orderInList.Price != order.Price)
                {
                    // TODO Code this case
                    return false;
                }
                orderInList.UpdateQty(order.Qty);
                return true;
            }

            return false;
        }

        protected bool Cancel(Order order)
        {
            if (_orderByIdList.Remove(order.OrderId))
            {
                if (order.Side == Side.Bid)
                {
                    if (!_bids.ContainsKey(order.Price))
                    {
                        return false;
                    }

                    var orderList = _bids[order.Price];
                    for (int i = orderList.Count - 1; i >= 0; i--)
                    {
                        if (orderList[i].OrderId == order.OrderId)
                        {
                            orderList.RemoveAt(i);
                            return true;
                        }
                    }
                }
                else if (order.Side == Side.Ask)
                {
                    if (!_asks.ContainsKey(order.Price))
                    {
                        return false;
                    }

                    var orderList = _asks[order.Price];
                    for (int i = orderList.Count - 1; i >= 0; i--)
                    {
                        if (orderList[i].OrderId == order.OrderId)
                        {
                            orderList.RemoveAt(i);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            using (var bidIt = BidLimits.GetEnumerator())
            {
                using (var askIt = AskLimits.GetEnumerator())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("-----------------------------");
                    sb.AppendLine("Bid       OrderBook       Ask");
                    sb.AppendLine("-----------------------------");
                    var hasCurrentBid = bidIt.MoveNext();
                    var hasCurrentAsk = askIt.MoveNext();
                    while (hasCurrentBid || hasCurrentAsk)
                    {
                        if (hasCurrentBid && hasCurrentAsk)
                        {
                            var currBid = bidIt.Current;
                            var currAsk = askIt.Current;
                            sb.AppendLine($"{currBid} | {currAsk}");
                            hasCurrentBid = bidIt.MoveNext();
                            hasCurrentAsk = askIt.MoveNext();
                        }
                        else if (hasCurrentBid)
                        {
                            var currBid = bidIt.Current;
                            sb.AppendLine($"{currBid} |");
                            hasCurrentBid = bidIt.MoveNext();
                        }
                        else
                        {
                            var currAsk = askIt.Current;
                            sb.AppendLine($"       |    {currAsk}");
                            hasCurrentAsk = askIt.MoveNext();
                        }
                        sb.AppendLine("-----------------------------");

                    }

                    return sb.ToString();
                }
            }
        }
    }
}
