namespace FlashSales.Application.Extensions
{
    public static class MoneyExtensions
    {
        public static long ToCents(this decimal amount)
            => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

        public static decimal ToAmount(this long cents)
            => cents / 100m;
    }
}
