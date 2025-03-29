using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string audioUrl { get; set; }
        public string? imageUrl { get; set; }

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
