using System.ComponentModel.DataAnnotations;

namespace Portal.Domain.Models.ContentItemModels
{
    public class ContentItemUploadLocalServer
    {
        public int? Id { get; set; }
        public bool IsPublic { get; set; }

        [Required(ErrorMessage = "error_content_item_upload_local_server_file_name_required")]
        public string FileName { get; set; } = null!;
    }
}
