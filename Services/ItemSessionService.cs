using System.Text.Json;
using KiCData.Models.WebModels.PurchaseModels;
using Microsoft.AspNetCore.Http;

namespace KiCData.Services
{
    public class ItemSessionService
    {
        private const string SessionKey = "Items";

        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public ItemSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public List<PurchaseItem> PurchaseItems
        {
            get
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return new List<PurchaseItem>();

                var json = session.GetString(SessionKey);
                return string.IsNullOrEmpty(json)
                    ? new List<PurchaseItem>()
                    : JsonSerializer.Deserialize<List<PurchaseItem>>(json) ?? new List<PurchaseItem>();
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
            return PurchaseItems == null || PurchaseItems.Count == 0;
        }
    }
}