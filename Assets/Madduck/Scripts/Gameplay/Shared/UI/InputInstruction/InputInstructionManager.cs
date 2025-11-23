using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Shared
{
    public struct InputInstruction
    {
        public string key;
        public string description;
    }
    
    public class InputInstructionManager
    {
        private readonly int _historyLimit = 10; //NOTE: Change to config later if needed.
        private readonly Dictionary<int, InputInstruction[]> _streams = new();
        private readonly Dictionary<int, LinkedList<InputInstruction[]>> _historyStreams = new();
        private readonly ReactiveProperty<InputInstruction[]> _currentInstructions = new(null);
        public ReadOnlyReactiveProperty<InputInstruction[]> CurrentInstructions => _currentInstructions
            .Select(x => x)
            .ToReadOnlyReactiveProperty();

        public int CurrentStreamIndex { get; private set; }

        [Inject]
        public InputInstructionManager() { }

        public void RemoveStream(int stream)
        {
            _streams.Remove(stream);
            _historyStreams.Remove(stream);
            HandleCurrentInstruction();
        }

        public void Show(InputInstruction[] instructions, int stream)
        {
            _streams.TryAdd(stream, null);
            _historyStreams.TryAdd(stream, new LinkedList<InputInstruction[]>());
            if (_streams[stream] != null)
            {
                if (_historyStreams[stream].Count >= _historyLimit)
                {
                    _historyStreams[stream].RemoveFirst();
                }
                _historyStreams[stream].AddLast(_currentInstructions.Value);
            }
            _streams[stream] = instructions;
            HandleCurrentInstruction();
        }

        public void Revert(int stream)
        {
            if (_historyStreams[stream].Count == 0)
            {
                DebugUtils.LogWarning($"No instruction history to revert, setting instruction to null");
                _currentInstructions.Value = null;
                return;
            }

            var history = _historyStreams[stream].Last.Value;
            _historyStreams[stream].RemoveLast();
            _streams[stream] = history;
            HandleCurrentInstruction();
        }

        private void HandleCurrentInstruction()
        {
            //sort stream form highest to lowest key
            var current = _streams.AsEnumerable().OrderByDescending(x => x.Key).First().Value;
            _currentInstructions.Value = current;
        }
    }
}