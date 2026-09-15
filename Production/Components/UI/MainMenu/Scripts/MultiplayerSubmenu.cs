using Godot;
using System;

public partial class MultiplayerSubmenu : SubmenuBase
{
    [Export] private LineEdit _lobbyIdEdit; // To insert for joining
    [Export] private Label _lblHostedLobbyId; // Display when hosting

    private NetworkManager _networkManager;

    private string _prevLobbyIdEditText;

    public override void _Ready()
    {
        base._Ready();
        _networkManager = NetworkManager.GetInstance();

        _prevLobbyIdEditText = _lobbyIdEdit.Text;
        _lobbyIdEdit.TextChanged += OnLobbyEditChange;
    }

    private void OnLobbyEditChange(string text)
    {
        if (text.Length == 0)
            return;

        int dif = text.Length - _prevLobbyIdEditText.Length;
        if (dif < 1)
            return;

        for (int i = _prevLobbyIdEditText.Length; i < text.Length; i++)
        {
            string change = text[i].ToString();

            if(!int.TryParse(change, out int value))
            {
                _lobbyIdEdit.Text = _prevLobbyIdEditText;
                _lobbyIdEdit.CaretColumn = text.Length - 1;
                return;
            }
        }

        _prevLobbyIdEditText = _lobbyIdEdit.Text;
    }

    // Buttons
    private void OnJoinPressed()
    {
        _networkManager.JoinLobby(_lobbyIdEdit.Text.ToInt());
    }

    private void OnHostPressed()
    {
        _networkManager.CreateHostedLobby();
        _lblHostedLobbyId.Text = "Hosted lobby ID: " + _networkManager.GetLobbyID().ToString();
    }
}
