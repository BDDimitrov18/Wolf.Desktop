using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class File
{
    public int Fileid { get; set; }

    public string Filename { get; set; } = null!;

    public string Filepath { get; set; } = null!;

    public DateTime Uploadedat { get; set; }

}
