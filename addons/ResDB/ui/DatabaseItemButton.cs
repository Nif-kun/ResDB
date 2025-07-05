using System;
using Godot;

namespace Addons.ResDB.UI;

[Tool]
public partial class DatabaseItemButton : Button {
    
    private TextureRect _iconRect;
    private Label _nameLabel;
    private Label _typeLabel;

    private string _fileName;
    private Resource _resource;

    public new string Name { get; private set; }
    public string Type { get; private set; }

    private bool _isReady;

    public override void _Ready() {
        _iconRect = GetNode<TextureRect>("Margin/Frame/Icon/TextureRect");
        _nameLabel = GetNode<Label>("Margin/Frame/Details/NameLabel");
        _typeLabel = GetNode<Label>("Margin/Frame/Details/TypeLabel");
        
        _nameLabel.LabelSettings = _nameLabel.LabelSettings.Duplicate() as LabelSettings;
        _typeLabel.LabelSettings = _typeLabel.LabelSettings.Duplicate() as LabelSettings;

        MouseEntered += OnHighlight;
        MouseExited += OnUnhighlight;
        FocusEntered += OnHighlight;
        FocusExited += OnUnhighlight;
        Toggled += OnToggled;

        EditorInterface.Singleton.GetInspector().EditedObjectChanged += OnEditorObjectChanged;
        EditorInterface.Singleton.GetInspector().PropertyEdited += OnEditorPropertyEdited;
        // Note: it disconnects automatically, apparently.
        
        _isReady = true;
    }

    private void OnHighlight() {
        _nameLabel.LabelSettings.FontColor = new Color("#dbdbdb");
        _typeLabel.LabelSettings.FontColor = new Color("#dbdbdb");
    }

    private void OnUnhighlight() {
        if (IsPressed()) return;
        _nameLabel.LabelSettings.FontColor = new Color("#a5a5a5");
        _typeLabel.LabelSettings.FontColor = new Color("#a5a5a5");
    }

    private void OnToggled(bool toggled) {
        if (toggled) {
            OnHighlight();
            if (_resource == null) return;
            EditorInterface.Singleton.InspectObject(_resource);
        } else {
            OnUnhighlight();
        }
    }

    private void OnEditorObjectChanged() {
        var inspectorObject = EditorInterface.Singleton.GetInspector().GetEditedObject();
        if (!ButtonPressed || inspectorObject.Equals(_resource)) return;
        SetPressed(false);
    }

    private void OnEditorPropertyEdited(string property) {
        var inspectorObject = EditorInterface.Singleton.GetInspector().GetEditedObject();
        if (!ButtonPressed || !inspectorObject.Equals(_resource)) return;
        
        var isNameProperty = property.Equals("Name", StringComparison.OrdinalIgnoreCase);
        var isIdProperty = property.Equals("Id", StringComparison.OrdinalIgnoreCase);
        var nameVariant = _resource.Get("Name");
        var idVariant = _resource.Get("Id");
        var nameHasValue = nameVariant.VariantType == Variant.Type.String && nameVariant.ToString().Length > 0;
        var idHasValue = idVariant.VariantType == Variant.Type.String && idVariant.ToString().Length > 0;
        
        if (nameHasValue) {
            if (!isNameProperty) return;
            Name = (string)nameVariant;
            _nameLabel.Text = Name;
            return;
        }
        if (idHasValue) { // Fallback to id if name is empty.
            if (!isNameProperty && !isIdProperty) return;
            Name = (string)idVariant;
            _nameLabel.Text = Name;
            return;
        }
        if (!isNameProperty && !isIdProperty) return; // Fallback to filename if both empty during edit
        Name = _fileName;
        _nameLabel.Text = Name;
    }

    public void Set(Resource res, string fileName) {
        if (!_isReady) return;
        SetResource(res);
        SetResName(res, fileName);
        SetResType(res);
        SetResIcon(res);
    }

    private void SetResource(Resource res) {
        _resource = res;
    }

    private void SetResName(Resource res, string fileName) {
        var nameVariant = res.Get("Name");
        var idVariant = res.Get("Id");
        var nameHasValue = nameVariant.VariantType == Variant.Type.String && nameVariant.ToString().Length > 0;
        var idHasValue = idVariant.VariantType == Variant.Type.String && idVariant.ToString().Length > 0;

        if (nameHasValue) {
            Name = (string)nameVariant;
        } else if (idHasValue) {
            Name = (string)idVariant;
        } else {
            Name = fileName;
        }
        _nameLabel.Text = Name;
        _fileName = fileName;
    }

    private void SetResType(Resource res) {
        var type = res.GetType().Name;
        _typeLabel.Text = $"Type: {type}";
        Type = type;
    }
    
    private void SetResIcon(Resource res) {
        var iconPath = res.Get("IconPath");
        if (iconPath.VariantType != Variant.Type.String) return;
        var path = (string)iconPath;
        _iconRect.Texture = GD.Load<Texture2D>(path);
    }
}