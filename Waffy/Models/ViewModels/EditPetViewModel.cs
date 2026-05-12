namespace Waffy.Models.ViewModels
{
    /// <summary>
    /// يُستخدم في EditPetPage لعرض بيانات الحيوان + صوره الموجودة معاً
    /// </summary>
    public class EditPetViewModel
    {
        public Pet Pet { get; set; } = new();
        public List<PetImage> Images { get; set; } = new();
    }
}