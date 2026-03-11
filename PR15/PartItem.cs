// PartItem.cs
using PR15;
using System.ComponentModel;

namespace PCBuilder
{
    public class PartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public string Category { get; set; }
        // Храним ссылку на оригинальную сущность для проверок
        public basepart_ BasePart { get; set; }
    }
}