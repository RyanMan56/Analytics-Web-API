﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Entities;

namespace Analytics.Models
{
    public class ProjectDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<AnalyserDto> Analysers { get; set; } = new List<AnalyserDto>();
        public string Url { get; set; }
        public string ApiKey { get; set; }
    }
}
