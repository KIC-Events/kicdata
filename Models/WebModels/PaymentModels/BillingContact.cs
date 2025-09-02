

namespace KiCData.Models.WebModels.PaymentModels
{
    
    
    public class BillingContact
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2{ get; set; }
        public string FamilyName{ get; set; }
        public string GivenName { get; set; }
        public string EmailAddress { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        
        public BillingContact(string addressLine1, string addressLine2, string familyName, string givenName, string emailAddress, string countryCode, string phoneNumber, string state, string city, string postalCode)
        {
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
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
            AddressLine1 = string.Empty;
            AddressLine2 = string.Empty;
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
}