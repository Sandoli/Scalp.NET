using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    enum Action
    {
        New,
        Update,
        Cancel
    }

    enum Side
    {
        Bid,
        Ask
    }

    struct Exec
    {
        public int qty;
        public double price;
        public Side side;
        internal bool full;

        public bool IsFull() => full;
    }


    struct Order
    {
        public int Qty { get; }
        public double Price { get; }
        public Side Side { get; }
        public string OrderId { get; }

        public Order(int qty, double price, Side side) : this(qty, price, side, OrderBook.CreateNewOrderId())
        {
        }

        public Order(int qty, double price, Side side, string orderId)
        {
            Qty = qty;
            Price = price;
            Side = side;
            OrderId = orderId;
        }

        public override string ToString()
        {
            return (Side == Side.Bid) ? $"{Qty,3} {Price,6:0.##}" : $"{Price,-6:0.##} {Qty,-3}";
        }
    }

    class OrderBook
    {
        private static Random random = new Random();
        public static string CreateNewOrderId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public List<Order> Bids { get; }
        public List<Order> Asks { get; }

        public OrderBook()
        {
            Bids = new List<Order>();
            Asks = new List<Order>();
        }

        internal void AddExec(Exec exec)
        {
            int execQty = exec.qty;
            if (exec.side == Side.Bid)
            {
                for (int i = Bids.Count - 1; i >= 0; i--)
                {
                    if (Bids[i].Qty <= execQty)
                    {
                        execQty -= Bids[i].Qty;
                        Bids.RemoveAt(i);
                    }
                }
            }
            if (exec.side == Side.Ask)
            {
                for (int i = Asks.Count - 1; i >= 0; i--)
                {
                    if (Asks[i].Qty <= execQty)
                    {
                        execQty -= Asks[i].Qty;
                        Asks.RemoveAt(i);
                    }
                }
            }
        }

        internal void Update(Order order)
        {
            var orders = Bids;
            if (order.Side == Side.Ask)
            {
                orders = Asks;
            }

            for (int i = orders.Count - 1; i >= 0; i--)
            {
                if (orders[i].OrderId == order.OrderId)    
                {
                    orders[i] = order;
                    break;
                }
            }
        }

        internal void Cancel(Order order)
        {
            var orders = Bids;
            if (order.Side == Side.Ask)
            {
                orders = Asks;
            }

            for (int i = orders.Count - 1; i >= 0; i--)
            {
                if (orders[i].OrderId == order.OrderId)
                {
                    orders.RemoveAt(i);
                    break;
                }
            }
        }

        internal void Print()
        {
            var bidIt = Bids.GetEnumerator();
            var askIt = Asks.GetEnumerator();

            Console.WriteLine("----------------------");
            Console.WriteLine("Bid   OrderBook    Ask");
            Console.WriteLine("----------------------");
            bool hasCurrentBid = bidIt.MoveNext();
            bool hasCurrentAsk = askIt.MoveNext();
            while (hasCurrentBid || hasCurrentAsk)
            {
                if (hasCurrentBid && hasCurrentAsk)
                {
                    var currBid = bidIt.Current;
                    var currAsk = askIt.Current;
                    Console.WriteLine($"{currBid} | {currAsk}");
                    hasCurrentBid = bidIt.MoveNext();
                    hasCurrentAsk = askIt.MoveNext();
                }
                else if (hasCurrentBid)
                {
                    var currBid = bidIt.Current;
                    Console.WriteLine($"{currBid} |");
                    hasCurrentBid = bidIt.MoveNext();
                }
                else
                {
                    var currAsk = askIt.Current;
                    Console.WriteLine($"       |    {currAsk}");
                    hasCurrentAsk = askIt.MoveNext();
                }
                Console.WriteLine("----------------------");

            }
        }
    }
}
