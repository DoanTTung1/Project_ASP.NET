using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class Genre
{
    public int GenreId { get; set; }

    public string Name { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
