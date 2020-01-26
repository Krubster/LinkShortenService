using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkShortenService.Models
{
    public class UrlModel
    {
        public int ID { get; set; }
        public string OriginalURL { get; set; }
        public string ShortURL { get; set; }
        public DateTime Created { get; set; }
        public int Counter { get; set; }
    }
}
