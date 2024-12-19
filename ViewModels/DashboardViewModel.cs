using JewelEase.Models;

namespace JewelEase.ViewModels
{
    public class DashboardViewModel
    {
        public int CustomersCount { get; set; }
        public int PendingAppointments { get; set; }
        public decimal TotalSales { get; set; }
        public int PendingQuotations { get; set; }
    }
}
