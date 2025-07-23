using System.Web;
using KiCData.Models.WebModels;
using KiCData.Models.WebModels.PaymentModels;

namespace KiCData.Services
{
    /// <summary>
    /// Service for handling storage-related operations.
    /// </summary>
    public class RegistrationStorageService
    {
        public List<RegistrationStorage> Storage { get; set; }
        
        public RegistrationStorageService()
        {
            Storage = new List<RegistrationStorage>();
        }
        
        
    }
    
    public class RegistrationStorage
    {
        public string? SessionID{ get; set; }
        public List<RegistrationViewModel> Registrations { get; set; }
        
        public RegistrationStorage(string sessionId, List<RegistrationViewModel> registrations)
        {
            SessionID = sessionId;
            Registrations = registrations;
        }
        
        public RegistrationStorage(string sessionId)
        {
            SessionID = sessionId;
            Registrations = new List<RegistrationViewModel>();
        }
        
        public RegistrationStorage(List<RegistrationViewModel> registrations)
        {
            SessionID = null;
            Registrations = registrations;
        }
        
        public RegistrationStorage()
        {
            SessionID = null;
            Registrations = new List<RegistrationViewModel>();
        }
        
        public bool IsEmpty()
        {
            return Registrations == null || Registrations.Count == 0;
        }
    }
}