using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class IrregularVerb
    {
        private string infinitive;
        private string pastSimple;
        private string pastParticiple;
        private string meaning;
        public IrregularVerb(string Infinitive, string PastSimple, string PastParticiple, string Meaning)
        {
            this.infinitive = Infinitive;
            this.pastSimple = PastSimple;
            this.pastParticiple = PastParticiple;
            this.meaning = Meaning;
        }
    }
}
