using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Procon29.Core
{
    public abstract class CellBase : Abstracts.SafeNotifyPropertyChanged, ICell
    {
        int _score;
        public int Score
        {
            get => _score;
            protected set
            {
                if (value == _score) return;
                _score = value;
                RaisePropertyChanged();
            }
        }

        public CellState GetState(Teams team)
        {
            switch (team)
            {
                case Teams.Team1:
                    return State1;
                case Teams.Team2:
                    return State2;
                default:
                    return CellState.None;
            }
        }

        public void SetState(Teams team, CellState state)
        {
            switch (team)
            {
                case Teams.Team1:
                    State1 = state;
                    break;
                case Teams.Team2:
                    State2 = state;
                    break;
            }
        }

        string _text;
        public string Text
        {
            get => _text;
            protected set
            {
                if (value == _text) return;
                _text = value;
                RaisePropertyChanged();
            }
        }

        //セルの状態を扱うプロパティ
        CellState _state1;
        //Team1に関するセルの状態
        public CellState State1
        {
            get => _state1;
            set
            {
                if (value == _state1) return;
                _state1 = value;
                RaisePropertyChanged();
            }
        }
        //Team2に関するセルの状態
        CellState _state2;
        public CellState State2
        {
            get => _state2;
            set
            {
                if (value == _state2) return;
                _state2 = value;
                RaisePropertyChanged();
            }
        }

        //セルの優先順位を扱うプロパティ
        CellPriority _priority;
        public CellPriority Priority
        {
            get => _priority;
            set
            {
                if (value == _priority) return;
                _priority = value;
                RaisePropertyChanged();
            }
        }

        public abstract ICell Clone(bool priority, bool state1, bool state2);
    }
}
