using System;
using System.Collections.Generic;

namespace BlogDomain.Model;

public partial class Writer : Entity
{
    public string Username { get; set; } = null!;

    public string UserId { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
