using KiCData.Models.WebModels;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KiCData.Models.WebModels.PurchaseModels;
using KiCData.Models.WebModels.PaymentModels;

namespace KiCData.Services
{
    public interface IPaymentService
    {
        public int CheckInventory(string objectSearchTerm, string variationSearchTerm);
        public List<TicketInventory> GetTicketInventory(string objectSearchTerm);
        public void CreateGenericPayment(string cardToken, BillingContact billingContact, List<IPurchaseModel> items);
        public string CheckPaymentStatus(string paymentId);
        public string CreateCUREPayment(string cardToken, BillingContact billingContact, List<RegistrationViewModel> items);
        public PaymentLink CreateCurePaymentLink(List<RegistrationViewModel> regList);
        public PaymentLink CreatePaymentLink(List<RegistrationViewModel> regList, KiCData.Models.Event kicEvent, string[] discountCodes = null, string redirectUrl = null);
        public double GetTicketPrice(string objectSearchTerm);
        
    }
}
