using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MatchGame2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, string> _dictionaryOfIconLocations = new Dictionary<string, string>();

        private string _activePlayer;
        private readonly CardAttributes _cardAttributes = new CardAttributes();

        public MainWindow()
        {
            InitializeComponent();
            SetUpGame();
        }

        private void SetUpGame()
        {
            ResetCardAttibutes(true);

            //hide the final score if it's showing
            var finalScoreLabel = (Label)FindName("FinalScore");
            if (finalScoreLabel != null)
            {
                finalScoreLabel.Visibility = Visibility.Hidden;
            }

            //reset scores
            ((TextBlock)FindName("P1Score")).Text = "0";
            ((TextBlock)FindName("P2Score")).Text = "0";

            //player 1 to start
            _activePlayer = "P1";

            //reset board icons to "?"
            foreach (var textBlock in mainGrid.Children.OfType<TextBlock>())
            {
                textBlock.Text = "?";
                textBlock.Foreground = Brushes.Black;
                textBlock.Background = Brushes.White;
                textBlock.MouseDown += TextBlock_MouseDown;
            }

            var emojiSeed = new List<string>()
            {
                "🦄", "🦓", "🐮", "🦙", "🐫", "🐬", "🦀", "🏀", "🎱", "🎺", "🎻", "🎹", "🥨", "🍩", "🍮", "🍓", "🍒", "🍑"
            };

            //set up grid with randomly placed emoji
            var animalEmoji = GenerateRandomEmojiList(emojiSeed);
            
            var counter = 0;

            var random = new Random();

            _dictionaryOfIconLocations = new Dictionary<string, string>();

            foreach (var _ in mainGrid.Children.OfType<TextBlock>())
            {
                var index = random.Next(animalEmoji.Count);
                var nextEmoji = animalEmoji[index];
                _dictionaryOfIconLocations.Add($"_{counter}", nextEmoji);
                counter++;
                animalEmoji.RemoveAt(index);
            }

            UpdateUi();
        }

        private static List<string> GenerateRandomEmojiList(IList<string> emojiSeed)
        {
            var result = new List<string>();

            var random = new Random();

            for (var i = 1; i <= 8; i++)
            {
                var index = random.Next(emojiSeed.Count);
                var selectedEmoji = emojiSeed[index];
                result.Add(selectedEmoji);
                result.Add(selectedEmoji);
                emojiSeed.RemoveAt(index);
            }

            return result;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                return; //no double clicking allowed
            }

            //prevent click taking action if 2 cards are turned over i.e. we shouldn't see 3 cards at once
            if (_cardAttributes.NumberOfCardsTurnedOver > 1)
            {
                return;
            }

            var textBlock = sender as TextBlock;
            textBlock.Text = _dictionaryOfIconLocations[textBlock.Name];
            textBlock.MouseDown -= TextBlock_MouseDown;

            UpdateUi();
            Thread.Sleep(1000);

            switch (_cardAttributes.NumberOfCardsTurnedOver)
            {
                case 0:
                    _cardAttributes.NumberOfCardsTurnedOver += 1;
                    _cardAttributes.TurnedOverCard = textBlock.Text;
                    _cardAttributes.NameOfTurnedOverCard = textBlock.Name;
                    break;
                case 1:
                    _cardAttributes.NumberOfCardsTurnedOver += 1;

                    if (_cardAttributes.TurnedOverCard == textBlock.Text)
                    {
                        ((TextBlock)FindName(_cardAttributes.NameOfTurnedOverCard)).Foreground = Brushes.White;
                        ((TextBlock)FindName(_cardAttributes.NameOfTurnedOverCard)).Background = Brushes.Black;

                        textBlock.Foreground = Brushes.White;
                        textBlock.Background = Brushes.Black;

                        ResetCardAttibutes();
                        UpdatePlayerScore();
                        UpdateTurnedOverCardTotal();
                    }
                    else
                    {
                        ((TextBlock)FindName($"{_cardAttributes.NameOfTurnedOverCard}")).Text = "?";
                        ((TextBlock)FindName(_cardAttributes.NameOfTurnedOverCard)).MouseDown += TextBlock_MouseDown;

                        textBlock.Text = "?";
                        textBlock.MouseDown += TextBlock_MouseDown;

                        ResetCardAttibutes();
                        SwapActivePlayers();
                    }
                    break;
            }

            if (_cardAttributes.TotalTurnedOverCards != 16)
            {
                return;
            }

            DisplayFinalScore();
            PlayAgain();
        }

        private void PlayAgain()
        {
            const string messageBoxText = "Do you want to continue?";
            const string caption = "Matching Game";

            const MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
            const MessageBoxImage icon = MessageBoxImage.Warning;

            var messageBoxResult = MessageBox.Show(messageBoxText, caption, messageBoxButton, icon);

            switch (messageBoxResult)
            {
                case MessageBoxResult.OK:
                case MessageBoxResult.Yes:
                    SetUpGame();
                    break;
                case MessageBoxResult.None:
                case MessageBoxResult.Cancel:
                case MessageBoxResult.No:
                    Application.Current.Shutdown();
                    break;
                default:
                    Application.Current.Shutdown();
                    break;
            }
        }

        private void UpdateTurnedOverCardTotal() => _cardAttributes.TotalTurnedOverCards += 2;

        private void DisplayFinalScore()
        {
            var finalScoreLabel = (Label)FindName("FinalScore");
            var player1Score = Convert.ToInt32(((TextBlock)FindName("P1Score")).Text);
            var player2Score = Convert.ToInt32(((TextBlock)FindName("P2Score")).Text);

            var result = player1Score > player2Score ? "Player 1 Wins" :
                                player1Score < player2Score ? "Player 2 Wins" :
                                "It's a Draw";

            finalScoreLabel.Content = result;
            finalScoreLabel.Visibility = Visibility.Visible;
            UpdateUi();
        }

        private void UpdatePlayerScore()
        {
            var activePlayerScore = (TextBlock)FindName($"{_activePlayer}Score");
            var score = Convert.ToInt32(activePlayerScore.Text);
            score++;
            activePlayerScore.Text = $"{score}";
        }

        private void SwapActivePlayers()
        {
            ((Label)FindName(_activePlayer)).Foreground = Brushes.Black;

            _activePlayer = _activePlayer == "P1" ? "P2" : "P1";

            ((Label)FindName(_activePlayer)).Foreground = Brushes.Red;
        }

        private static void UpdateUi() =>
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(delegate { }));

        private void ResetCardAttibutes(bool resetAllValues = false)
        {
            if (resetAllValues)
            {
                _cardAttributes.TurnedOverCard = "";
                _cardAttributes.NameOfTurnedOverCard = "";
                _cardAttributes.NumberOfCardsTurnedOver = 0;
                _cardAttributes.TotalTurnedOverCards = 0;

                return;
            }

            _cardAttributes.TurnedOverCard = "";
            _cardAttributes.NameOfTurnedOverCard = "";
            _cardAttributes.NumberOfCardsTurnedOver = 0;
        }

        public class CardAttributes
        {
            public string TurnedOverCard { get; set; } = "";
            public string NameOfTurnedOverCard { get; set; } = "";
            public int NumberOfCardsTurnedOver { get; set; }
            public int TotalTurnedOverCards { get; set; }
        }
    }
}
