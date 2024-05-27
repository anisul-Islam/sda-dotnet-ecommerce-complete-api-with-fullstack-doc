namespace api.Models
{
    public enum OrderStatus
    {
        Pending,    // Default state
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

}