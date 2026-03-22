using System;

namespace ExpenseManager.Application.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
