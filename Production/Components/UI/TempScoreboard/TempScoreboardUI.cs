using Godot;
using System;
using System.Collections.Generic;

public partial class TempScoreboardUI : SubmenuBase
{
    [Export] protected int _maxDisplayedRecords = 10;
    [Export] protected PackedScene _recordUI;
    [Export] protected Control _recordsHolder;
    [Export] protected Label _txtPages;

    private TempScoreboardRecord _placeholderRecord = null;

    List<TempScoreboardRecord> _tempList = new List<TempScoreboardRecord>();
    List<Control> _records = new List<Control>();
 
    int _pages;
    int _currentPage = 0;

    public override void _Ready()
    {
        base._Ready();

        // Setup record UI
        for (int i = 0; i < _maxDisplayedRecords; i++)
        {
            Control record = _recordUI.Instantiate() as Control;
            _recordsHolder.AddChild(record);
            _records.Add(record);
        }
    }


    public override void Open(){
        base.Open();

        // Test score
        //TempScoreboardRecord testRecord1 = new TempScoreboardRecord(1000, "Winners", 4);
        //GameManager.GetInstance().TempScoreboard.SaveRecord(testRecord1);
        //TempScoreboardRecord testRecord2 = new TempScoreboardRecord(3500, "Team2", 2);
        //GameManager.GetInstance().TempScoreboard.SaveRecord(testRecord2);
        //TempScoreboardRecord testRecord3 = new TempScoreboardRecord(2500, "Team3", 3);
        //GameManager.GetInstance().TempScoreboard.SaveRecord(testRecord3);

        // Load 
        TempJsonScoreboard scoreboard = GameManager.GetInstance().TempScoreboard.Scoreboard;
        _pages = scoreboard.Records.Count / _maxDisplayedRecords;
        _currentPage = 0;

        _tempList = new List<TempScoreboardRecord>(scoreboard.Records);

        DisplayPage(_currentPage);
    }

    public void DisplayPageByPosition(int position)
    {
        int page = position / _maxDisplayedRecords;
        _currentPage = page;
        DisplayPage(_currentPage);
    }

    public void SetPlaceholderRecord(int position, float time)
    {
        int teamSize = GameManager.GetInstance().CurrentLevel._sled.DogSpawner.DogList.Count;
        _placeholderRecord = new TempScoreboardRecord(time, "Your time", teamSize);
        
        int placeholderPos = GameManager.GetInstance().TempScoreboard.PositionOfTime(time);
        _tempList.Insert(placeholderPos, _placeholderRecord);
    }

    private void DisplayPage(int page)
    {
        TempJsonScoreboard scoreboard = GameManager.GetInstance().TempScoreboard.Scoreboard;
        int start = page * _maxDisplayedRecords;

        int placeholderPos = -1;
        if (_placeholderRecord != null)
        {
            placeholderPos = GameManager.GetInstance().TempScoreboard.PositionOfTime(_placeholderRecord.Time);
        }

        int count = _maxDisplayedRecords;
        if (placeholderPos != -1)
            count += 1;

        for (int i = 0; i < _maxDisplayedRecords; i++)
        {
            int current = start + i;

            if (current >= _tempList.Count)
            {
                // Hide
                _records[i].Visible = false;
                continue;
            }

            TempScoreboardRecord record = _tempList[current];
            ScoreboardRecordUI ui = _records[i] as ScoreboardRecordUI;

            ui.DisplayHighlighted(current == placeholderPos);

            // Show
            _records[i].Visible = true;

            string left = (current + 1).ToString() + ". " + record.TeamName;
            left += " (Dogs:" + record.TeamSize +")";

            ui.DisplayText(left, LevelManager.TimeFormated(record.Time));
        }

        _txtPages.Text = (_currentPage + 1) + "/" + (_pages + 1);
    }

    public void TurnPage(Label pageLabel, int direction)
    {
        _currentPage = Math.Clamp(_currentPage + direction, 0, _pages);
        DisplayPage(_currentPage);
    }

    public override void _Input(InputEvent @event){
        // if (!Visible)
        //     return;

        // if (@event.IsActionPressed("MenuRight") && _currentPage < _pages)
        // {
        //     _currentPage++;
        //     DisplayPage(_currentPage);
        // }
        
        // if (@event.IsActionPressed("MenuLeft") && _currentPage > 0)
        // {
        //     _currentPage--;
        //     DisplayPage(_currentPage);
        // }
    }
}
