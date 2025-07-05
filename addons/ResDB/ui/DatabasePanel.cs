using System;
using Godot;
using Godot.Collections;
using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

namespace Addons.ResDB.UI;

[Tool]
public partial class DatabasePanel : VBoxContainer {
    
    public Action<string, string> DatabasePressed;
    public Action<string> DatabaseDeleted;

    private LineEdit _searchEdit;
    private Button _addDatabaseButton;
    private AddDatabasePopup _addDatabasePopup;
    private VBoxContainer _databaseContainer;

    private ButtonGroup _buttonGroup = new();
    
    private Dictionary<string, string> _namePathDict = [];
    
    public override void _Ready() {
        _searchEdit = GetNode<LineEdit>("SearchBar/LineEdit");
        _addDatabaseButton = GetNode<Button>("SearchBar/AddBtn");
        _addDatabasePopup = GetNode<AddDatabasePopup>("%AddDatabasePopup");
        _databaseContainer = GetNode<VBoxContainer>("Databases/List");

        _searchEdit.TextChanged += OnSearchEdit;
        _addDatabaseButton.Pressed += () => { _addDatabasePopup.Popup(); };
        _addDatabasePopup.Confirmed += OnAddDatabase;
        
        LoadConfig();
    }

    private void OnSearchEdit(string text) {
        var children = _databaseContainer.GetChildren();
        if (string.IsNullOrEmpty(text)) {
            foreach (var child in children) {
                if (child is Control { Visible: false } control) {
                    control.Show();
                }
            }
        } else {
            foreach (var child in children) {
                if (child is not Button button) continue;
                if (button.Text.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                    button.Show();
                } else {
                    button.Hide();
                }
            }
        }
    }
    
    private void OnAddDatabase(string name, string path) {
        if (!CollectionExtensions.TryAdd(_namePathDict, name, path)) return;
        BuildButton(name, path);
        SaveConfig();
    }

    private void OnDatabaseDeleted(string name) {
        _namePathDict.Remove(name);
        SaveConfig();
        DatabaseDeleted?.Invoke(name);
    }
    
    private void BuildButton(string name, string path) {
        var scene = GD.Load<PackedScene>("res://addons/ResDB/ui/DatabaseButton.tscn");
        var button = scene.Instantiate<DatabaseButton>();
        button.Text = name;
        button.ButtonGroup = _buttonGroup;
        _databaseContainer.AddChild(button);
        
        button.Pressed += () => DatabasePressed?.Invoke(name, path);
        button.DeletePressed += () => OnDatabaseDeleted(name);
    }
    
    private void UpdateList() {
        foreach (var pair in _namePathDict) {
            BuildButton(pair.Key, pair.Value);
        }
    }
    
    private void SaveConfig() {
        const string dirPath = "res://addons/ResDB/config";
        const string filePath = $"{dirPath}/paths.json";
        
        if (!DirAccess.DirExistsAbsolute(dirPath))
            DirAccess.MakeDirAbsolute(dirPath);
        
        // Save as JSON
        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(Json.Stringify(_namePathDict, indent: "  "));
    }
    
    private void LoadConfig() {
        const string filePath = "res://addons/ResDB/config/paths.json";
        
        if (!FileAccess.FileExists(filePath)) return;
        
        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        var jsonText = file.GetAsText();
        
        var result = Json.ParseString(jsonText);
        if (result.VariantType != Variant.Type.Dictionary) return;
        var dict = (Dictionary<string, string>)result;
        
        _namePathDict = dict;
        UpdateList();
    }
    
}