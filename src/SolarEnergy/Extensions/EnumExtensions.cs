using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SolarEnergy.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.Name ?? enumValue.ToString();
        }

        public static string GetServiceTypeDisplayName(this SolarEnergy.Models.SolarServiceType? serviceType)
        {
            if (serviceType == null) return "Não informado";
            
            return serviceType.Value switch
            {
                SolarEnergy.Models.SolarServiceType.PanelSales => "Venda de Painéis",
                SolarEnergy.Models.SolarServiceType.EnergyRental => "Aluguel de Energia",
                SolarEnergy.Models.SolarServiceType.Both => "Venda e Aluguel",
                _ => "Não informado"
            };
        }

        public static string GetServiceTypeIcon(this SolarEnergy.Models.SolarServiceType? serviceType)
        {
            if (serviceType == null) return "fas fa-question-circle";
            
            return serviceType.Value switch
            {
                SolarEnergy.Models.SolarServiceType.PanelSales => "fas fa-shopping-cart",
                SolarEnergy.Models.SolarServiceType.EnergyRental => "fas fa-handshake",
                SolarEnergy.Models.SolarServiceType.Both => "fas fa-star",
                _ => "fas fa-question-circle"
            };
        }

        public static string GetServiceTypeColor(this SolarEnergy.Models.SolarServiceType? serviceType)
        {
            if (serviceType == null) return "bg-secondary";
            
            return serviceType.Value switch
            {
                SolarEnergy.Models.SolarServiceType.PanelSales => "bg-primary",
                SolarEnergy.Models.SolarServiceType.EnergyRental => "bg-success", 
                SolarEnergy.Models.SolarServiceType.Both => "bg-warning",
                _ => "bg-secondary"
            };
        }
    }
}