﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lingva.WebAPI.Dto
{
    public class SubtitleRowDTO
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public string LanguageName { get; set; }

        public int SubtitleId { get; set; }
    }
}
