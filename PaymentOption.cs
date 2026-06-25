using System.ComponentModel.DataAnnotations;

namespace TmsApi.Services;

public class PaymentOptions
{
    [Required(AllowEmptyStrings = false)]
    public required string GatewayUrl { get; init; }

    [Range(100, 100000, ErrorMessage = "MaxDepositBirr must be between 100 and 100,000 Birr.")]
    public required decimal MaxDepositBirr { get; init; }
}