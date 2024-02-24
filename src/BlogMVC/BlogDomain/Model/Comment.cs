using System;
using System.Collections.Generic;

namespace BlogDomain.Model;

public partial class Comment : Entity
{
    public string? Text { get; set; }

    public DateTime Data { get; set; }

    public int Status { get; set; }

    public int ArticleId { get; set; }

    public int ReaderId { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Reader Reader { get; set; } = null!;
}
