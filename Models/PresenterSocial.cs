using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KiCData.Models
{
  [Table("PresenterSocial")]
  public class PresenterSocial
  {
      public int Id { get; set; }

      public string Platform { get; set; } = default!;

      public string Handle { get; set; } = default!;

      public int PresenterId { get; set; }

      [JsonIgnore]
      public Presenter Presenter { get; set; } = default!;
  }
}