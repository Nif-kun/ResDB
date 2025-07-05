using System;
using Godot;

namespace Addons.ResDB.UI;

[Tool]
public partial class DatabaseButton : Button {

    public Action DeletePressed;
    private Button _deleteButton;

    public override void _Ready() {
        _deleteButton = GetNode<Button>("DeleteBtn");
        _deleteButton.Hide();
        _deleteButton.Pressed += OnDeletePressed;

        Toggled += toggle => {
            if (toggle) {
                _deleteButton.Show();
            } else {
                _deleteButton.Hide();
            }
        };

        MouseEntered += () => {
            if (!IsPressed()) return;
            _deleteButton.Show();
        };
        
        // Delay check to avoid flickering
        MouseExited += () => CallDeferred(nameof(HideDeleteButton));
        _deleteButton.MouseExited += () => CallDeferred(nameof(HideDeleteButton));
    }
    
    private void HideDeleteButton() {
        if (IsHovered() || _deleteButton.IsHovered()) return;
        _deleteButton.Hide();
    }

    private void OnDeletePressed() {
        DeletePressed?.Invoke();
        QueueFree();
    }
    
}