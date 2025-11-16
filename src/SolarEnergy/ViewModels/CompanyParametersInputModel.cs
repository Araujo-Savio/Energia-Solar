namespace SolarEnergy.ViewModels
{
    public class CompanyParametersInputModel
    {
        public decimal SystemPricePerKwp { get; set; }
        public decimal MaintenancePercent { get; set; }
        public decimal InstallDiscountPercent { get; set; }
        public decimal RentalFactorPercent { get; set; }
        public decimal RentalMinMonthly { get; set; }
        public decimal RentalSetupPerKwp { get; set; }
        public decimal RentalAnnualIncreasePercent { get; set; }
        public decimal RentalDiscountPercent { get; set; }
        public decimal ConsumptionPerKwp { get; set; }
        public decimal MinSystemSizeKwp { get; set; }
    }
}
