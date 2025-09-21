using KiCData.Migrations;
using KiCData.Models;
using KiCData.Models.WebModels;

namespace KiCData.Services
{
    public class InventoryService
    {
        private readonly KiCdbContext _context;
        private readonly IKiCLogger _logger;
        
        public InventoryService(KiCdbContext kiCdbContext, IKiCLogger kiCLogger)
        {
            _context = kiCdbContext;
            _logger = kiCLogger;
        }
        
        /// <summary>
        /// Checks the inventory count for a specific item by name.
        /// </summary>
        /// <param name="itemName">The name of the item to check inventory for.</param>
        /// <returns>The stock count as an integer, or null if not found.</returns>
        public int? CheckInventory(string itemName)
        {
            return checkInventory(itemName);
        }
        
        /// <summary>
        /// Internal method to check inventory count for a specific item.
        /// </summary>
        /// <param name="itemName">The name of the item to check inventory for.</param>
        /// <returns>The stock count as an integer, or null if not found.</returns>
        private int? checkInventory(string itemName)
        {
            try
            {
                int? count = _context.InventoryItems
                .Where(i => i.Name == itemName)
                .FirstOrDefault()
                .Stock;

                return count;
            }
            catch(Exception ex)
            {
                _logger.Log(ex);
                return 0;
            }
        }
        
        /// <summary>
        /// Asynchronously retrieves a list of inventory items of a specific type.
        /// </summary>
        /// <param name="itemType">The type of items to retrieve.</param>
        /// <returns>A list of InventoryItem objects.</returns>
        public async Task<List<InventoryItem>> GetItemInventoryAsync(string itemType)
        {
            return await Task.Run(() => getItemInventory(itemType));
        }
        
        /// <summary>
        /// Internal asynchronous method to get inventory items by type.
        /// </summary>
        /// <param name="itemType">The type of items to retrieve.</param>
        /// <returns>A list of InventoryItem objects.</returns>
        private async Task<List<InventoryItem>> getItemInventory(string itemType)
        {
            List<InventoryItem> items = _context.InventoryItems
                .Where(i => i.Type == itemType)
                .ToList();

            return items;
        }
        
        /// <summary>
        /// Gets the price of a ticket item by name.
        /// </summary>
        /// <param name="itemName">The name of the ticket item.</param>
        /// <returns>The price as a double.</returns>
        public double GetTicketPrice(string itemName)
        {
            return getTicketPrice(itemName);
        }
        
        /// <summary>
        /// Internal method to get the price of a ticket item by name.
        /// </summary>
        /// <param name="itemName">The name of the ticket item.</param>
        /// <returns>The price as a double.</returns>
        private double getTicketPrice(string itemName)
        {
            try
            {
                InventoryItem? item = _context.InventoryItems
                    .Where(i => i.Name == itemName)
                    .FirstOrDefault();

                double price = (double)item.PriceInCents / 100;

                return price;
            }
            catch(Exception ex)
            {
                _logger.Log(ex);
                return 0.0;
            }
        }
        
        /// <summary>
        /// Asynchronously adjusts inventory for a list of registrations, either incrementing or decrementing stock.
        /// </summary>
        /// <param name="registrationViewModels">List of registration view models.</param>
        /// <param name="increment">If true, increases inventory; otherwise, decreases inventory.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public Task AdjustInventoryAsync(List<RegistrationViewModel> registrationViewModels, bool increment = false)
        {
            if (increment) return Task.Run(() => increaseInventory(registrationViewModels));
            else return Task.Run(() => reduceInventory(registrationViewModels));
        }
        
        /// <summary>
        /// Internal asynchronous method to reduce inventory for a list of registrations.
        /// </summary>
        /// <param name="registrationViewModels">List of registration view models.</param>
        private async void reduceInventory(List<RegistrationViewModel> registrationViewModels)
        {
            foreach(RegistrationViewModel rvm in registrationViewModels)
            {
                InventoryItem? item = _context.InventoryItems
                    .Where(i => i.Name == rvm.TicketType)
                    .FirstOrDefault();

                if (item is null) throw new Exception("A bad item in the cart.");

                item.Stock--;

                _context.Update(item);
                
                if(rvm.MealAddon is not null)
                {
                    InventoryItem addon = _context.InventoryItems
                        .Where(i => i.Name == "Decadent Delights")
                        .First();

                    addon.Stock--;

                    _context.Update(addon);
                }
            }

            _context.SaveChanges();
        }
        
        /// <summary>
        /// Internal asynchronous method to increase inventory for a list of registrations.
        /// </summary>
        /// <param name="registrationViewModels">List of registration view models.</param>
        private async void increaseInventory(List<RegistrationViewModel> registrationViewModels)
        {
            foreach(RegistrationViewModel rvm in registrationViewModels)
            {
                InventoryItem? item = _context.InventoryItems
                    .Where(i => i.Name == rvm.TicketType)
                    .FirstOrDefault();

                if (item is null) throw new Exception("A bad item in the cart.");

                item.Stock++;

                _context.Update(item);
                
                if(rvm.MealAddon is not null)
                {
                    InventoryItem addon = _context.InventoryItems
                        .Where(i => i.Name == "Decadent Delights")
                        .First();

                    addon.Stock++;

                    _context.Update(addon);
                }
            }

            _context.SaveChanges();
        }
        
        /// <summary>
        /// Asynchronously retrieves the add-on inventory item ("Decadent Delights").
        /// </summary>
        /// <returns>The InventoryItem object for the add-on.</returns>
        public async Task<InventoryItem> GetAddonItemAsync()
        {
            return await Task.Run(() => GetAddonItem());
        }
        
        /// <summary>
        /// Internal asynchronous method to get the add-on inventory item ("Decadent Delights").
        /// </summary>
        /// <returns>The InventoryItem object for the add-on.</returns>
        private async Task<InventoryItem> GetAddonItem()
        {
            InventoryItem item = _context.InventoryItems
                .Where(i => i.Name == "Decadent Delights")
                .First();

            return item;
        }
    }
}