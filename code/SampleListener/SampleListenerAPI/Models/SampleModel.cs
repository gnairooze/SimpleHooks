using System.ComponentModel.DataAnnotations;

namespace SampleListenerAPI.Models
{
    public class SampleModel
    {
        [Key]
        //identity column
        public long Id { get; set; }
        [Required]
        public string EventName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string CorrelationId { get; set; } = string.Empty;
        
        public string EventData { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
