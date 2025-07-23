using KiCData.Models.WebModels.PurchaseModels;

namespace KiCData.Models.WebModels.PaymentModels
{
    public class CureCardFormModel : CardFormModel
    {
        public new List<RegistrationViewModel> Items { get; set; }
        
        public CureCardFormModel(BillingContact billingContact, string? cardToken, List<IPurchaseModel>? items)
            : base(billingContact, cardToken, items)
        {
        }

        public CureCardFormModel()
            : base()
        {
        }
    }
}