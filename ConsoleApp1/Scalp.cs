using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    class Scalp
    {
        readonly int _qty;
        int _qtyBid;
        int _qtyAsk;
        int _pos;
        double _pxBid;
        double _pxAsk;
        double _tick;
        readonly int _posMin;
        readonly int _posMax;

        public Scalp(int qty, double lastPrice, double tick, int posMin, int posMax)
        {
            _qty = qty;
            _qtyBid = qty;
            _qtyAsk = qty;
            _pos = 0;
            _tick = tick;
            _posMin = posMin;
            _posMax = posMax;
            UpdateOrderPrices(lastPrice);
        }

        private void UpdateOrderPrices(double lastOrExecPrice)
        {
            _pxBid = lastOrExecPrice - _tick;
            _pxAsk = lastOrExecPrice + _tick;
        }


        public IEnumerable<KeyValuePair<Action, Order>> RunFullExec(OrderBook ob, Exec exec)
        {
            bool minPosReached = false;
            bool maxPosReached = false;

            // Execs
            _qtyBid = _qty;
            _qtyAsk = _qty;

            if (exec.side == Side.Ask)
            {
                _pos -= exec.qty;
                if (_pos <= _posMin) minPosReached = true;
            }
            else
            {
                _pos += exec.qty;
                if (_pos >= _posMax) maxPosReached = true;
            }
            UpdateOrderPrices(exec.price);

            // Adjust new orders according to orders in book
            Action bidAction = Action.New;
            string bidOrderId = string.Empty;
            foreach (var order in ob.Bids)
            {
                if (_pxBid != order.Price || _qtyBid != order.Qty)
                {
                    bidAction = Action.Update;
                    bidOrderId = order.OrderId;
                }
                else
                {
                    _qtyBid -= order.Qty;
                }
            }
            Action askAction = Action.New;
            string askOrderId = string.Empty;
            foreach (var order in ob.Asks)
            {
                if (_pxAsk != order.Price || _qtyAsk != order.Qty)
                {
                    askAction = Action.Update;
                    askOrderId = order.OrderId;
                }
                else
                {
                    _qtyAsk -= order.Qty;

                }
            }

            if (maxPosReached)
            {
                return RunPosLimitReached(bidAction, bidOrderId, askAction, askOrderId, Side.Ask);
            }

            if (minPosReached)
            {
                return RunPosLimitReached(bidAction, bidOrderId, askAction, askOrderId, Side.Bid);
            }

            return RunCreateOrders(bidAction, bidOrderId, askAction, askOrderId);
        }

        private List<KeyValuePair<Action, Order>> RunCreateOrders(Action bidAction, string bidOrderId, Action askAction, string askOrderId)
        {
            var list = new List<KeyValuePair<Action, Order>>();
            if (_qtyBid > 0)
            {
                if (bidAction == Action.Update)
                {
                    list.Add(new KeyValuePair<Action, Order>(Action.Update, new Order(_qtyBid, _pxBid, Side.Bid, bidOrderId)));
                }
                else if (bidAction == Action.Cancel)
                {
                    list.Add(new KeyValuePair<Action, Order>(Action.Cancel, new Order(_qtyBid, _pxBid, Side.Bid, bidOrderId)));
                }
                else
                {
                    list.Add(new KeyValuePair<Action, Order>(Action.New, new Order(_qtyBid, _pxBid, Side.Bid)));
                }
            }
            if (_qtyAsk > 0)
            {
                if (askAction == Action.Update)
                {
                    list.Add(new KeyValuePair<Action, Order>(Action.Update, new Order(_qtyAsk, _pxAsk, Side.Ask, askOrderId)));
                }
                else if (askAction == Action.Cancel)
                {
                    list.Add(new KeyValuePair<Action, Order>(Action.Cancel, new Order(_qtyAsk, _pxAsk, Side.Ask, askOrderId)));
                }
                else
                {
                    list.Add(new KeyValuePair<Action, Order>(Action.New, new Order(_qtyAsk, _pxAsk, Side.Ask)));
                }
            }
            Print(list.Select(p => p.Value));
            return list;
        }

        public List<KeyValuePair<Action, Order>> RunPartialExec(OrderBook ob, Exec exec)
        {
            bool minPosReached = false;
            bool maxPosReached = false;

            int bidQty = ob.Bids.Sum(p => p.Qty);
            int askQty = ob.Asks.Sum(p => p.Qty);

            // Execs
            if (exec.side == Side.Ask)
            {
                _qtyBid = bidQty + exec.qty;
                _qtyAsk = askQty - exec.qty;
            }
            else
            {
                _qtyBid = bidQty - exec.qty;
                _qtyAsk = askQty + exec.qty;
            }

            if (exec.side == Side.Ask)
            {
                _pos -= exec.qty;
                if (_pos <= _posMin) minPosReached = true;
            }
            else
            {
                _pos += exec.qty;
                if (_pos >= _posMax) maxPosReached = true;

            }
            //UpdateOrderPrices(exec.price);

            // Adjust new orders according to orders in book
            Action bidAction = Action.New;
            string bidOrderId = string.Empty;
            foreach (var order in ob.Bids)
            {
                if (_pxBid != order.Price || _qtyBid != order.Qty)
                {
                    bidAction = Action.Update;
                    bidOrderId = order.OrderId;
                }
                else
                {
                    _qtyBid -= order.Qty;
                }
            }
            Action askAction = Action.New;
            string askOrderId = string.Empty;
            foreach (var order in ob.Asks)
            {
                if (_pxAsk != order.Price || _qtyAsk != order.Qty)
                {
                    askAction = Action.Update;
                    askOrderId = order.OrderId;
                }
                else
                {
                    _qtyAsk -= order.Qty;

                }
            }


            if (maxPosReached)
            {
                return RunPosLimitReached(bidAction, bidOrderId, askAction, askOrderId, Side.Ask);
            }

            if (minPosReached)
            {
                return RunPosLimitReached(bidAction, bidOrderId, askAction, askOrderId, Side.Bid);
            }

            return RunCreateOrders(bidAction, bidOrderId, askAction, askOrderId);
        }

        public List<KeyValuePair<Action, Order>> RunPosLimitReached(Action bidAction, string bidOrderId, Action askAction, string askOrderId, Side sideReached)
        {
            if (sideReached == Side.Ask) _qtyBid = 0;
            if (sideReached == Side.Bid) _qtyAsk = 0;

            return RunCreateOrders(bidAction, bidOrderId, askAction, askOrderId);
        }

        public struct Parameters
        {
            Exec? exec;
            bool posLimitBidReached;
            bool posLimitAskReached;
            OrderBook ob;
        }

        public IEnumerable<KeyValuePair<Action, Order>> Run(OrderBook ob, Exec? exec = null)
        {
            // No Exec
            if (exec == null)
            {
                _qtyBid = _qty;
                _qtyAsk = _qty;
            }
            else if (exec.Value.IsFull())
            {
                return RunFullExec(ob, exec.Value);
            }
            else
            {
                return RunPartialExec(ob, exec.Value);
            }

            return RunCreateOrders(Action.New, string.Empty, Action.New, string.Empty); // new List<KeyValuePair<Action, Order>>()
        }

        public void Print(IEnumerable<Order> orders)
        {
            Console.WriteLine("Scalp ================");
            Console.WriteLine($"Position[{_pos}]");

            Console.WriteLine("Orders ===============");
            foreach (var order in orders)
            {
                if (order.Side == Side.Ask) Console.Write("             ");
                Console.WriteLine($"{order}");
            }
            Console.WriteLine("======================");
            Console.WriteLine();
        }
    }
}
