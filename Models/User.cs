using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;

namespace RoomRental.Models;

public class User : IdentityUser 
{
    [Required(ErrorMessage = "Не указана фамилия")]
    [Display(Name = "Фамилия")]
    public string Surname { get; set; }

    [Required(ErrorMessage = "Не указано имя")]
    [Display(Name = "Имя")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Не указано отчество")]
    [Display(Name = "Отчество")]
    public string Lastname { get; set; }
    [NotMapped]
    [ValidateNever]
    public string SNL { get { return $"{Surname} {Name} {Lastname}"; } }

    [Required(ErrorMessage = "Не указана организация")]
    [Display(Name = "Организация пользователя")]
    public int OrganizationId { get; set; }

    [ValidateNever]
    public virtual Organization Organization { get; set; }
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
