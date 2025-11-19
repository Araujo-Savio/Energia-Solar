namespace SolarEnergy.ViewModels
{
    public class CompanyParametersViewModel
    {
        public decimal PricePerKwp { get; set; }
        public decimal AnnualMaintenancePercent { get; set; }
        public decimal InstallationDiscountPercent { get; set; }
        public decimal MonthlyFeeFactorPercent { get; set; }
        public decimal MinimumMonthlyFee { get; set; }
        public decimal SetupFeePerKwp { get; set; }
        public decimal AnnualRentAdjustmentPercent { get; set; }
        public decimal BillDiscountPercent { get; set; }
        public decimal MonthlyConsumptionPerKwp { get; set; }
        public decimal MinimumSystemPowerKwp { get; set; }
    }
}
