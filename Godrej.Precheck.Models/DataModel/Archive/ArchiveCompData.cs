using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Godrej.Precheck.Models.DataModel.Archive
{
    /// <summary>
    /// Model for archive COMP data table
    /// </summary>
    [Table("tbl_archive_comp_data")]
    public class ArchiveCompData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DrawingCompMappingId { get; set; }

        [MaxLength(200)]
        public string DrawingNumber { get; set; }

        [MaxLength(50)]
        public string CompTableName { get; set; }

        [MaxLength(200)]
        public string Nomenclature { get; set; }

        [MaxLength(15)]
        public string IDNos { get; set; }

        [MaxLength(20)]
        public string IRNos { get; set; }

        [MaxLength(20)]
        public string MSNNos { get; set; }

        [MaxLength(100)]
        public string ConsumedInOriginal { get; set; }

        [MaxLength(150)]
        public string Remarks { get; set; }

        [MaxLength(150)]
        public string Quantity { get; set; }

        [MaxLength(20)]
        public string MyDate { get; set; }

        [MaxLength(5)]
        public string SrNos { get; set; }

        [MaxLength(10)]
        public string UserName { get; set; }

        [MaxLength(100)]
        public string ConsumedInAssembly { get; set; }

        [MaxLength(50)]
        public string ConsumedInProdSeries { get; set; }

        [MaxLength(50)]
        public string ConsumedInId { get; set; }

        [MaxLength(50)]
        public string ComponentId { get; set; }

        [MaxLength(100)]
        public string AssemblyId { get; set; }

        [MaxLength(50)]
        public string ItemCode { get; set; }

        [MaxLength(10)]
        public string ComponentType { get; set; }

        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation property
        public virtual DrawingCompMapping DrawingCompMapping { get; set; }
    }
}
