namespace Fab.Editor.Core.Services;

public sealed record UndoAction(Action Undo, Action Redo, string Description, string? CoalesceKey = null);

public sealed class UndoService {
	private readonly Stack<UndoAction> _undoStack = new();
	private readonly Stack<UndoAction> _redoStack = new();
	private readonly TimeSpan _coalesceWindow = TimeSpan.FromSeconds(2);
	private DateTime _lastPushUtc;
	private bool _applying;

	public event EventHandler? Changed;
	public bool CanUndo => _undoStack.Count > 0;
	public bool CanRedo => _redoStack.Count > 0;

	public void Push(UndoAction action) {
		if (_applying) return;

		if (action.CoalesceKey is not null &&
			_undoStack.Count > 0 &&
			_undoStack.Peek().CoalesceKey == action.CoalesceKey &&
			DateTime.UtcNow - _lastPushUtc < _coalesceWindow) {
			var prev = _undoStack.Pop();
			var merged = new UndoAction(
				Undo: prev.Undo,
				Redo: action.Redo,
				Description: action.Description,
				CoalesceKey: action.CoalesceKey);
			_undoStack.Push(merged);
		}
		else {
			_undoStack.Push(action);
		}

		_redoStack.Clear();
		_lastPushUtc = DateTime.UtcNow;
		Changed?.Invoke(this, EventArgs.Empty);
	}

	public bool Undo() {
		if (!_undoStack.TryPop(out var action)) return false;
		_applying = true;
		try { action.Undo(); }
		finally { _applying = false; }
		_redoStack.Push(action);
		Changed?.Invoke(this, EventArgs.Empty);
		return true;
	}

	public bool Redo() {
		if (!_redoStack.TryPop(out var action)) return false;
		_applying = true;
		try { action.Redo(); }
		finally { _applying = false; }
		_undoStack.Push(action);
		_lastPushUtc = DateTime.UtcNow;
		Changed?.Invoke(this, EventArgs.Empty);
		return true;
	}

	public void Clear() {
		if (_undoStack.Count == 0 && _redoStack.Count == 0) return;
		_undoStack.Clear();
		_redoStack.Clear();
		Changed?.Invoke(this, EventArgs.Empty);
	}
}
