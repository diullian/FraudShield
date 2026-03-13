using FraudShield.Domain.Enums;

namespace FraudShield.Domain.Entities;


public class Customer
{
    public Guid Id { get; set; }
    public string Document { get; set; }
    public string Email { get; set; }
    public string IpAddress { get; set; }
    public string Country { get; set; }

    public DeviceType DeviceType { get; init; }

}
