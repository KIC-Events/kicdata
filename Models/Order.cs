using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiCData.Models;

[Table("Order")]
public class Order
{
    [Key]
    public int? Id { get; set; }
    [MaxLength(100)]
    public string? SquareOrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int ItemsTotal { get; set; }
    public int Discounts { get; set; }
    public int SubTotal { get; set; }
    public int Taxes { get; set; }
    public int GrandTotal { get; set; }
    public int PaymentsTotal { get; set; }
    public int RefundsTotal { get; set; }
}