using ForbiddenKnowledge.Data.DbModels.Audit;
using Microsoft.EntityFrameworkCore;

namespace ForbiddenKnowledge.Data
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

        public DbSet<AuditTrail> AuditTrails { get; set; }




        //The DbSets below are NOT tables in the DB. These are stateless, "lightweight" DTOs, such as SP result containers.
        //As such, these models/entities are records, not classes.

        public DbSet<DTOBlogPostPageViewCount> BlogPostPageViewCounts { get; set; }
    }
}
