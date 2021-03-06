using System;
using System.Collections.Generic;

namespace RestEaseClientGeneratorConsoleApp.Examples.SpeechServices.Models
{
    public class Model
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public Model BaseModel { get; set; }

        public Dataset[] Datasets { get; set; }

        public string ModelKind { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public object Properties { get; set; }

        public string Locale { get; set; }

        public DateTime LastActionDateTime { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }
}
