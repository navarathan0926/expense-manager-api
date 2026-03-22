using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string UserName { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}