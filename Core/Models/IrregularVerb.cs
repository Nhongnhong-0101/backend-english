using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class IrregularVerb
    {
        public string infinitive { get; set; }
        public string pastSimple { get; set; }
        public string pastParticiple { get; set; }
        public string meaning { get; set; }
        public IrregularVerb(string Infinitive, string PastSimple, string PastParticiple, string Meaning)
        {
            infinitive = Infinitive;
            pastSimple = PastSimple;
            pastParticiple = PastParticiple;
            meaning = Meaning;
        }
    }
}
