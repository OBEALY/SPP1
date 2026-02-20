namespace TestedProject;

public sealed record OrderItem(string Sku, decimal UnitPrice, int Quantity);

public sealed record OrderPriceBreakdown(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal DeliveryFee,
    decimal PrioritySurcharge,
    decimal Total);

public sealed class OrderPricingService
{
    public const decimal FreeDeliveryThreshold = 100m;
    public const decimal StandardDeliveryFee = 7.99m;
    public const decimal PriorityDeliverySurcharge = 5m;

    public OrderPriceBreakdown CalculateTotal(
        IReadOnlyCollection<OrderItem> items,
        decimal discountPercent = 0m,
        bool priorityDelivery = false)
    {
        if (items is null || items.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item.", nameof(items));
        }

        if (discountPercent is < 0m or > 50m)
        {
            throw new ArgumentOutOfRangeException(nameof(discountPercent), "Discount must be between 0 and 50 percent.");
        }

        ValidateItems(items);

        var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var discountAmount = RoundMoney(subtotal * discountPercent / 100m);
        var discountedSubtotal = subtotal - discountAmount;

        var deliveryFee = discountedSubtotal >= FreeDeliveryThreshold ? 0m : StandardDeliveryFee;
        var prioritySurcharge = priorityDelivery ? PriorityDeliverySurcharge : 0m;
        var total = RoundMoney(discountedSubtotal + deliveryFee + prioritySurcharge);

        return new OrderPriceBreakdown(
            Subtotal: RoundMoney(subtotal),
            DiscountAmount: discountAmount,
            DeliveryFee: deliveryFee,
            PrioritySurcharge: prioritySurcharge,
            Total: total);
    }

    private static void ValidateItems(IEnumerable<OrderItem> items)
    {
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Sku))
            {
                throw new ArgumentException("SKU cannot be empty.");
            }

            if (item.UnitPrice <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(item.UnitPrice), "Unit price must be greater than zero.");
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(item.Quantity), "Quantity must be greater than zero.");
            }
        }
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
