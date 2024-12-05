using ForbiddenKnowledge.Model.Audit;
using Microsoft.EntityFrameworkCore;

namespace ForbiddenKnowledge.Data
{
	public class AuditDbContext : DbContext
	{
		public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

		public DbSet<AuditTrail> AuditTrails { get; set; }
	}
}
