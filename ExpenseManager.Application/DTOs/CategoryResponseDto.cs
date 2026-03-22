using System;

namespace ExpenseManager.Application.DTOs
{
    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPredefined { get; set; }
        public Guid? UserId { get; set; }
    }
}
