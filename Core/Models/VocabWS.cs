using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class VocabWS
    {
        public Guid wsId { get; set; }
        public string vocab { get; set; }
        public string primaryMeaningVi { get; set; }
        public string primaryMeaningEn { get; set; }
        public bool isStar { get; set; } = false;

        public VocabWS() { }
        public VocabWS(Guid wsId, string vocab, string primaryMeaningVi, string primaryMeaningEn, bool isStar)
        {
            this.wsId = wsId;
            this.vocab = vocab;
            this.primaryMeaningVi = primaryMeaningVi;
            this.primaryMeaningEn = primaryMeaningEn;
            this.isStar = isStar;
        }
    }
}
