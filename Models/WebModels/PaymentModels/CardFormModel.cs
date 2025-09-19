using KiCData.Models.WebModels.PurchaseModels;

namespace KiCData.Models.WebModels.PaymentModels
{
    public class CardFormModel
    {
        public BillingContact BillingContact { get; set; }
        
        public string? CardToken { get; set; }
        
        public List<IPurchaseModel>? Items { get; set; }
        
        public CardFormModel(BillingContact billingContact, string? cardToken, List<IPurchaseModel>? items)
        {
            BillingContact = billingContact;
            CardToken = cardToken;
            Items = items;
        }
        
        public CardFormModel()
        {
            BillingContact = new BillingContact();
            CardToken = null;
            Items = new List<IPurchaseModel>();
        }
    }
}