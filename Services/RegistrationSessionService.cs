using System.Text.Json;
using KiCData.Models.WebModels;
using Microsoft.AspNetCore.Http;

namespace KiCData.Services
{
    /// <summary>
    /// Service for handling storage-related operations.
    /// </summary>
    public class RegistrationSessionService
    {
        public List<SessionRegistration>? Registrations { get; set; }
        
        public RegistrationSessionService()
        {
            Registrations = new List<SessionRegistration>();
        }
        
        public bool IsEmpty()
        {
            return Registrations is null || Registrations.Count == 0;
        }
        
        public void AddRegistration(string Key, RegistrationViewModel registrationViewModel)
        {
            SessionRegistration sessionRegistration = new SessionRegistration(Key, registrationViewModel);
            Registrations.Add(sessionRegistration);
        }
        
        public void RemoveRegistrations(string Key)
        {
            foreach(SessionRegistration sr in Registrations)
            {
                if(sr.SessionKey == Key)
                {
                    Registrations.Remove(sr);
                }
            }
        }
        
        public List<RegistrationViewModel> GetRegistrationsForUser(string SessionID)
        {
            List<RegistrationViewModel> viewModels = new List<RegistrationViewModel>();
            
            foreach(SessionRegistration sr in Registrations)
            {
                if(sr.SessionKey == SessionID)
                {
                    viewModels.Add(sr.Registration);
                }
            }

            return viewModels;
        }
    }
    
    public class SessionRegistration
    {
        public String? SessionKey{ get; set; }
        
        public RegistrationViewModel? Registration {get;set;}
        
        public SessionRegistration()
        {
            
        }
        
        public SessionRegistration(string sessionKey, RegistrationViewModel registrationViewModel)
        {
            SessionKey = sessionKey;
            Registration = registrationViewModel;
        }
    }
}