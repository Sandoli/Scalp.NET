namespace Market
{
    public class OrderBook : Core.OrderBook
    {
        public bool Create(Order order)
        {
            return base.Create(order);
        }
    }
}
