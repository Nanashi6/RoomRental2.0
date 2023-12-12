using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using RoomRental.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRental.Models;

public partial class Invoice
{
    [Display(Name = "Идентификатор")]
    public int InvoiceId { get; set; }

    [Required(ErrorMessage = "Не указана организация-арендатор")]
    [Display(Name = "Организация-арендатор")]
    public int RentalOrganizationId { get; set; }

    [Required(ErrorMessage = "Не указано помещение")]
    [Display(Name = "Помещение")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Не указана сумма оплаты")]
    [Display(Name = "Сумма оплаты")]
    [Range(0, double.MaxValue, ErrorMessage = "Значение не может быть меньше нуля")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Не указана дата заключения договора")]
    [Display(Name = "Дата заключения договора")]
    [DataType(DataType.Date)]
    public DateTime ConclusionDate { get; set; }

    [Required(ErrorMessage = "Не указана дата оплаты")]
    [Display(Name = "Дата оплаты")]
    [DataType(DataType.Date)]
    [CheckDate("ConclusionDate")]
    public DateTime PaymentDate { get; set; }

    [Required(ErrorMessage = "Не указан оформляющий")]
    [Display(Name = "Оформляющий")]
    public string ResponsiblePersonId { get; set; }

    [Display(Name = "Организация-арендатор")]
    [ValidateNever]
    public virtual Organization RentalOrganization { get; set; }

    [Display(Name = "Оформляющий")]
    [ValidateNever]
    public virtual User ResponsiblePerson { get; set; } = null!;

    [ValidateNever]
    public virtual Room Room { get; set; } = null!;
}

// Ключ для аренд-счетов --- Помещение + организация + дата заключения договора