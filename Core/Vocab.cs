using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Vocab
    {
        public Guid vocabId { get; set; }
        public String vocab{ get; set; }
        public String primaryMeaningVi{ get; set; }
        public String primaryMeaningEn{ get; set; }
        public String phonetic{ get; set; }
        public String audioUrl { get; set; }

        public Vocab()
        {

        }
        public Vocab(Guid vocabId, string vocab, string primaryMeaningVi, string primaryMeaningEn, string phonetic, string audioUrl)
        {
            this.vocabId = vocabId;
            this.vocab = vocab;
            this.primaryMeaningVi = primaryMeaningVi;
            this.primaryMeaningEn = primaryMeaningEn;
            this.phonetic = phonetic;
            this.audioUrl = audioUrl;
        }
    }

}
