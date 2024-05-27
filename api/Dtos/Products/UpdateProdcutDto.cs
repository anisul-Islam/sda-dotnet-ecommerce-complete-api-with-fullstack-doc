namespace api.Models
{
  public class UpdateProductDto
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int Sold { get; set; }
    public decimal Shipping { get; set; }
    public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    public string Image { get; set; } = string.Empty;
  }
}
