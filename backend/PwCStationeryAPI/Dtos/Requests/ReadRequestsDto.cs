namespace PwCStationeryAPI.Dtos.Requests
{
    public class ReadRequestDto
    {
        public int Id { get; set; }

        public string ItemName { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        public DateTime? CreatedAt { get; set; } // map if you have it
    }
}
