using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountProvider.Models;

public class UserAddressModel
{
    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string PostalCode { get; set; } = null!;

    public string City { get; set; } = null!;
    public string UserId { get; set; } = null!;
}
