using System.Collections.Generic;
using API;
using Common;

namespace Core
{
    public class Order
    {
        private readonly List<Exec> _execs;

        public int Qty { get; private set; }
        public int RemainingQty { get; private set; }
        public decimal Price { get; private set; }
        public Side Side { get; }
        public string OrderId { get; }
        public IEnumerable<Exec> Execs => _execs;

        #region Flags
        public bool Filled => RemainingQty == 0;

        #endregion

        #region Constructors
        protected Order(int qty, decimal price, Side side) : this(qty, price, side, IdGenerator.CreateNewOrderId())
        {
        }

        protected Order(int qty, decimal price, Side side, string orderId)
        {
            Qty = qty;
            RemainingQty = qty;
            Price = price;
            Side = side;
            OrderId = orderId;
            _execs = new List<Exec>();
        }
        #endregion

        public void UpdateQty(int qty)
        {
            if (Filled) return;
            var deltaQty = qty - Qty;
            Qty = qty;
            RemainingQty += deltaQty;
        }

        public bool UpdatePrice(int price)
        {
            bool canUpdate = !Filled && RemainingQty == Qty;
            if (canUpdate)
            {
                Price = price;
            }
            return canUpdate;
        }

        internal void AddExec(Exec exec)
        {
            _execs.Add(exec);
            RemainingQty -= exec.Qty;
        }

        public override string ToString()
        {
            return (Side == Side.Bid) ? $"{Qty,3} {Price,6:0.00}" : $"{Price,-6:0.00} {Qty,-3}";
        }
    }
}
