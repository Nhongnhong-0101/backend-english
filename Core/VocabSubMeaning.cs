using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class VocabSubMeaning
    {
        private Guid meaningId;
        private Guid vocabId;
        private string partOfSpeech;
        private string meaningEn;
        private string meaningVi;
        private string example;
        private string audioUrl;
        private string imageUrl; //It is not necessary,  just empty string

        public VocabSubMeaning()
        {

        }

        public VocabSubMeaning(Guid meaningId, Guid vocabId, string partOfSpeech, string meaningEn, string meaningVi, string example, string audioUrl, string imageUrl)
        {
            this.meaningId = meaningId;
            this.vocabId = vocabId;
            this.partOfSpeech = partOfSpeech;
            this.meaningEn = meaningEn;
            this.meaningVi = meaningVi;
            this.example = example;
            this.audioUrl = audioUrl;
            this.imageUrl = imageUrl;
        }
    }
}
