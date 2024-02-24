using System;
using System.Collections.Generic;

namespace BlogDomain.Model;

public partial class Category : Entity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
