using ForbiddenKnowledge.Data.DbModels;
using Microsoft.EntityFrameworkCore;



namespace ForbiddenKnowledge.Data
{
    public class ForbiddenKnowledgeContext : DbContext
    {
        public ForbiddenKnowledgeContext(DbContextOptions<ForbiddenKnowledgeContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // specify relationships for the BlogPostTag join table
            modelBuilder.Entity<BlogPostTag>(entity =>
            {
                entity.ToTable("blog_post_tag", "public"); 
                entity.HasKey(e => new { e.PostId, e.TagId }); 

                entity.HasOne(e => e.BlogPostTagLookup)       // Join BlogPostTag to BlogPostTagLookup
                    .WithMany()                               // No reverse navigation needed
                    .HasForeignKey(e => e.TagId)              
                    .OnDelete(DeleteBehavior.Cascade);        

                entity.HasOne<BlogPost>()                    // Join BlogPostTag to BlogPost
                    .WithMany(bp => bp.BlogPostTags)         // Reverse navigation to BlogPost's tags
                    .HasForeignKey(e => e.PostId)             
                    .OnDelete(DeleteBehavior.Cascade);        
            });
        }



        // The DbSets below represent real tables in the DB. The models/entities for these are classes.
        public DbSet<BlogPostCategoryLookup> BlogPostCategoryLookups { get; set; }

        public DbSet<BlogPostTagLookup> BlogPostTagLookups { get; set; }

        public DbSet<BlogPostTag> BlogPostTags { get; set; }

        public DbSet<BlogPost> BlogPosts { get; set; }

        public DbSet<User> Users { get; set; }




        //The DbSets below are NOT tables in the DB. These are stateless, "lightweight" DTOs, such as SP result containers.
        //As such, these models/entities are records, not classes.


    }
}
