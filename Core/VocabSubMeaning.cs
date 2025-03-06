using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class VocabSubMeaning
    {
        public Guid meaningId;
        public Guid vocabId;
        public String partOfSpeech;
        public String meaningEn;
        public String meaningVi;
        public String example;
        public String vocab;
        public String imageUrl; //It is not necessary,  just empty string

        public VocabSubMeaning()
        {

        }

        public VocabSubMeaning(Guid meaningId, Guid vocabId, string partOfSpeech, string meaningEn, string meaningVi, string example, string word, string imageUrl)
        {
            this.meaningId = meaningId;
            this.vocabId = vocabId;
            this.partOfSpeech = partOfSpeech;
            this.meaningEn = meaningEn;
            this.meaningVi = meaningVi;
            this.example = example;
            this.vocab = word;
            this.imageUrl = imageUrl;
        }
    }
}
