namespace CarFix.Application.DTOs.ServiceCenter
{
    public class CenterSpecialtyResponseDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}