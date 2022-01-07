using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

//Ronan Waldron
//G00384180
//Mastermind Project - Internet and Mobile Application Development

namespace MastermindRW
{
    #region Initial
    //Variable Declaration etc.
    public partial class MainPage : ContentPage
    {
        private const int Rounds = 8;
        private SaveGameData save = new SaveGameData();

        private int colorSelect;
        private static readonly string[] colorType = { "blue", "yellow", "red", "cyan", "green", "purple" };
        private readonly List<Image> dashPegs = new List<Image>();
        private readonly List<Grid> scoreBoardPegs = new List<Grid>();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            GameUIInit();
            PegsUIInit();

            NewGame();
        }

        #endregion


        // Display the game pegs based on savegame data if present
        private void RefreshPegs()
        {
            for (int i = 0; i < scoreBoardPegs.Count; i++)
            {
                int index = 0;
                foreach (Image peg in scoreBoardPegs[Rounds - 1 - i].Children)
                {
                    if (index < save.pegs[i, 1])
                    {
                        peg.Source = "black.png";
                    }
                    else if (index < save.pegs[i, 0])
                    {
                        peg.Source = "white.png";
                    }
                    else
                    {
                        peg.Source = "";
                    }
                    index++;
                }//foreach
            }//for
        }//void


        // Enable checkbutton
        private void EnableCheck()
        {
            bool isValid = true;//set

            for (int i = 0; i < 4; i++)
            {
                if (save.board[save.round, i] == -1)
                {
                    isValid = false;
                }
            }

            CheckButton.IsEnabled = isValid;
        }//void

 
        // Loading Savegame

        private void LoadSave()
        {
            for (int i = 0; i < dashPegs.Count; i++)
            {
                int y = int.Parse(dashPegs[i].StyleId);
                int x = int.Parse(dashPegs[i].Parent.StyleId);

                int value = save.board[x, y];

                if (value == -1)
                {
                    dashPegs[i].Source = "white.png";
                }
                else
                {
                    dashPegs[i].Source = colorType[value] + ".png";
                }
            }//for

            EnableCurrentPegRow();
            RefreshPegs();
        }//void


        // Enable display of unplaced pegs on board
        private void EnableCurrentPegRow()
        {
            // Enable next row
            foreach (Image peg in dashPegs)
            {
                if (peg.Parent.StyleId == save.round.ToString())
                {
                    peg.Opacity = 1f;
                    peg.IsEnabled = true;
                }
                else
                {
                    peg.Opacity = 0.5f;
                    peg.IsEnabled = false;
                }
            }//foreach
        }//void

        #region Game States

        // Display when you win
        private async void WinGame()
        {
            await DisplayAlert("WINNER WINNER CHICKEN DINNER!", $"You became the Mastermind on Round {save.round + 1}", "New Game");
            NewGame();
        }


        // Display when youre bad
 
        private async void LoseGame()
        {
            await DisplayAlert("LOSER LOSER WINDOWS PHONE USER!", $"Maybe Try Again?\n", "New Game");
            NewGame();
        }

        #endregion

        // For New Game

        private void NewGame()
        {
            save = new SaveGameData();

            Random chance = new Random();//cr ran

            List<int> duplicates = new List<int>();

            for (int i = 0; i < dashPegs.Count; i++)
            {
                dashPegs[i].Source = "white.png";
            }

            for (int i = 0; i < scoreBoardPegs.Count; i++)
            {
                foreach (Image item in scoreBoardPegs[i].Children)
                {
                    item.Source = "";
                }
            }//for

            EnableCurrentPegRow();

            for (int i = 0; i < 4; i++)
            {
                int num;

                // Make sure its not a duplicate
                do
                {
                    num = chance.Next(0, colorType.Length);
                } while (duplicates.Contains(num));

                duplicates.Add(num);

                save.target[i] = num;
            }//for

        }//void


        #region Click Events


        // Saving Gamedata event

        private async void Save_Clicked(object sender, EventArgs e)
        {
            string appFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string filePath = Path.Combine(appFolder, "Save.json");

            string text = JsonConvert.SerializeObject(save);
            File.WriteAllText(filePath, text);

            await DisplayAlert("Game Saved!", "Game has saved successfully!", "Close");
        }


        // Restarting the game event

        private async void Restart_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Restart?", "You will lose all of your current progress!", "Yes", "No");

            if (answer == false)
            {
                return;
            }

            //c newgame
            NewGame();

            //c displays reload
            LoadSave();
            EnableCheck();
        }


        // Loading game event + alert if save file cannot be found

        private async void Load_Clicked(object sender, EventArgs e)
        {
            string appFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string filePath = Path.Combine(appFolder, "Save.json");

            if (File.Exists(filePath) == false)
            {
                await DisplayAlert("No Save Data!", "Please save a game first in order to load.", "Close");
                return;
            }

            string text = File.ReadAllText(filePath);
            save = JsonConvert.DeserializeObject<SaveGameData>(text);

            LoadSave();
            EnableCheck();

            await DisplayAlert("Save Data Located!", "Game has been loaded", "Play");
        }

        // Tutorial event popup
        private async void Tutorial_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Mastermind - How To Play", "Select a colour from the Peg Bar and click one of the available slots on the board.\nClick Check to see if you made any correct guesses.\nContinue playing through the levels and try to beat the game!.\n\nGood Luck!", "Close");

        }

        #endregion

        #region Tap Events

        // Change Peg colour of the peg on gameboard when clicked

        private void PegPlacement_Tapped(object sender, EventArgs e)
        {
            Image pegImg = (Image)sender;

            pegImg.Source = colorType[colorSelect] + ".png";

            save.board[save.round, int.Parse(pegImg.StyleId)] = colorSelect;

            EnableCheck();//checkbutton
        }


        // Select actual Peg colour to place

        private void PegSelection_Tapped(object sender, EventArgs e)
        {
            Image img = (Image)sender;

            foreach (Image child in PegsContainer.Children)
            {
                child.BackgroundColor = Color.Transparent;
            }

            img.BackgroundColor = new Color(1f, 1f, .25f);

            colorSelect = int.Parse(img.StyleId);
        }

        #endregion


        // Check Pegs are correct 

        private void Checked_Clicked(object sender, EventArgs e)
        {
            int pegWhite = 0;
            int pegBlack = 0;

            // Check number of black/white pegs to give
            for (int i = 0; i < 4; i++)
            {
                bool whitePegMatch = false;
                for (int j = 0; j < 4; j++)
                {
                    if (save.target[i] == save.board[save.round, j])
                    {
                        if (whitePegMatch == false)
                        {
                            pegWhite++;
                            whitePegMatch = true;
                        }

                        if (i == j)
                        {
                            pegBlack++;
                        }
                    }
                }
            }

            save.pegs[save.round, 0] = pegWhite;
            save.pegs[save.round, 1] = pegBlack;

            // The game was won
            if (pegBlack == 4)
            {
                WinGame();
                return;
            }

            RefreshPegs();

            // Move on to next round
            save.round++;

            if (save.round >= Rounds)
            {
                LoseGame();
                return;
            }

            EnableCurrentPegRow();

            EnableCheck();
        }

        

        #region UI Generation


        // Peg UI Initialization

        private void PegsUIInit()
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += PegSelection_Tapped;

            for (int i = 0; i < 6; i++)
            {
                Image peg = new Image();
                peg.StyleId = i.ToString();
                peg.Source = colorType[i] + ".png";

                if (i == 0)
                {
                    peg.BackgroundColor = new Color(1f, 1f, .25f);
                }

                peg.GestureRecognizers.Add(tapGestureRecognizer);
                PegsContainer.Children.Add(peg);
            }//for
        }//void

 
        // Game UI Initialization 

        private void GameUIInit()
        {
            for (int row = 0; row < Rounds; row++)
            {
                // Vertical Rounds list
                StackLayout roundDisplay = new StackLayout();
                roundDisplay.Orientation = StackOrientation.Horizontal;


                // Guess Grid Square
                PegGuessCreate(roundDisplay);

                // Board rows
                GuessPegInit(row, roundDisplay);


                RoundsContainer.Children.Add(roundDisplay);
            }//for
        }//void


        // Frame for selecting Peg Guess

        private void GuessPegInit(int row, StackLayout roundDisplay)
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += PegPlacement_Tapped;

            Frame framePegSelect = new Frame();
            framePegSelect.Margin = new Thickness(4, 0);
            framePegSelect.CornerRadius = 10;
            framePegSelect.BackgroundColor = Color.FromHex("#5086e6");
            framePegSelect.Padding = new Thickness(0);


            StackLayout pegDisplay = new StackLayout();
            pegDisplay.Padding = new Thickness(Rounds, 0);
            pegDisplay.Orientation = StackOrientation.Horizontal;

            pegDisplay.StyleId = (Rounds - 1 - row).ToString();

            for (int j = 0; j < 4; j++)
            {
                Image peg = new Image();
                peg.Source = "white.png";
                if (row != 7)
                {
                    peg.Opacity = .5f;
                    peg.IsEnabled = false;
                }
                peg.GestureRecognizers.Add(tapGestureRecognizer);
                peg.StyleId = j.ToString();

                dashPegs.Add(peg);
                pegDisplay.Children.Add(peg);
            }//for

            framePegSelect.Content = pegDisplay;
            framePegSelect.CornerRadius = 30;
            roundDisplay.Children.Add(framePegSelect);

        }//void


        // Create UI for guess pegs

        private void PegGuessCreate(StackLayout roundDisplay)
        {
            Frame guessFrame = new Frame();
            guessFrame.BackgroundColor = Color.FromHex("#5086e6"); 
            guessFrame.Padding = new Thickness(1);

            Grid grid = new Grid();
            scoreBoardPegs.Add(grid);

            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Image peg = new Image();

                    peg.WidthRequest = 24;
                    peg.HeightRequest = 24;

                    peg.HorizontalOptions = LayoutOptions.Center;
                    peg.VerticalOptions = LayoutOptions.Center;

                    peg.SetValue(Grid.RowProperty, x);
                    peg.SetValue(Grid.ColumnProperty, y);

                    grid.Children.Add(peg);
                }//for2
            }//for1

            guessFrame.Content = grid;
            roundDisplay.Children.Add(guessFrame);
        }//void

        #endregion

        
    }//mainpage
}//namespace
