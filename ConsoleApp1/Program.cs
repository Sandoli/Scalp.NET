using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            int qty = 10;
            int lastPrice = 13;
            Scalp scalp = new Scalp(qty, lastPrice, 0.5, -50, 50);
            OrderBook ob = new OrderBook();

            Exec? exec = null;
            bool exit = false;
            while (!exit)
            {
                var newOrders = scalp.Run(ob, exec);
                SendOrdersToMarket(newOrders, ob);
                ob.Print();

                Console.Write("?");
                var line = Console.ReadLine();

                var tokens = line.Split(' ');
                int execQty = qty;
                if (tokens.Count() == 2)
                {
                    execQty = Math.Min(qty, int.Parse(tokens[1]));
                }
                if (tokens[0] == "A")
                {
                    if (execQty == ob.Asks.Sum(p => p.Qty))
                    {
                        exec = new Exec { qty = execQty, side = Side.Ask, price = ob.Asks.First().Price, full = true };
                    }
                    else
                    {
                        exec = new Exec { qty = execQty, side = Side.Ask, price = ob.Asks.First().Price, full = false };
                    }
                }
                else if (tokens[0] == "B")
                {
                    if (execQty == ob.Bids.Sum(p => p.Qty))
                    {
                        exec = new Exec { qty = execQty, side = Side.Bid, price = ob.Bids.First().Price, full = true };
                    }
                    else
                    {
                        exec = new Exec { qty = execQty, side = Side.Bid, price = ob.Bids.First().Price, full = false };
                    }
                }
                else if (tokens[0] == "Q")
                {
                    exit = true;
                }

                ob.AddExec(exec.Value);
                ob.Print();
            }
        }

        private static void SendOrdersToMarket(IEnumerable<KeyValuePair<Action, Order>> newOrders, OrderBook ob)
        {
            foreach (var kvp in newOrders)
            {
                var action = kvp.Key;
                var order = kvp.Value;
                if (kvp.Key == Action.New)
                {
                    if (order.Side == Side.Ask) ob.Asks.Add(order);
                    if (order.Side == Side.Bid) ob.Bids.Add(order);

                }
                else
                {
                    ob.Update(order);
                }
            }
        }
    }
}
