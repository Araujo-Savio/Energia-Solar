namespace SolarEnergy.ViewModels
{
    public class CompanyParametersInputModel
    {
        public decimal PricePerKwP { get; set; }
        public decimal AnnualMaintenance { get; set; }
        public decimal InstallationDiscount { get; set; }
        public decimal RentalPercent { get; set; }
        public decimal MinRentalValue { get; set; }
        public decimal RentalSetupFee { get; set; }
        public decimal AnnualRentIncrease { get; set; }
        public decimal RentDiscount { get; set; }
        public decimal KwhPerKwp { get; set; }
        public decimal MinSystemPower { get; set; }
    }
}
