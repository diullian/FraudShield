using FraudShield.Communication.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Communication.Requests;

public class CustomerRequest
{
    public string Document { get; set; }
    public string Email { get; set; }
    public string IpAddress { get; set; }
    public string Country { get; set; }
    public DeviceType DeviceType { get; init; }
}
