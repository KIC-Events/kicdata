using KiCData.Models;
using KiCData.Models.WebModels;
using KiCData.Models.WebModels.PaymentModels;
using Microsoft.Extensions.Configuration;
using Square;
using Square.Authentication;
using Square.Models;
using System.Xml;
using KiCData.Exceptions;
using Square.Exceptions;
using System.Diagnostics;

namespace KiCData.Services
{
    public class PaymentService
    {
        #region Setup
        private SquareClient _client;
        private IKiCLogger _logger;
        private string _locationId;
        private KiCdbContext _context;
        private IConfigurationRoot _config;

        public PaymentService(IConfigurationRoot configuration, IKiCLogger logger, KiCdbContext context) // Initialize Square Client with configuration settings
        {
            Square.Environment env = Square.Environment.Production;

            if (configuration["Square:Environment"] == "Sandbox")
            {
                env = Square.Environment.Sandbox;
            }
            var token =  configuration["Square:Token"];

            _client = new SquareClient.Builder().BearerAuthCredentials
                (
                    new BearerAuthModel.Builder(token)
                    .Build()
                )
                .Environment(env)
                .Build();
                
            _logger = logger;

            try
            {
                Console.WriteLine($"Base URI: {_client.GetBaseUri()}");
                _locationId = _client.LocationsApi.ListLocations().Locations.First().Id;
            }
            catch (ApiException e)
            {
                Console.WriteLine($"An error occurred while fetching locations: {e.Message}");
                Console.WriteLine($"Response code: {e.ResponseCode}");
                foreach (var err in e.Errors)
                {
                    Console.WriteLine($"[{err.Code}] {err.Category} - {err.Detail}");
                }
                throw;
            }
            
            _context = context;

            _config = configuration;
        }

        #endregion
        
        #region Inventory Methods
        

        /// <summary>
        /// Internal method to get Square Order ID.
        /// </summary>
        /// <param name="registrationViewModels">List of registrations.</param>
        /// <returns>Order ID as string.</returns>
        public string? getOrderID(List<RegistrationViewModel> registrationViewModels)
        {
            RegistrationViewModel rvm = registrationViewModels.First();

            try
            {
                string? orderID = _context.Attendees
                .Where(a => a.BadgeName == rvm.BadgeName
                    && a.Ticket.EventId == int.Parse(_config["CUREID"]))
                .FirstOrDefault()
                .OrderID;

                return orderID;
            }
            catch(NullReferenceException ex)
            {
                string? orderID = "Full comp: " + rvm.DiscountCode;

                return orderID;
            }
            catch(InvalidOperationException ex)
            {
                string? orderId = "Missing, please contact technology@kicevents.com for help.";
                _logger.Log(new UnreachableException("Missing order ID."));
                return orderId;
            }
        }
        #endregion

        #region Generic Payment Methods

        /// <summary>
        /// Calculates the total price for a list of registrations.
        /// </summary>
        /// <param name="items">List of registrations.</param>
        /// <returns>Total price as double.</returns>
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

        /// <summary>
        /// Adds ticket items to the database with Square Order ID.
        /// </summary>
        /// <param name="items">List of registration view models.</param>
        /// <param name="squareOrderID">Square Order ID.</param>
        /// <returns>A list of the Attendee records that were added to the database.</returns>
        private List<Attendee> addTicketItemsToDataBase(List<RegistrationViewModel> items, string squareOrderID)
        {
            List<Attendee> addedAttendees = [];
            foreach (var item in items)
            {
                if(item.TicketComp is not null)
                {
                    addCompedTicketItemToDataBase(item, squareOrderID);
                    break;
                }
            
                // Attempt to match the registration to an existing member.
                // Create a new member record if a match is not found.
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

                    member = _context.Members.Add(member).Entity;
                    _context.SaveChanges();
                }

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

                string saltedString = item.BadgeName + item.DateOfBirth.ToString();

                Attendee attendee = new Attendee
                {
                    Member = member,
                    MemberId = member.Id,
                    Ticket = ticket,
                    BadgeName = item.BadgeName,
                    BackgroundChecked = false,
                    Pronouns = item.Pronouns,
                    ConfirmationNumber = saltedString.GetHashCode(),
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
                addedAttendees.Add(_context.Attendees.Add(attendee).Entity);
                _context.SaveChanges();
            }

            return addedAttendees;
        }

        /// <summary>
        /// Adds a comped (complimentary) ticket item to the database, updating ticket, attendee, and member records as needed.
        /// </summary>
        /// <param name="item">The registration view model containing attendee and ticket information.</param>
        /// <param name="squareOrderID">The Square Order ID associated with the ticket.</param>
        private Attendee addCompedTicketItemToDataBase(RegistrationViewModel item, string squareOrderID)
        {
            TicketComp ticketComp = _context.TicketComp
                .Where(tc => tc.Id == item.TicketComp.Id)
                .First();

            Ticket ticket = _context.Ticket
                .Where(t => t.Id == ticketComp.TicketId)
                .First();

            Attendee attendee = _context.Attendees
                .Where(a => a.TicketId == ticket.Id)
                .FirstOrDefault();
                
            if(attendee is null)
            {
                string saltedString = item.BadgeName + item.DateOfBirth.ToString();
                attendee = new Attendee()
                {
                    Ticket = ticket,
                    TicketId = ticket.Id,
                    BadgeName = item.BadgeName,
                    BackgroundChecked = false,
                    ConfirmationNumber = saltedString.GetHashCode(),
                    RoomWaitListed = item.WaitList,
                    TicketWaitListed = item.WaitList,
                    RoomPreference = item.RoomType,
                    IsPaid = false,
                    isRegistered = true,
                    Pronouns = item.Pronouns,
                    OrderID = squareOrderID
                };
            }

            Member member = _context.Members
                .Where(m => m.Id == attendee.MemberId)
                .FirstOrDefault();
                
            if(member is null)
            {
                member = new Member()
                {
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Email = item.LastName,
                    DateOfBirth = item.DateOfBirth,
                    FetName = item.FetName,
                    ClubId = item.ClubId,
                    PhoneNumber = item.PhoneNumber,
                    City = item.City,
                    State = item.State,
                    SexOnID = item.SexOnID
                };
            }

            KiCData.Models.Event e = _context.Events
                .Where(ev => ev.Id == int.Parse(_config["CUREID"]))
                .First();

            attendee.Member = member;
            attendee.MemberId = member.Id;

            ticket.Event = e;
            ticket.EventId = e.Id;
            ticket.Price = item.Price;
            
            if(ticketComp.CompPct == 100)
            {
                ticket.Type = "Comp";
            }
            else
            {
                ticket.Type = "Partial Comp";
            }

            ticket.DatePurchased = DateOnly.FromDateTime(DateTime.Now);
            ticket.StartDate = e.StartDate;
            ticket.EndDate = e.EndDate;
            ticket.IsComped = true;

            ticketComp.Ticket = ticket;
            ticketComp.TicketId = ticket.Id;

            _context.SaveChanges();
            return attendee;
        }

        /// <summary>
        /// Sets attendees as paid asynchronously.
        /// </summary>
        /// <param name="registrationViewModels">List of registrations.</param>
        /// <returns>Task.</returns>
        public Task SetAttendeesPaidAsync(List<RegistrationViewModel> registrationViewModels)
        {
            return Task.Run(() => SetAttendeesPaid(registrationViewModels));
        }

        /// <summary>
        /// Internal method to set attendees as paid.
        /// </summary>
        /// <param name="registrationViewModels">List of registrations.</param>
        private async void SetAttendeesPaid(List<RegistrationViewModel> registrationViewModels)
        {
            await Task.Run(() =>
            {
                foreach (RegistrationViewModel rvm in registrationViewModels)
                {
                    try
                    {
                        Attendee attendee = _context.Attendees
                        .Where(a => a.Ticket.EventId == int.Parse(_config["CUREID"])
                        && a.BadgeName == rvm.BadgeName)
                        .First();

                        attendee.IsPaid = true;

                        _context.SaveChanges();
                    }
                    catch(Exception ex)
                    {
                        _logger.LogText("An exception has occurred while setting an attendee's status as paid.");
                        _logger.Log(ex);
                        _logger.LogText(rvm.LastName);
                        _logger.LogText(rvm.FirstName);
                        _logger.LogText(rvm.Email);
                        _logger.LogText(rvm.BadgeName);
                    }
                }
            });            
        }

        public List<Attendee> SetAttendeesPaid(List<Attendee> attendees)
        {
            foreach (Attendee attendee in attendees)
            {
                attendee.IsPaid = true;
                _context.Update(attendee);
            }
            _context.SaveChanges();
            return attendees;
        }

        /// <summary>
        /// Checks the status of a payment using Square Payment API.
        /// </summary>
        /// <param name="paymentId">Payment ID string.</param>
        /// <returns>Status as string.</returns>
        public string CheckPaymentStatus(string paymentId)
        {
            return checkPaymentStatus(paymentId);
        }

        /// <summary>
        /// Internal method to check payment status.
        /// </summary>
        /// <param name="paymentId">Payment ID string.</param>
        /// <returns>Status as string.</returns>
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

        public string CreateCUREPayment(string cardToken, BillingContact billingContact,
            List<RegistrationViewModel> items)
        {
            return CreateCUREPayment(cardToken, billingContact, items, out var attendees);
        }
        
        /// <summary>
        /// Creates a payment for CURE event tickets.
        /// </summary>
        /// <param name="cardToken">Card token for payment.</param>
        /// <param name="billingContact">Billing contact information.</param>
        /// <param name="items">List of registrations.</param>
        /// <returns>Payment status as string.</returns>
        public string CreateCUREPayment(string cardToken, BillingContact billingContact, List<RegistrationViewModel> items, out List<Attendee> attendees)
        {
            double itemPrice = getTotalPrice(items);

            var result = createCUREPayment(cardToken, billingContact, itemPrice);

            attendees = addTicketItemsToDataBase(items, result.Payment.OrderId);

            return result.Payment.Status;
        }

        /// <summary>
        /// Internal method to create a payment for CURE event tickets.
        /// </summary>
        /// <param name="cardToken">Card token for payment.</param>
        /// <param name="billingContact">Billing contact information.</param>
        /// <param name="price">Total price.</param>
        /// <returns>CreatePaymentResponse object.</returns>
        private CreatePaymentResponse createCUREPayment(string cardToken, BillingContact billingContact, double price)
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

                
            CreatePaymentResponse result = _client.PaymentsApi.CreatePayment(payment);

            return result;
        }
        
        public List<Attendee> HandleNonPaymentCURETicketOrder(List<RegistrationViewModel> registrationViewModels)
        {
            List<Attendee> attendees = [];
            foreach(RegistrationViewModel rvm in registrationViewModels)
            {
                attendees.Add(addCompedTicketItemToDataBase(rvm, Guid.NewGuid().ToString()));
            }
            return  attendees;
        }
        #endregion

        #region CURE Payment Link Methods

        /// <summary>
        /// Creates a Square payment link for CURE event tickets.
        /// </summary>
        /// <param name="regList">List of registrations.</param>
        /// <returns>PaymentLink object.</returns>
        public PaymentLink CreateCurePaymentLink(List<RegistrationViewModel> regList)
        {
            PaymentLink paymentLink = createCurePaymentLink(regList);

            return paymentLink;
        }

        /// <summary>
        /// Internal method to create a Square payment link for CURE event tickets.
        /// </summary>
        /// <param name="regList">List of registrations.</param>
        /// <returns>PaymentLink object.</returns>
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

        /// <summary>
        /// Internal method to generate a dynamic Square payment link for requested items.
        /// </summary>
        /// <param name="regList">List<RegistrationViewModel> containing the registrants purchasing event tickets.</param>
        /// <param name="kicEvent">The KiCData.Models.Event object for which tickets are being purchased.</param>
        /// <param name="discountCodes">String[] array of discount codes that could apply.</param>
        /// <param name="redirectUrl">Url for redirect after payment complete (merch, etc.) leave empty for generic success page.</param>
        /// <returns>PaymentLink</returns>
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
