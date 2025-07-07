using System;
using Microsoft.Extensions.Configuration;
using Square;
using Square.Models;
using Square.Authentication;
using KiCData.Models.WebModels.PurchaseModels;
using KiCData.Models;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace KiCData.Services
{
    public class InternalPaymentService : IPaymentService
    {
        #region Fields
        private SquareClient _squareClient;
        private IKiCLogger _kiCLogger;
        private string _locationId;
        private KiCdbContext _context;
        #endregion
        
        public InternalPaymentService(IConfigurationRoot configuration, IKiCLogger logger, KiCdbContext context)
        {
            Square.Environment env = Square.Environment.Production;

            if (configuration["Square:Environment"] == "Sandbox")
            {
                env = Square.Environment.Sandbox;
            }

            _squareClient = new SquareClient.Builder().BearerAuthCredentials
                (
                    new BearerAuthModel.Builder(configuration["Square:Token"])
                    .Build()
                )
                .Environment(env)
                .Build();
            _kiCLogger = logger;
            
            _locationId = _squareClient.LocationsApi.ListLocations().Locations.First().Id;
            
            _context = context;
        }
        
        #region Inventory Methods
        public int CheckInventory(string objectSearchTerm, string variationSearchTerm)
        {
            return checkInventory(objectSearchTerm, variationSearchTerm);
        }
        
        private int checkInventory(string objectSearchTerm, string variationSearchTerm)
        {
            ListCatalogResponse catResponse = _squareClient.CatalogApi.ListCatalog();
            CatalogObject? obj = catResponse.Objects
                .Where(o => o.ItemData.Name.Contains(objectSearchTerm))
                .FirstOrDefault();

            if (obj == null)
            {
                throw new Exception("Object not found.");
            }

            string varId = obj.ItemData.Variations
                .Where(v => v.ItemVariationData.Name.Contains(variationSearchTerm))
                .FirstOrDefault()
                .Id;

            RetrieveInventoryCountResponse countResponse = _squareClient.InventoryApi.RetrieveInventoryCount(varId);

            if (countResponse.Counts == null)
            {
                throw new Exception("No count for " + variationSearchTerm + " found.");
            }

            if(countResponse.Counts.Count > 1)
            {
                throw new Exception("Found multiple counts for " + variationSearchTerm + ".");
            }

            InventoryCount count = countResponse.Counts.FirstOrDefault();

            int response = int.Parse(count.Quantity);

            return response;
        }
        #endregion
        
        #region Generic Payment Methods
        
        
        public void CreateGenericPayment(string cardToken, BillingContact billingContact, List<IPurchaseModel> items)
        {
            double itemPrice = convertItemsToOrder(items);
            createGenericPayment(cardToken, billingContact, itemPrice);
        }

        private void createGenericPayment(string cardToken, BillingContact billingContact, double price)
        {
            long amountInCents = (long)(price * 100); // Square API expects amount in cents
            Money amountMoney = new Money.Builder()
                .Amount(amountInCents)
                .Currency("USD") // Assuming USD, change as necessary
                .Build();
            CreatePaymentRequest payment = new CreatePaymentRequest.Builder(sourceId: cardToken, idempotencyKey: Guid.NewGuid().ToString())
                .AmountMoney(amountMoney)
                .LocationId(_locationId)
                .Build();
                
            var result = _squareClient.PaymentsApi.CreatePayment(payment);
        }
        
        private double convertItemsToOrder(List<IPurchaseModel> items)
        {
            double totalPrice = 0.0;
            List<ITicketPurchaseModel> ticketItems = new List<ITicketPurchaseModel>();
            List<IPurchaseModel> nonTicketItems = new List<IPurchaseModel>();
            
            foreach (var item in items)
            {
                totalPrice += item.Price;
                if(item.Type.ToLower() == "ticket")
                {
                    ticketItems.Add((ITicketPurchaseModel)item);
                }
                else
                {
                    nonTicketItems.Add(item);
                }
            }
            
            if(ticketItems.Count > 0)
            {
                addTicketItemsToDataBase(ticketItems);
            }
            
            if(nonTicketItems.Count > 0)
            {
                HandleOrderItems(nonTicketItems);
            }
            
            return totalPrice;
        }

        private void addTicketItemsToDataBase(List<ITicketPurchaseModel> items)
        {
            foreach (var item in items)
            {
                Ticket ticket = new Ticket
                {
                    EventId = item.EventId,
                    Price = item.Price,
                    Type = item.Type,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    DatePurchased = DateOnly.FromDateTime(DateTime.Now),
                    IsComped = false // Assuming tickets are not comped by default
                };
                
                _context.Ticket.Add(ticket);
            }
            
            _context.SaveChanges();
        }
        
        private void HandleOrderItems(List<IPurchaseModel> items)
        {
        
        }
        
        /// <summary>
        /// Queries the Square Payment API to check the status of a payment.
        /// </summary>
        /// <param name="paymentId">The String ID of a payment.</param>
        /// <returns>bool</returns>
        public string CheckPaymentStatus(string paymentId)
        {
            return checkPaymentStatus(paymentId);
        }
        
        private string checkPaymentStatus(string paymentId)
        {
            try
            {
                var response = _squareClient.PaymentsApi.GetPayment(paymentId);
                return response.Payment.Status.ToLower();
            }
            catch (Square.Exceptions.ApiException ex)
            {
                _kiCLogger.LogSquareEx(ex);
                return "Error: " + ex.Message;
            }
        }
        #endregion
        
        #region Cure Payment Methods
        
        #endregion
        
        #region Blasphemy Payment Methods
        
        public void CreateBlasphemyTicketPayment()
        {
            
        }
        
        private void createBlasphemyTicketPayment()
        {
            
        }
        #endregion
    }
    
    public class BillingContact
    {
        public string[] AddressLines { get; set; }
        public string FamilyName{ get; set; }
        public string GivenName { get; set; }
        public string EmailAddress { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        
        public BillingContact(string[] addressLines, string familyName, string givenName, string emailAddress, string countryCode, string phoneNumber, string state, string city, string postalCode)
        {
            AddressLines = addressLines;
            FamilyName = familyName;
            GivenName = givenName;
            EmailAddress = emailAddress;
            CountryCode = countryCode;
            PhoneNumber = phoneNumber;
            State = state;
            City = city;
            PostalCode = postalCode;
        }
        
        public BillingContact()
        {
            AddressLines = Array.Empty<string>();
            FamilyName = string.Empty;
            GivenName = string.Empty;
            EmailAddress = string.Empty;
            CountryCode = string.Empty;
            PhoneNumber = string.Empty;
            State = string.Empty;
            City = string.Empty;
            PostalCode = string.Empty;
        }
    }
    
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