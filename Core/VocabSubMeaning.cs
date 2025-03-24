using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class VocabSubMeaning
    {
        public Guid meaningId { get; set; }
        public Guid vocabId { get; set; }
        public String partOfSpeech { get; set; }
        public String meaningEn { get; set; }
        public String meaningVi { get; set; }
        public String example { get; set; }
        public String vocab { get; set; }
        public String imageUrl { get; set; } //It is not necessary,  just empty strin 

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
