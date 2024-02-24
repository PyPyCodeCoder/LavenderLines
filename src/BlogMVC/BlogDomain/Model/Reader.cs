using System;
using System.Collections.Generic;

namespace BlogDomain.Model;

public partial class Reader : Entity
{
    public string Username { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
