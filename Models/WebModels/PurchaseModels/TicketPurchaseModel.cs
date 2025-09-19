using System;

namespace KiCData.Models.WebModels.PurchaseModels
{
    public interface ITicketPurchaseModel : IPurchaseModel
    {
        int EventId { get; set; }
        double Price { get; set; }
        string Type { get; set; }
        DateOnly StartDate { get; set; }
        DateOnly EndDate { get; set; }
    }
    
    public class TicketPurchaseModel : ITicketPurchaseModel
    {
        public int EventId { get; set; }
        public double Price { get; set; }
        public string Type { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public TicketPurchaseModel(int eventId, double price, string type, int qtyTickets, DateOnly startDate, DateOnly endDate)
        {
            EventId = eventId;
            Price = price;
            Type = type;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
    
    public class CureTicketPurchaseModel : ITicketPurchaseModel
    {
        public int EventId { get; set; }
        public double Price { get; set; }
        public string Type { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        
        //TODO: Add additional properties specific to Cure tickets

        public CureTicketPurchaseModel(int eventId, double price, string type, int qtyTickets, DateOnly startDate, DateOnly endDate)
        {
            EventId = eventId;
            Price = price;
            Type = type;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}