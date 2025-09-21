

namespace KiCData.Models.WebModels.PaymentModels
{
    public class CureCardFormModel : CardFormModel
    {
        public new List<RegistrationViewModel> Items { get; set; }
        
        public CureCardFormModel(BillingContact billingContact, string? cardToken, List<InventoryItem>? items)
            : base(billingContact, cardToken, items)
        {
        }

        public CureCardFormModel()
            : base()
        {
        }
    }
}