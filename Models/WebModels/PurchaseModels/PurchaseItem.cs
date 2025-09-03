

namespace KiCData.Models.WebModels.PurchaseModels
{
    public abstract class PurchaseItem : IPurchaseModel
    {
        public string Type { get; set; } = "Item";
        public double Price { get; set; }
        public string Name { get; set; }
        public string? SquareID { get; set; }
        
    }
}