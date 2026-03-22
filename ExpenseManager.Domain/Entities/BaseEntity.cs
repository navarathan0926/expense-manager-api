using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseManager.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; } 
    }
}