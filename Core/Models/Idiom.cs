using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Idiom
    {
        private string idiom;
        private string meaning;
        public Idiom(string idiom, string Meaning)
        {
            this.idiom = idiom;
            meaning = Meaning;
        }
    }
}
