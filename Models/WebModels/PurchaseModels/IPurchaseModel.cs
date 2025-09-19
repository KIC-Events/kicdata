using System;

namespace KiCData.Models.WebModels.PurchaseModels
{
    public interface IPurchaseModel
    {
        double Price { get; set; }   
        string Type { get; set; }
    }
}