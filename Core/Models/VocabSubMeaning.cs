using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class VocabSubMeaning
    {
        public Guid meaningId { get; set; }
        public Guid vocabId { get; set; }
        public string partOfSpeech { get; set; }
        public string meaningEn { get; set; }
        public string meaningVi { get; set; }
        public string example { get; set; }
        public string vocab { get; set; }
        public string imageUrl { get; set; } //It is not necessary,  just empty strin 

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
            vocab = word;
            this.imageUrl = imageUrl;
        }
    }
}
