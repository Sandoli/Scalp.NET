namespace Core
{
    public struct Exec
    {
        public decimal Price { get; private set; }

        public int Qty { get; private set; }

        public Exec(int qty, decimal price)
        {
            Qty = qty;
            Price = price;
        }
    }
}
