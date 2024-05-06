using System;
using System.Collections.Generic;

namespace BlogDomain.Model;

public partial class Article : Entity
{
    public string Title { get; set; }
    
    public string? Text { get; set; }

    public DateTime Data { get; set; }

    public int Status { get; set; }

    public int CategoryId { get; set; }

    public int WriterId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Writer Writer { get; set; } = null!;
}
