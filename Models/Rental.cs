using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RoomRental.Attributes;
using System.ComponentModel.DataAnnotations;

namespace RoomRental.Models;

public partial class Rental
{
    [Display(Name = "Идентификатор")]
    public int RentalId { get; set; }
    [Required(ErrorMessage = "Не указано помещение")]
    [Display(Name = "Помещение")]
    public int RoomId { get; set; }
    [Required(ErrorMessage = "Не указана организация-арендатор")]
    [Display(Name = "Организация-арендатор")]
    public int RentalOrganizationId { get; set; }

    [Required(ErrorMessage = "Не указана дата начала аренды")]
    [Display(Name = "Дата начала аренды")]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = "Не указана дата окончания аренды")]
    [Display(Name = "Дата окончания аренды")]
    [DataType(DataType.Date)]
    [CheckDate("CheckInDate")]
    public DateTime CheckOutDate { get; set; }

    [Required(ErrorMessage = "Не указана сумма оплаты")]
    [Display(Name = "Сумма оплаты")]
    [Range(0, double.MaxValue, ErrorMessage = "Значение не может быть меньше нуля")]
    public decimal Amount { get; set; }

    [Display(Name = "Организация-арендатор")]
    [ValidateNever]
    public virtual Organization RentalOrganization { get; set; }

    [ValidateNever]
    public virtual Room Room { get; set; }
}
