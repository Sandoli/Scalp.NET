using API;

namespace Common
{
    public struct Limit
    {
        public int Qty { get; }
        public decimal Price { get; }
        public Side Side { get; }
        public int OrderCount { get; }

        public Limit(int qty, decimal price, Side side, int orderCount)
        {
            Qty = qty;
            Price = price;
            Side = side;
            OrderCount = orderCount;
        }

        public override string ToString()
        {
            return (Side == Side.Bid) ? $"{OrderCount,3} {Qty,3} {Price,6:0.000}" : $"{Price,-6:0.000} {Qty,-3} {OrderCount,-3}";
        }
    }
}
