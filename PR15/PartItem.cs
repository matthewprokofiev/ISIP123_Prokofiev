namespace PR15
{
    public class PartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; } // Добавлено описание
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public string Category { get; set; }
        public basepart_ BasePart { get; set; }
    }
}