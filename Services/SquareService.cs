using Microsoft.Extensions.Configuration;
using Square;
using Square.Apis;
using Square.Authentication;
using Square.Models;

public class SquareService
{
    IConfigurationRoot _config;
    SquareClient _client;
    
    public SquareService(IConfigurationRoot configurationRoot)
    {
        _config = configurationRoot;       
        
        Square.Environment env = Square.Environment.Production;

        if (_config["Square:Environment"] == "Sandbox")
        {
            env = Square.Environment.Sandbox;
        }

        _client = new SquareClient.Builder().BearerAuthCredentials
            (
                new BearerAuthModel.Builder(_config["Square:Token"])
                .Build()
            )
            .Environment(env)
            .Build(); 
    }
    
    public Dictionary<string, int> GetCureInventory()
    {
        Dictionary<string, int> CureTicketCounts = new Dictionary<string, int>();

        string catId = _client.CatalogApi.ListCatalog().Objects.Where(o => o.Type == "Category" && o.CategoryData.Name == "CURE").First().Id;

        var cureItems = _client.CatalogApi.ListCatalog().Objects.Where(o => o.ItemData.CategoryId == catId && o.ItemData.IsArchived == false).ToList();

        foreach(CatalogObject catObj in cureItems)
        {
            string name = catObj.ItemData.Name;
            int count = _client.InventoryApi.RetrieveInventoryCount(catObj.Id).Counts.Count;

            CureTicketCounts.Add(name, count);
            
            if(catObj.ItemData.Variations.Count > 1)
            {
                foreach(CatalogObject obj in catObj.ItemData.Variations)
                {
                    string objName = obj.ItemData.Name;
                    int objCount = _client.InventoryApi.RetrieveInventoryCount(obj.Id).Counts.Count;

                    CureTicketCounts.Add(objName, objCount);
                }
            }
        }

        return CureTicketCounts;
    }
}