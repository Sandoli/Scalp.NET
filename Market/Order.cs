using API;

namespace Market
{
    public class Order : Core.Order
    {
        #region Constructors
        public Order(int instrumentId, int qty, decimal price, Side side) : base(qty, price, side)
        {
            InstrumentId = instrumentId;
        }
        #endregion

        public int InstrumentId { get; private set; }
    }
}
