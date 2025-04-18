using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForbiddenKnowledge.Data
{

    [Keyless]
    public record DTOBlogPostPageViewCount
    {
        [Column("count")]
        public int Count { get; set; }
    }

}