using System;
using System.Collections.Generic;

namespace BlogDomain.Model;

public partial class Reader : Entity
{
    public string Username { get; set; } = null!;

    public string UserId { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
