public interface IInteractable
{
    string GetInteractPrompt(); // Trả về text hiển thị (vd: "Nhấn E để mở cửa")
    void Interact();            // Hành động xảy ra khi bấm E
}