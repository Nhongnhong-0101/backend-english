using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Vocab
    {
        public Guid vocabId;
        public String vocab;
        public String primaryMeaningVi;
        public String primaryMeaningEn;
        public String phonetic;
        public String audioUrl;

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
