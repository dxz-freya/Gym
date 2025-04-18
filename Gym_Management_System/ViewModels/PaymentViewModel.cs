public class PaymentViewModel
{
  public required decimal Price { get; set; }
  public required string PaymentMethod { get; set; }
  public required DateTime PaymentDate { get; set; }
}
