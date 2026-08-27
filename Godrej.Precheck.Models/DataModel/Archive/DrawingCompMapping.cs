using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Godrej.Precheck.Models.DataModel.Archive
{
    /// <summary>
    /// Model for drawing number to COMP table mapping
    /// </summary>
    [Table("tbl_drawing_comp_mapping")]
    public class DrawingCompMapping
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string DrawingNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string CompTableName { get; set; }

        [MaxLength(200)]
        public string Nomenclature { get; set; }

        [MaxLength(50)]
        public string ItemCode { get; set; }

        [MaxLength(10)]
        public string ComponentType { get; set; }

        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public virtual ICollection<ArchiveCompData> ArchiveCompData { get; set; } = new List<ArchiveCompData>();
    }
}
