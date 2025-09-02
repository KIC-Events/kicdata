using KiCData.Models;
using KiCData.Models.WebModels;
using KiCData.Models.WebModels.Member;
using KiCData.Models.WebModels.PaymentModels;
using KiCData.Models.WebModels.PurchaseModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Square;
using Square.Authentication;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KiCData.Services
{
    public class PaymentService : IPaymentService
    {
        #region Setup
        private SquareClient _client;
        private IKiCLogger _logger;
        private string _locationId;
        private KiCdbContext _context;

        public PaymentService(IConfigurationRoot configuration, IKiCLogger logger, KiCdbContext context) // Initialize Square Client with configuration settings
        {
            Square.Environment env = Square.Environment.Production;

            if (configuration["Square:Environment"] == "Sandbox")
            {
                env = Square.Environment.Sandbox;
            }

            _client = new SquareClient.Builder().BearerAuthCredentials
                (
                    new BearerAuthModel.Builder(configuration["Square:Token"])
                    .Build()
                )
                .Environment(env)
                .Build();
                
            _logger = logger;
            
            _locationId = _client.LocationsApi.ListLocations().Locations.First().Id;
            
            _context = context;
        }

        #endregion
        
        #region Inventory Methods
        public int CheckInventory(string objectSearchTerm, string variationSearchTerm)
        {
            int response = checkInventory(objectSearchTerm, variationSearchTerm);
            return response;
        }

        private int checkInventory(string objectSearchTerm, string variationSearchTerm)
        {
            ListCatalogResponse catResponse = _client.CatalogApi.ListCatalog();
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

            RetrieveInventoryCountResponse countResponse = _client.InventoryApi.RetrieveInventoryCount(varId);

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
        
        public List<TicketInventory> GetTicketInventory(string objectSearchTerm)
        {
            List<TicketInventory> response = getTicketInventory(objectSearchTerm);
            return response;
        }

        private List<TicketInventory> getTicketInventory(string objectSearchTerm)
        {
            List<TicketInventory> inventory = new List<TicketInventory>();

            ListCatalogResponse catResponse = _client.CatalogApi.ListCatalog();
            List<CatalogObject> objs = catResponse.Objects
                .Where(o => o.Type == "ITEM" && o.ItemData?.Name.Contains(objectSearchTerm) == true)
                .ToList();

            if (objs == null || objs.Count == 0)
            {
                throw new Exception("Object not found.");
            }

            foreach(CatalogObject obj in objs)
            {
                foreach(CatalogObject variation in obj.ItemData.Variations)
                {
                    string varId = variation.Id;

                    RetrieveInventoryCountResponse countResponse = _client.InventoryApi.RetrieveInventoryCount(varId);

                    int QuantityAvailable = 0;
                    if (countResponse.Counts != null)
                    {
                        InventoryCount count = countResponse.Counts.FirstOrDefault();
                        QuantityAvailable = int.Parse(count.Quantity);
                        throw new Exception("No count for " + variation.ItemVariationData.Name + " found.");
                    }

                    TicketInventory ti = new TicketInventory
                    {
                        SquareId = variation.Id,
                        Name = variation.ItemVariationData.Name,
                        Price = (variation.ItemVariationData.PriceMoney.Amount ?? 0) / 100.0,
                        QuantityAvailable = QuantityAvailable
                    };

                    inventory.Add(ti);
                }
            }

            return inventory;
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
                
            var result = _client.PaymentsApi.CreatePayment(payment);
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
        
        private double getTotalPrice(List<RegistrationViewModel> items)
        {
            double totalPrice = 0.0;
            
            foreach(RegistrationViewModel rvm in items)
            {
                double price = rvm.Price;
                
                if(rvm.TicketComp is not null)
                {
                    double compAmt = rvm.TicketComp.CompAmount ?? 0.0;
                    price = price - compAmt;
                }
                
                totalPrice += price;
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
        
        private void addTicketItemsToDataBase(List<RegistrationViewModel> items, string squareOrderID)
        {
            Console.WriteLine("Adding ticket items to database for order ID: " + squareOrderID);
            Console.WriteLine("Items: ");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(items));
            
            foreach (var item in items)
            {
                Member? member = _context.Members
                    .Where(m => m.FirstName == item.FirstName && m.LastName == item.LastName && m.DateOfBirth == item.DateOfBirth)
                    .FirstOrDefault();

                if (member is null)
                {
                    member = new Member
                    {
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Email = item.Email,
                        DateOfBirth = item.DateOfBirth,
                        FetName = item.FetName,
                        ClubId = item.ClubId,
                        PhoneNumber = item.PhoneNumber,
                        PublicId = null,
                        AdditionalInfo = null,
                        SexOnID = item.SexOnID,
                        City = item.City,
                        State = item.State,
                        VendorId = null,
                        Vendor = null,
                        Volunteer = null,
                        Staff = null,
                        User = null,
                        Attendee = null,
                    };

                    _context.Members.Add(member);
                    _context.SaveChanges();

                    member = _context.Members
                        .Where(m => m.FirstName == item.FirstName && m.LastName == item.LastName && m.DateOfBirth == item.DateOfBirth)
                        .First();
                }

                Console.WriteLine("Ticket item: ");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(item));

                Ticket ticket = new Ticket
                {
                    Event = item.Event,
                    EventId = item.Event.Id,
                    Price = item.Price,
                    Type = item.TicketType,
                    DatePurchased = DateOnly.FromDateTime(DateTime.Today),
                    StartDate = item.Event.StartDate,
                    EndDate = item.Event.EndDate,
                    IsComped = false                  
                };

                if (item.TicketComp is not null) ticket.IsComped = true;

                Attendee attendee = new Attendee
                {
                    Member = member,
                    MemberId = member.Id,
                    Ticket = ticket,
                    BadgeName = item.BadgeName,
                    BackgroundChecked = false,
                    Pronouns = item.Pronouns,
                    ConfirmationNumber = item.BadgeName.GetHashCode(),
                    RoomWaitListed = item.WaitList,
                    TicketWaitListed = item.WaitList,
                    RoomPreference = item.RoomType,
                    IsPaid = false,
                    IsRefunded = false,
                    isRegistered = false,
                    OrderID = squareOrderID,
                    PaymentLinkID = null
                };

                ticket.Attendee = attendee;

                _context.Ticket.Add(ticket);
                _context.Attendees.Add(attendee);
                _context.SaveChanges();
            }
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
                var response = _client.PaymentsApi.GetPayment(paymentId);
                return response.Payment.Status.ToLower();
            }
            catch (Square.Exceptions.ApiException ex)
            {
                _logger.LogSquareEx(ex);
                return "Error: " + ex.Message;
            }
        }
        #endregion

        #region CURE Payment Methods
        
        public string CreateCUREPayment(string cardToken, BillingContact billingContact, List<RegistrationViewModel> items)
        {
            Console.WriteLine("CreateCUREPayment flag 1");
            Console.WriteLine("Creating CURE Payment for " + items.Count + " items.");
            double itemPrice = getTotalPrice(items);
            
            Console.WriteLine("CreateCUREPayment flag 2");

            var result = createCUREPayment(cardToken, billingContact, itemPrice);

            Console.WriteLine("CreateCUREPayment flag 3");

            Console.WriteLine("Sending items: ");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(items));

            Console.WriteLine("Sending payment result: ");
            if (result == null)
            {
                Console.WriteLine("Result is null");
            }
            else
            {
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result.Payment));
            }

            addTicketItemsToDataBase(items, result.Payment.OrderId);

            Console.WriteLine("CreateCUREPayment flag 4");

            return result.Payment.Status;
        }

        private CreatePaymentResponse createCUREPayment(string cardToken, BillingContact billingContact, double price)
        {
            Console.WriteLine("createCUREPayment flag 1");
            Console.WriteLine("Values passed in: " + cardToken + " " + billingContact.EmailAddress + " " + price);
            long amountInCents = (long)(price * 100); // Square API expects amount in cents
            Money amountMoney = new Money.Builder()
                .Amount(amountInCents)
                .Currency("USD") // Assuming USD, change as necessary
                .Build();

            Console.WriteLine("createCUREPayment flag 2");
            Console.WriteLine("Amount in cents: " + amountInCents);

            CreatePaymentRequest payment = new CreatePaymentRequest.Builder(sourceId: cardToken, idempotencyKey: Guid.NewGuid().ToString())
                .AmountMoney(amountMoney)
                .LocationId(_locationId)
                .Build();

            Console.WriteLine("createCUREPayment flag 3");
            Console.WriteLine("Payment request built. Calling PaymentsApi.CreatePayment...");
            Console.WriteLine("payment json: " + System.Text.Json.JsonSerializer.Serialize(payment));
                
            CreatePaymentResponse result = _client.PaymentsApi.CreatePayment(payment);

            return result;
        }
        
        #endregion

        #region CURE Payment Link Methods
        /*
         * 10-24-2024 194-add-ticket-purchase-for-blashphemy
         * https://github.com/Malechus/kic/issues/194
         * This method should be a reusable method with injectable data
         * but since we are two and half months away from the event
         * rather than risk a refactor I am just renaming this method
         * from CreatePaymentLink to CreateCurePaymentLink and building a new
         * reusable CreatePaymentLink method.
         * Malechus
         */
        public PaymentLink CreateCurePaymentLink(List<RegistrationViewModel> regList)
        {
            PaymentLink paymentLink = createCurePaymentLink(regList);

            return paymentLink;
        }

        private PaymentLink createCurePaymentLink(List<RegistrationViewModel> regList)
        {
            List<OrderLineItem> orderLineItems = new List<OrderLineItem>();
            List<OrderLineItemDiscount> orderDiscounts = new List<OrderLineItemDiscount>();

            var locations = _client.LocationsApi.ListLocations();
            string locationID = locations.Locations.FirstOrDefault().Id;

            foreach (RegistrationViewModel reg in regList)
            {
                ListCatalogResponse catalog = _client.CatalogApi.ListCatalog();
                CatalogObject catObj = catalog.Objects
                    .Where(o => o.ItemData.Name == "CURE Event Ticket")
                    .FirstOrDefault();
                string id = catObj.Id;
                CatalogObject variation = catObj.ItemData.Variations.Where(v => v.ItemVariationData.Name == reg.TicketType).FirstOrDefault();
                string varId = variation.Id;

                var appliedDiscounts = new List<OrderLineItemAppliedDiscount>();

                if(reg.DiscountCode != null)
                {
                    string discountName = "";
                    switch (reg.TicketComp.CompReason)
                    {
                        case "Event Staff or Volunteer":
                            discountName = "Staff Comp";
                            break;
                        case "Scholarship":
                            discountName = "Staff Comp";
                            break;
                        case "Key Volunteer":
                            discountName = "Staff Comp";
                            break;
                        case "Comp - Gratuity":
                            discountName = "Staff Comp";
                            break;
                        case "Club 425 Member Discount":
                            discountName = "Club 425 Member";
                            break;
                        case "425 Early Access":
                            discountName = "Club 425 Member";
                            break;
                        default:
                            throw new Exception("Bad discount code.");
                            break;
                    }

                    var discount = catalog.Objects.Where(o => o.Type == "DISCOUNT" && o.DiscountData.Name == discountName).FirstOrDefault();

                    OrderLineItemAppliedDiscount orderLineItemAppliedDiscount = new OrderLineItemAppliedDiscount.Builder(discountUid: discount.Id)
                        .Build();

                    OrderLineItemDiscount lineItemDiscount = new OrderLineItemDiscount.Builder()
                        .Uid(discount.Id)
                        .Name(discount.DiscountData.Name)
                        .Scope("LINE_ITEM")
                        .AppliedMoney(discount.DiscountData.AmountMoney)
                        .AmountMoney(discount.DiscountData.AmountMoney)
                        .Build();

                    orderDiscounts.Add(lineItemDiscount);

                    appliedDiscounts.Add(orderLineItemAppliedDiscount);
                }

                OrderLineItem orderLineItem = new OrderLineItem.Builder(quantity: "1")
                    .CatalogObjectId(varId)
                    .AppliedDiscounts(appliedDiscounts)
                    .Note(reg.FirstName + " " + reg.LastName)
                    .Build();

                orderLineItems.Add(orderLineItem);
            }

            OrderServiceCharge orderServiceCharge = new OrderServiceCharge.Builder()
                .Name("Handling Fee")
                .Percentage("3")
                .CalculationPhase("SUBTOTAL_PHASE")
                .Build();

            List<OrderServiceCharge> serviceCharges = new List<OrderServiceCharge>();
            serviceCharges.Add(orderServiceCharge);

            OrderPricingOptions pricingOptions = new OrderPricingOptions.Builder()
                .AutoApplyTaxes(true)
                .Build();

            Order order = new Order.Builder(locationId: locationID)
                .LineItems(orderLineItems)
                .PricingOptions(pricingOptions)
                .ServiceCharges(serviceCharges)
                .Discounts(orderDiscounts)
                .Build();

            //CreateOrderRequest orderRequest = new CreateOrderRequest.Builder()
            //    .IdempotencyKey(Guid.NewGuid().ToString())
            //    .Order(order)
            //    .Build();
            //
            //CreateOrderResponse orderResponse = _client.OrdersApi.CreateOrder(orderRequest);

            CheckoutOptions options = new CheckoutOptions.Builder()
                .RedirectUrl("https://cure.kicevents.com/RegistrationSuccessful")
                .Build();

            CreatePaymentLinkRequest paymentRequest = new CreatePaymentLinkRequest.Builder()
                .IdempotencyKey(Guid.NewGuid().ToString())
                .Order(order)
                .CheckoutOptions(options)
                .Build();

            PaymentLink paymentLink;

            try
            {
                CreatePaymentLinkResponse response = _client.CheckoutApi.CreatePaymentLink(paymentRequest);

                paymentLink = response.PaymentLink;
            }
            catch(Square.Exceptions.ApiException ex)
            {
                _logger.LogSquareEx(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                throw ex;
            }

            return paymentLink;
        }
        
        #endregion

        #region Payment Link Methods
        /// <summary>
        /// Generates a dynamic Square payment link for the requested items.
        /// </summary>
        /// <param name="regList">List<RegistrationViewModel> containing the registrants purchasing event tickets.</param>
        /// <param name="kicEvent">The KiCData.Models.Event object for which tickets are being purchased.</param>
        /// <param name="discountCodes">String[] array of discount codes that could apply.</param>
        /// <param name="redirectUrl">Url for redirect after payment complete (merch, etc.) leave empty for generic success page.</param>
        /// <returns>PaymentLink</returns>
        public PaymentLink CreatePaymentLink(List<RegistrationViewModel> regList, KiCData.Models.Event kicEvent, string[] discountCodes = null, string redirectUrl = null)
        {
            PaymentLink paymentLink = createPaymentLink(regList, kicEvent, discountCodes, redirectUrl);

            return paymentLink;
        }

        private PaymentLink createPaymentLink(List<RegistrationViewModel> regList, KiCData.Models.Event kicEvent, string[] discountCodes = null, string redirectUrl = null)
        {
            if(redirectUrl is null) redirectUrl = "https://www.kicevents.com/success";
            List<OrderLineItem> orderLineItems = new List<OrderLineItem>();
            List<OrderLineItemDiscount> orderDiscounts = new List<OrderLineItemDiscount>();

            var locations = _client.LocationsApi.ListLocations();
            string locationID = locations.Locations.FirstOrDefault().Id;

            foreach(RegistrationViewModel reg in regList)
            {
                ListCatalogResponse catalogResponse = _client.CatalogApi.ListCatalog();
                CatalogObject? catalogObject = catalogResponse.Objects
                    .Where(o => o.ItemData.Name == reg.Event.Name)
                    .FirstOrDefault();

                if(catalogObject is null)
                {
                    _logger.LogText("Could not find Catalog Object " + reg.Event.Name);
                    throw new Exception("Catalog object not found.");
                }

                string id = catalogObject.Id;

                CatalogObject? variation = catalogObject.ItemData.Variations.Where(v => v.ItemVariationData.Name == reg.TicketType).FirstOrDefault();

                if(variation is null)
                {
                    _logger.LogText("Could not find Item Variation " + reg.TicketType);
                    throw new Exception("Item variation not found.");
                }

                string varId = variation.Id;

                OrderLineItem orderLineItem = new OrderLineItem.Builder(quantity: "1")
                    .CatalogObjectId(varId)
                    .Note(reg.FirstName + " " + reg.LastName)
                    .Build();

                orderLineItems.Add(orderLineItem);
            }

            OrderServiceCharge orderServiceCharge = new OrderServiceCharge.Builder()
                .Name("Handling Fee")
                .Percentage("3")
                .CalculationPhase("SUBTOTAL_PHASE")
                .Build();

            List<OrderServiceCharge> serviceCharges = new List<OrderServiceCharge>();
            serviceCharges.Add(orderServiceCharge);

            OrderPricingOptions pricingOptions = new OrderPricingOptions.Builder()
                .AutoApplyTaxes(true)
                .Build();

            Order order = new Order.Builder(locationId: locationID)
                .LineItems(orderLineItems)
                .PricingOptions(pricingOptions)
                .ServiceCharges(serviceCharges)
                .Discounts(orderDiscounts)
                .Build();

            CheckoutOptions options = new CheckoutOptions.Builder()
                .RedirectUrl(redirectUrl)
                .Build();

            CreatePaymentLinkRequest paymentRequest = new CreatePaymentLinkRequest.Builder()
                .IdempotencyKey(Guid.NewGuid().ToString())
                .Order(order)
                .CheckoutOptions(options)
                .Build();

            PaymentLink paymentLink;

            try
            {
                CreatePaymentLinkResponse response = _client.CheckoutApi.CreatePaymentLink(paymentRequest);

                paymentLink = response.PaymentLink;
            }
            catch (Square.Exceptions.ApiException ex)
            {
                _logger.LogSquareEx(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                throw ex;
            }

            return paymentLink;
        }
        
        #endregion
    }
}
