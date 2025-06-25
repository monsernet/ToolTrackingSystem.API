using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Models.DTOs.ToolIssuances
{
    public class IssuanceResponseDto
    {
        public int Id { get; }
        public string IssuanceNumber { get; }
        public int ToolId { get; }
        public string ToolName { get; }
        public int IssuedToId { get; }
        public string IssuedToName { get; } 
        public DateTime IssuedDate { get; }
        public DateTime? ExpectedReturnDate { get; }
        public DateTime? ActualReturnDate { get; }
        public string Status { get; }
        public string Purpose { get; }
        public string? Notes { get; }

        public IssuanceResponseDto(ToolIssuance issuance)
        {
            Id = issuance.Id;
            IssuanceNumber = issuance.IssuanceNumber;
            ToolId = issuance.ToolId;
            ToolName = issuance.Tool?.Name ?? string.Empty;
            IssuedToId = issuance.IssuedToId;
            IssuedToName = issuance.IssuedTo?.FirstName + ' ' + issuance.IssuedTo?.LastName ?? string.Empty;
            IssuedDate = issuance.IssuedDate;
            ExpectedReturnDate = issuance.ExpectedReturnDate;
            ActualReturnDate = issuance.ActualReturnDate;
            Status = issuance.Status.ToString();
            Purpose = issuance.Purpose;
            Notes = issuance.Notes;
        }
    }
}
