

namespace KiCData.Models.WebModels.PaymentModels
{
    
    
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
}