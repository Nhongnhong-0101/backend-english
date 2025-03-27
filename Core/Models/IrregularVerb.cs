using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class IrregularVerb
    {
        private string infinitive;
        private string pastSimple;
        private string pastParticiple;
        private string meaning;
        public IrregularVerb(string Infinitive, string PastSimple, string PastParticiple, string Meaning)
        {
            infinitive = Infinitive;
            pastSimple = PastSimple;
            pastParticiple = PastParticiple;
            meaning = Meaning;
        }
    }
}
