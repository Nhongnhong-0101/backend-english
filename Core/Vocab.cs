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
        public string vocab;
        public string primaryMeaningVi;
        public string primaryMeaningEn;
        public string phonetic;
        public string audioUrl;
        public string partOfSpeech;

        public Vocab()
        {

        }
        public Vocab(Guid vocabId, string vocab, string primaryMeaningVi, string primaryMeaningEn, string phonetic, string audioUrl, string pos)
        {
            this.vocabId = vocabId;
            this.vocab = vocab;
            this.primaryMeaningVi = primaryMeaningVi;
            this.primaryMeaningEn = primaryMeaningEn;
            this.phonetic = phonetic;
            this.audioUrl = audioUrl;
            this.partOfSpeech = pos;
        }
    }

}
