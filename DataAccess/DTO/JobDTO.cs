﻿using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.DTO
{
    [AutoMap(typeof(Job))]
    public class JobDTO
    {
        public int JobId { get; set; }
        public int UserId { get; set; }
        public int JobStatusId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectFileId { get; set; }
        public string JobDescription { get; set; }
        public int SchemaId { get; set; }
        public string JobInstruction { get; set; }
    }
}
