using System.ComponentModel.DataAnnotations;

namespace RoomRental.ViewModels.IdentityViewModels
{
    public class ChangePasswordViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
