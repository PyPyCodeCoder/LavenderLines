using System;
using System.Collections.Generic;

namespace BlogDomain.Model;

public partial class Writer : Entity
{
    public string Username { get; set; } = null!;

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
