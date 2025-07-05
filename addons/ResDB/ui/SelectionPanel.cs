using System;
using System.Collections.Generic;
using Godot;

namespace Addons.ResDB.UI;

[Tool]
public partial class SelectionPanel : VBoxContainer {

    private ScrollContainer _selectionBox;
    private GridContainer _selectionGrid;
    private LineEdit _nameSearchEdit;
    private LineEdit _typeSearchEdit;
    private Button _reloadButton;
    private Button _compileButton;
    
    private (string name, string path) _currentDatabase;
    private string _nameSearchFilter = "";
    private string _typeSearchFilter = "";
    
    private readonly Dictionary<string, GridContainer> _cachedGrids = [];
    
    public override void _Ready() {
        _selectionBox = GetNode<ScrollContainer>("SelectionBox");
        _nameSearchEdit = GetNode<LineEdit>("TopBar/NameSearch");
        _typeSearchEdit = GetNode<LineEdit>("TopBar/TypeSearch");
        _reloadButton = GetNode<Button>("TopBar/ReloadBtn");
        _compileButton = GetNode<Button>("TopBar/CompileBtn");

        _nameSearchEdit.TextChanged += OnNameSearchEdit;
        _typeSearchEdit.TextChanged += OnTypeSearchEdit;
        _reloadButton.Pressed += OnReloadPressed;
        _compileButton.Pressed += OnCompilePressed;
    }

    private void OnNameSearchEdit(string text) {
        _nameSearchFilter = text;
        ApplySearchFilter();
    }

    private void OnTypeSearchEdit(string text) {
        _typeSearchFilter = text;
        ApplySearchFilter();
    }

    private void ApplySearchFilter() {
        if (!IsInstanceValid(_selectionGrid)) return;
        foreach (var child in _selectionGrid.GetChildren()) {
            if (child is not DatabaseItemButton itemButton) continue;
            
            var nameMatch = string.IsNullOrEmpty(_nameSearchFilter) ||
                            itemButton.Name.Contains(_nameSearchFilter, StringComparison.OrdinalIgnoreCase);
            
            var typeMatch = string.IsNullOrEmpty(_typeSearchFilter) ||
                            itemButton.Type.Contains(_typeSearchFilter, StringComparison.OrdinalIgnoreCase);
            
            itemButton.Visible = nameMatch && typeMatch;
        }
    }

    private void OnReloadPressed() {
        if (_currentDatabase == (null, null)) return;
        var database = _currentDatabase;
        Unload(database.name); // Resets _currentDatabase, thus reason for temp var
        Load(database.name, database.path);
        _nameSearchEdit.Text = string.Empty;
        _typeSearchEdit.Text = string.Empty;
    }

    private void OnCompilePressed() {
        // hehe, make a TypeMap instead, make it static so modders can add their type to the dictionary.
        // probably have an interface for calling the add action
    }

    public void Load(string name, string path) {
        if (_cachedGrids.TryGetValue(name, out var grid)) {
            if (IsInstanceValid(_selectionGrid)) {
                _selectionBox.RemoveChild(_selectionGrid);
            }
            _selectionGrid = grid;
            _selectionBox.AddChild(_selectionGrid);
        } else {
            UpdateItemGrid();
            UpdateGridItems(path);
            _cachedGrids.TryAdd(name, _selectionGrid);
        }
        _currentDatabase = (name, path);
    }

    public void Unload(string name) {
        if (_currentDatabase.name == name) {
            _currentDatabase = (null, null);
        }
        if (_cachedGrids.TryGetValue(name, out var grid)) {
            grid.QueueFree();
        }
        _cachedGrids.Remove(name);
    }
    
    private void UpdateItemGrid() {
        if (IsInstanceValid(_selectionGrid)) {
            _selectionBox.RemoveChild(_selectionGrid);
        }
        var grid = new GridContainer();
        grid.Name = "ItemGrid";
        grid.Columns = 3;
        grid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        grid.SizeFlagsVertical = SizeFlags.ExpandFill;
        _selectionGrid = grid;
        _selectionBox.AddChild(grid);
    }
    
    private void UpdateGridItems(string path) {
        if (!DirAccess.DirExistsAbsolute(path)) {
            GD.PrintErr($"[ResDB] — Directory({path}) does not exist.");
            return;
        }
    
        var dir = DirAccess.Open(path);
        if (dir == null) {
            GD.PrintErr($"[ResDB] — Cannot access Directory({path}).");
            return;
        }
    
        dir.ListDirBegin();
        for (var name = dir.GetNext(); name != ""; name = dir.GetNext()) {
            if (name is "." or "..") continue;
    
            var full = $"{path}/{name}";
            if (dir.CurrentIsDir())
                UpdateGridItems(full);
            else if (name.EndsWith(".tres") || name.EndsWith(".res")) {
                var res = ResourceLoader.Load(full);
                var fileName = full.GetFile();
                BuildItem(res, fileName);
            }
        }
        dir.ListDirEnd();
    }
    
    private void BuildItem(Resource res, string fileName) {
        if (res == null) return;
        var scene = GD.Load<PackedScene>("res://addons/ResDB/ui/DatabaseItemButton.tscn");
        var button = scene.Instantiate<DatabaseItemButton>();
    
        if (_selectionGrid == null) {
            GD.PrintErr("[ResDB] — No ItemGrid found.");
            return;
        }
        _selectionGrid.AddChild(button);
        button.Set(res, fileName);
    }
    
}