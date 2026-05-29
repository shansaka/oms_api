namespace Oms.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; } =  decimal.Zero;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}