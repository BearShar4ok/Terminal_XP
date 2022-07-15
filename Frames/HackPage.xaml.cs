using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Terminal_XP.Classes;

namespace Terminal_XP.Frames
{
    public enum Direction { Left, Right, Up, Down, JustNext }
    
    public partial class HackPage : Page
    {
        private const string Symbols = "~!@#$%^&*()_-=+{}|?/\"\';:<>";
        
        private string[] _words;
        private string _filename;
        private string _theme;
        private Random _random = new Random();

        private string _rightWord;
        private uint _lives;
        private int _rowSpon;
        private int _columnSpon;
        private List<List<Span>> _spans = new List<List<Span>>();
        
        private int _lineNumber = 0;
        
        
        public HackPage(string filename, string theme)
        {
            InitializeComponent();

            _words = ConfigManager.Config.WordsForHacking;
            _rightWord = _words[_random.Next(_words.Length)];
            _lives = ConfigManager.Config.CountLivesForHacking;
            
            _filename = filename;
            _theme = theme;
            
            KeyDown += KeyPress;
 
            Initialize();
        }

        private void Closing()
        {
            
        }

        private void GoToBack()
        {
            Closing();
            GC.Collect();
            NavigationService?.Navigate(new LoadingPage(Path.GetDirectoryName(_filename), _theme));
        }

        private void GoToFile()
        {
            NavigationService.Navigate(Addition.GetPageByFilename(_filename, _theme));
        }
        
        private string GenerateRandomString(int length)
        {
            var pos = _random.Next(length - _rightWord.Length - 1);
            
            var result = "";
            var lstWord = false;
            var currSymb = "";
            
            var inds = Enumerable.Range(0, _words.Length).ToList();
            inds.Remove(Array.IndexOf(_words, _rightWord));
            
            for (var i = 0; i < length; i++)
            {
                if (result.Length >= pos && pos != -1)
                {
                    result += _rightWord;
                    pos = -1;
                }

                if (lstWord)
                {
                    currSymb = Symbols[_random.Next(Symbols.Length)].ToString();
                    lstWord = false;
                }
                else
                {
                    if (_random.Next(0, (int)ConfigManager.Config.RatioSpawnWords) == 0 && inds.Count > 0)
                    {
                        var ind = inds[_random.Next(inds.Count)];
                        currSymb += _words[ind];
                        
                        lstWord = true;
                    }
                    else
                    {
                        currSymb += Symbols[_random.Next(Symbols.Length)].ToString();
                        
                        lstWord = false;
                    }
                }

                if ((result + currSymb).Length > length) continue;
                if ((result + currSymb).Length > pos && pos != -1) continue;;
                
                result += currSymb;
            }

            return result;
        }

        private void AddToField()
        {
            var str = GenerateRandomString((int)ConfigManager.Config.LengthHackString);
            var word = "";

            for (var i = 0; i < str.Length; i++)
            {
                if (i % 50 == 0)
                    leftP.Inlines.Add(new LineBreak());
                
                if (char.IsLetter(str[i]))
                    word += str[i].ToString();
                else
                {   
                    if (word != "" && (i + 1 == str.Length || char.IsLetter(str[i + 1])))
                    {
                        leftP.Inlines.Add(new Span(new Run(word) { FontSize = 32 }));
                        word = "";
                    }
                    
                    leftP.Inlines.Add(new Run(str[i].ToString()) { FontSize = 32 });
                }

            }
            
            leftRTB.Focus();
        }
        
        private void Initialize()
        {
            AddToField();
            
            var inlines = leftP.Inlines.ToList();
            var oneSpan = new List<Span>();
            
            foreach (var item in inlines)
            {
                switch (item)
                {
                    case LineBreak _:
                        _spans.Add(oneSpan);
                        
                        oneSpan = new List<Span>();
                        break;
                    case Span span:
                        oneSpan.Add(span);
                        break;
                }
            }
            
            _spans.Add(oneSpan);

            _spans[_columnSpon][_rowSpon].Background = new SolidColorBrush(Colors.DarkGreen);
            _spans[_columnSpon][_rowSpon].Focus();
        }
        
        private void KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    GoToBack();
                    break;
                case Key.Right:
                    HighliteWord(Direction.Right);
                    break;
                case Key.Left:
                    HighliteWord(Direction.Left);
                    break;
                case Key.Down:
                    HighliteWord(Direction.Down);
                    break;
                case Key.Up:
                    HighliteWord(Direction.Up);
                    break;
                case Key.Tab:
                    HighliteWord(Direction.JustNext);
                    break;
                case Key.Enter:
                    FillConsole();
                    break;
            }
        }

        private void FillConsole() => AddTextToConsole(CheckTheWord());
        
        private int HowManyCorrectSymbols(string word)
        {
            var set = new HashSet<char>();
            
            foreach (var symbW in word)
            {
                foreach (var symbRW in _rightWord)
                {
                    if (symbRW == symbW)
                        set.Add(symbW);
                }
            }
            
            return set.Count;
        }

        private string CheckTheWord()
        {
            if (_lives == 0) // progressBar.Value
            {
                MessageBox.Show("Файл заблокирован");
                GoToFile();

                return ">DENIED";
            }

            if (((Run) _spans[_columnSpon][_rowSpon].Inlines.FirstInline).Text == _rightWord)
            {
                MessageBox.Show("Файл заблокирован");
                GoToBack();
                
                return ">ACESS";
            }
            else
            {
                _lives--;

                return ">DENIED";
            }
        }

        private void AddTextToConsole(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Output.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(55) });
                
                var text = new TextBlock()
                {
                    FontSize = 14,
                    Margin = new Thickness(5, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Text = message
                };
                
                var border = new Border()
                {
                    CornerRadius = new CornerRadius(5),
                    BorderThickness = new Thickness(1),
                    Child = text
                };

                Grid.SetRow(border, _lineNumber);
                Grid.SetRow(text, _lineNumber);
                
                Output.Children.Add(border);
                _lineNumber++;
            }));
        }
        
        private void ClearBackgoundSpans()
        {
            for (var i = 0; i < _spans.Count; i++)
            {
                for (var j = 0; j < _spans[i].Count; j++)
                {
                    _spans[i][j].Background = new SolidColorBrush(Colors.White);
                }
            }
        }
        
        private void CorrectSpanPos(bool isItArrow)
        {
            if (isItArrow)
            {
                if (_columnSpon >= _spans.Count)
                    _columnSpon = _spans.Count - 1;
                
                if (_columnSpon <= 0)
                    _columnSpon = 1;
                
                if (_rowSpon >= _spans[_columnSpon].Count)
                    _rowSpon = _spans[_columnSpon].Count - 1;
                    
                if (_rowSpon < 0)
                    _rowSpon = 0;
            }
            else
            {
                if (_rowSpon >= _spans[_columnSpon].Count)
                {
                    _columnSpon += 1;
                    _rowSpon = 0;
                }
                
                if (_columnSpon >= _spans.Count)
                {
                    _columnSpon = 1;
                    _rowSpon = 0;
                }
            }
        }

        private void HighliteWord(Direction direction)
        {
            ClearBackgoundSpans();
            var isItArrow = true;

            if (direction == Direction.Left)
                _rowSpon -= 1;
            if (direction == Direction.Right)
                _rowSpon += 1;
            if (direction == Direction.Up)
                _columnSpon -= 1;
            if (direction == Direction.Down)
                _columnSpon += 1;
            if (direction == Direction.JustNext)
            {
                _rowSpon += 1;
                isItArrow = false;
            }

            CorrectSpanPos(isItArrow);

            _spans[_columnSpon][_rowSpon].Background = new SolidColorBrush(Colors.DarkGreen);
            _spans[_columnSpon][_rowSpon].Focus();
        }
    }
}