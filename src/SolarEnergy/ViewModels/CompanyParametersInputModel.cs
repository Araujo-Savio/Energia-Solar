namespace SolarEnergy.ViewModels
{
    public class CompanyParametersInputModel
    {
        public decimal PricePerKwp { get; set; } = 4200m;
        public decimal MaintenancePercent { get; set; } = 1.2m;
        public decimal InstallDiscountPercent { get; set; } = 4m;
        public decimal RentalFactorPercent { get; set; } = 68m;
        public decimal RentalMinMonthly { get; set; } = 250m;
        public decimal RentalSetupPerKwp { get; set; } = 150m;
        public decimal RentalAnnualIncreasePercent { get; set; } = 4.5m;
        public decimal RentalDiscountPercent { get; set; } = 15m;
        public decimal ConsumptionPerKwp { get; set; } = 120m;
        public decimal MinSystemSizeKwp { get; set; } = 2.5m;
    }
}
