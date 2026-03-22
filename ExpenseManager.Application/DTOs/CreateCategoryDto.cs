using System;

namespace ExpenseManager.Application.DTOs
{
    public class CreateCategoryDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
