using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Domain.Models.AlbumModels
{
    public class FilterAdvanced
    {
        public string? FirstChar { get; set; }
        public string? Genre { get; set; }
        public string? Country { get; set; }
        public string? Year { get; set; }
        public bool? Status { get; set; }
        public string? Language { get; set; }
        public string? Rating { get; set; }
    }
}
