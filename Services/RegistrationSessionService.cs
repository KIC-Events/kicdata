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
        private const string SessionKey = "Registrations";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegistrationSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<RegistrationViewModel> Registrations
        {
            get
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return new List<RegistrationViewModel>();

                var json = session.GetString(SessionKey);
                return string.IsNullOrEmpty(json)
                    ? new List<RegistrationViewModel>()
                    : JsonSerializer.Deserialize<List<RegistrationViewModel>>(json) ?? new List<RegistrationViewModel>();
            }
            set
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return;

                var json = JsonSerializer.Serialize(value);
                session.SetString(SessionKey, json);
            }
        }

        public void Clear() => _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);

        public bool IsEmpty()
        {
            var regs = Registrations;
            return regs == null || regs.Count == 0;
        }
    }
    
}