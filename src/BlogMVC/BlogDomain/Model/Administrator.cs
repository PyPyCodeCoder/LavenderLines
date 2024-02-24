using System;
using System.Collections.Generic;

namespace BlogDomain.Model;

public partial class Administrator : Entity
{
    public string Username { get; set; } = null!;
}
