using System.ComponentModel.DataAnnotations;

namespace KiCData.Models.WebModels.PaymentModels
{
    
    
    public class BillingContact
    {

        [Required]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string AddressLine2 { get; set; }

        public string[] AddressLines { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string FamilyName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string GivenName { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string CountryCode { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public string State { get; set; }
        public string City { get; set; }

        [Display(Name = "Zip")]
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