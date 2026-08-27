using Godrej.Precheck.Models.DTOs.Testing;

namespace Godrej.Precheck.Repository.Repository.TestingRepository
{
    public interface ITestingRepository
    {
        /// <summary>
        /// Returns all components where precheck is fully completed (status = 3).
        /// </summary>
        Task<List<PrecheckCompletedComponentDto>> GetPrecheckCompletedComponentsAsync();
        /// <summary>
        /// Checks if a template exists and is active.
        /// </summary>
        Task<bool> CheckTemplateExistsAsync(int templateId);

        /// <summary>
        /// Gets all active fields for a given template id.
        /// </summary>
        Task<List<TemplateFieldDto>> GetTemplateFieldsByTemplateIdAsync(int templateId);

        /// <summary>
        /// Gets template fields by drawing number, prefilled with saved values for the given MSN instance (if any).
        /// </summary>
        Task<List<TemplateFieldDto>> GetTemplateFieldsByDrawingNumberAsync(string drawingNumber, string? msnNumber, int? stageId = null);

        /// <summary>
        /// Gets template id mapped to a drawing number.
        /// </summary>
        Task<int?> GetTemplateIdByDrawingNumberAsync(string drawingNumber);

        /// <summary>
        /// Gets drawing id by drawing number.
        /// </summary>
        Task<int?> GetDrawingIdByDrawingNumberAsync(string drawingNumber);

        /// <summary>
        /// Inserts inspection master and all field values in a single transaction.
        /// </summary>
        Task<int> InsertInspectionValuesAsync(
            int templateId,
            string drawingNumber,
            List<TemplateFieldValueInsertDto> values);


        Task<int?> GetMasterIdByDrawingNumberAsync(string drawingNumber);

        /// <summary>
        /// msnNumber is optional: pass it to target one specific inspection instance (the
        /// correct behavior for real export). Omit only for debug/dev tooling, which falls
        /// back to "latest instance for this drawing".
        /// </summary>
        Task<InspectionExportDataDto?> GetInspectionExportDataAsync(string drawingNumber, string? msnNumber = null);
        Task<List<FieldValueExportDto>> GetFieldValuesForExportAsync(string drawingNumber, string? msnNumber = null);

        /// <summary>
        /// Returns all drawing numbers with their stage-wise completion status.
        /// </summary>
        Task<List<DrawingStageStatusDto>> GetDrawingStageStatusAsync();

        /// <summary>
        /// Returns all is_row_field=1 fields for a specific template and stage.
        /// </summary>
        Task<List<TemplateFieldDto>> GetStageFieldsAsync(int templateId, int stageId);

        /// <summary>
        /// Returns all fixed (is_row_field=0) field definitions for a template.
        /// </summary>
        Task<List<TemplateFieldDto>> GetFixedFieldsAsync(int templateId);

        /// <summary>
        /// Returns saved fixed field values for one (drawing, MSN) inspection instance (row_number=0 in the child table).
        /// </summary>
        Task<List<StageRowValueRawDto>> GetFixedFieldValuesByDrawingAsync(string drawingNumber, string msnNumber);

        /// <summary>
        /// Returns saved row field values for one (drawing, MSN) inspection instance and stage, across all its data rows.
        /// </summary>
        Task<List<StageRowValueRawDto>> GetStageRowValuesByDrawingAsync(string drawingNumber, string msnNumber, int stageId);

        /// <summary>
        /// Saves fixed fields under the (drawing, MSN) instance's shared master (row_number=0 in the child table).
        /// Creates that master if it doesn't exist yet. Returns the master's id.
        /// </summary>
        Task<int> SaveFixedFieldsAsync(
            string drawingNumber,
            string msnNumber,
            List<StageRowValueInsertDto> fixedValues);

        /// <summary>
        /// Saves one inspection row's stage values under the (drawing, MSN) instance's shared master
        /// (row_number=rowNumber in the child table). Creates that master if it doesn't exist yet.
        /// Returns the master's id.
        /// </summary>
        Task<int> SaveRowDataAsync(
            string drawingNumber,
            string msnNumber,
            int stageId,
            int rowNumber,
            int totalRows,
            List<StageRowValueInsertDto> fieldValues);

        /// <summary>
        /// Returns tbl_inspection_master stage-completion status for one (drawing, MSN) inspection instance.
        /// Returns null if no master record exists yet.
        /// </summary>
        Task<InspectionMasterStatusDto?> GetInspectionMasterStatusAsync(string drawingNumber, string msnNumber);

        /// <summary>
        /// Returns fields with stageid=10 from tbl_template_fields with their saved values (if any) for one (drawing, MSN) instance.
        /// </summary>
        Task<List<HeaderFieldValueDto>> GetStage10FieldsWithValuesAsync(int templateId, string drawingNumber, string msnNumber);

        /// <summary>
        /// Returns all active field definitions (name, label, is_row_field) for the template
        /// mapped to the given drawing number. Use to verify template HTML id attributes match DB field names.
        /// </summary>
        Task<List<TemplateFieldDto>> GetFieldNamesForExportAsync(string drawingNumber);

        /// <summary>
        /// Returns all images stored in tbl_template_images for the given template id.
        /// Each image carries a placeholder_key (e.g. "IMAGE_1") and base64 image_data.
        /// </summary>
        Task<List<TemplateImageDto>> GetTemplateImagesAsync(int templateId);
    }
}
