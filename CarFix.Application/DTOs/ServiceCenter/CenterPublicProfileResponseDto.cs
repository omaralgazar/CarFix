namespace CarFix.Application.DTOs.ServiceCenter
{
    public class CenterPublicProfileResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public decimal Rating { get; set; }
        public List<CenterCapabilityResponseDto>? Capabilities { get; set; } = new();
    }
}