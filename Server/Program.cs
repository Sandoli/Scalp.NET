using Market;
using API;
using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var ob = new OrderBook();
            //ob.Create(new Order(10, 10.0m, Side.Bid));
            //ob.Create(new Order(10, 11.0m, Side.Bid));
            //ob.Create(new Order(10, 11.0m, Side.Bid));
            //ob.Create(new Order(10, 12.0m, Side.Bid));
            //ob.Create(new Order(10, 13.0m, Side.Bid));
            //ob.Create(new Order(10, 14.0m, Side.Bid));
            ob.Create(new Order(10, 10, 15.0m, Side.Bid));
            //ob.Create(new Order(10, 10.0m, Side.Ask));
            //ob.Create(new Order(10, 11.0m, Side.Ask));
            //ob.Create(new Order(10, 11.0m, Side.Ask));
            //ob.Create(new Order(10, 12.0m, Side.Ask));
            //ob.Create(new Order(10, 13.0m, Side.Ask));
            //ob.Create(new Order(10, 14.0m, Side.Ask));
            ob.Create(new Order(10, 10, 15.5m, Side.Ask));
            Console.WriteLine(ob);
            Console.ReadLine();

            ob.Create(new Order(10, 10, 12.0m, Side.Bid));
            Console.WriteLine(ob);
            Console.ReadLine();

        }
    }
}
