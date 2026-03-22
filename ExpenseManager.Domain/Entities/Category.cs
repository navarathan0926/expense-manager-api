namespace ExpenseManager.Domain.Entities
{
	public class Category : BaseEntity
	{
		public required string Name { get; set; }
		public string? Description { get; set; }
		public bool IsPredefined { get; set; } = false;
		public Guid? UserId { get; set; }
		public User? User { get; set; }
		public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
	}
}
