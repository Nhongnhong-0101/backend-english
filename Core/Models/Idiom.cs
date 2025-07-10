using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Idiom
    {
        public string idiom { get; set; }
        public string meaning { get; set; }
        public Idiom(string idiom, string meaning)
        {
            this.idiom = idiom;
            this.meaning = meaning;
        }
    }
}
