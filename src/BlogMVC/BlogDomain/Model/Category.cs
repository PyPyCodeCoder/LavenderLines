using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogDomain.Model;

public partial class Category : Entity
{
    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Категорія")]
    public string Name { get; set; } = null!;

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
