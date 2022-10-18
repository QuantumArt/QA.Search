using QA.Search.Generic.DAL.Services.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    public class GenericItem : IGenericItem
    {
        [Key]
        [Column("content_item_id")]
        public int ContentItemID { get; set; }
        [ForeignKey("status_type_id")]
        public StatusType StatusType { get; set; }
        [Column("visible")]
        public bool Visible { get; set; }
        [Column("archive")]
        public bool Archive { get; set; }
        [Column("created")]
        public DateTime Created { get; set; }
        [Column("modified")]
        public DateTime Modified { get; set; }
    }
}
