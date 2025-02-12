﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;

namespace API.Entities
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public bool IsMain { get; set; }
        public string? PublicId { get; set; }
        public bool IsApproved { get; set; }
        //Navigation prop
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;
    }
}