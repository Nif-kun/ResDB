using System;
using Godot;

namespace Addons.ResDB.UI;

[Tool]
public partial class AddDatabasePopup : PanelContainer {
    
    public Action<string, string> Confirmed;
    
    private LineEdit _nameEdit;
    private LineEdit _pathEdit;
    private Button _cancelButton;
    private Button _confirmButton;
    private bool _hasName;
    private bool _hasPath;

    public override void _Ready() {
        Hide();
        _nameEdit = GetNode<LineEdit>("Frame/Name/LineEdit");
        _pathEdit = GetNode<LineEdit>("Frame/Path/LineEdit");
        _cancelButton = GetNode<Button>("Frame/Buttons/Cancel");
        _confirmButton = GetNode<Button>("Frame/Buttons/Confirm");
        
        _confirmButton.Disabled = true;
        
        _nameEdit.TextChanged += OnNameChanged;
        _pathEdit.TextChanged += OnPathChanged;
        _cancelButton.Pressed += OnCancel;
        _confirmButton.Pressed += OnConfirm;
    }

    private void OnCancel() {
        Close();
    }

    private void OnConfirm() {
        Confirmed?.Invoke(_nameEdit.Text, _pathEdit.Text);
        Close();
    }

    private void OnNameChanged(string name) {
        _hasName = name.Length > 0;
        VerifyData();
    }

    private void OnPathChanged(string path) {
        _hasPath = path.Length > 0;
        VerifyData();
    }

    private void VerifyData() {
        if (_hasName && _hasPath) {
            _confirmButton.Disabled = false;
        } else {
            _confirmButton.Disabled = true;
        }
    }

    public void Popup() {
        var popupRoot = GetParentOrNull<Control>();
        if (popupRoot != null) {
            popupRoot.MouseFilter = MouseFilterEnum.Pass;
        }
        Show();
    }

    private void Close() {
        _nameEdit.Clear();
        _pathEdit.Clear();
        var popupRoot = GetParentOrNull<Control>();
        if (popupRoot != null) {
            popupRoot.MouseFilter = MouseFilterEnum.Ignore;
        }
        Hide();
    }
    
}