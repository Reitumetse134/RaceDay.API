namespace RaceDay.API.DTOs
{
    public class CategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Gender { get; set; } = "mixed";
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double Distance { get; set; }
        public decimal EntryFee { get; set; }
    }
}