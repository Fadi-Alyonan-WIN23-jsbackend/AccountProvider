﻿using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class UserAddress
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string PostalCode { get; set; } = null!;

    public string City { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public UserAccount Users { get; set; } = null!;
}
