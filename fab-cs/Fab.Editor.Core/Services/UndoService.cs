namespace Fab.Editor.Core.Services;

public sealed record UndoAction(Action Undo, string Description, string? CoalesceKey = null);

public sealed class UndoService {
	private readonly Stack<UndoAction> _stack = new();
	private readonly TimeSpan _coalesceWindow = TimeSpan.FromSeconds(2);
	private DateTime _lastPushUtc;
	private bool _applying;

	public event EventHandler? Changed;
	public bool CanUndo => _stack.Count > 0;

	public void Push(UndoAction action) {
		if (_applying) return;

		if (action.CoalesceKey is not null &&
			_stack.Count > 0 &&
			_stack.Peek().CoalesceKey == action.CoalesceKey &&
			DateTime.UtcNow - _lastPushUtc < _coalesceWindow) {
			// Merge: keep the original undo (farther back), ignore new one.
			_lastPushUtc = DateTime.UtcNow;
			return;
		}

		_stack.Push(action);
		_lastPushUtc = DateTime.UtcNow;
		Changed?.Invoke(this, EventArgs.Empty);
	}

	public bool Undo() {
		if (!_stack.TryPop(out var action)) return false;
		_applying = true;
		try {
			action.Undo();
		}
		finally {
			_applying = false;
		}
		Changed?.Invoke(this, EventArgs.Empty);
		return true;
	}

	public void Clear() {
		if (_stack.Count == 0) return;
		_stack.Clear();
		Changed?.Invoke(this, EventArgs.Empty);
	}
}
