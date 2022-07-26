using System;
using System.Collections.Generic;
using System.Globalization;
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
    
    // TODO: сделать количество букв в строке адаптивным к размеру экрана (это параметр CountCharInLine)
    // TODO: Мб переделать генератор, чтобы он в текст вставлял радномное число слов, в не с какой-то верноятностью добавлял слова
    // TODO: Чтобы слова не повторялись надо раскомментировать трочку 118
    // TODO: Переделать распеределение слов, т. к. может получиться, что из-за слова предыдущая строка будет идти не до конца(см. метод AddToField)
    // TODO: И ещё мб сделать, чтобы все символы были одного размера
    
    public partial class HackPage : Page
    {
        private const string Symbols = "~!@#$%^&*()_-=+{}|?/\"\';:<>";
        private const bool IsDebugMod = false;
        
        private int HeightConsole = 35;
        private int CountCharInLine = 50;

        private string[] _words;
        private string _filename;
        private string _theme;
        private FontFamily _localFontFamily;
        private Random _random = new Random();

        private string _rightWord;
        private int _lives;
        private int _startColumnSpon;
        private int _lastColumnSpon;
        private int _rowSpon;
        private int _columnSpon;
        private int _lineNumber;
        private List<List<Span>> _spans = new List<List<Span>>();

        public HackPage(string filename, string theme)
        {
            InitializeComponent();

            _words = ConfigManager.Config.WordsForHacking;
            _rightWord = _words[_random.Next(_words.Length)];
            _lives = (int)ConfigManager.Config.CountLivesForHacking;
            
            _filename = filename;
            _theme = theme;

            LoadTheme(_theme);

            Application.Current.MainWindow.KeyDown += KeyPress;

            Initialize();
        }

        public void Reload()
        {
            LoadTheme(_theme);
        }

        private void Closing()
        {
            Application.Current.MainWindow.KeyDown -= KeyPress;
        }

        private void LoadTheme(string theme)
        {
            _localFontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Assets/Themes/Fallout/#Fallout Regular");
        }

        private void GoToBack()
        {
            Closing();
            GC.Collect();
            NavigationService?.Navigate(new LoadingPage(Path.GetDirectoryName(_filename), _theme));
        }

        private void GoToFile()
        {
            Closing();
            GC.Collect();
            NavigationService.Navigate(Addition.GetPageByFilename(_filename, _theme,null));
        }
        
        private string GenerateRandomString(int length)
        {
            var pos = _random.Next(length - _rightWord.Length - 1);
            
            var result = "";
            var lstWord = false;
            var currSymb = "";
            
            var inds = Enumerable.Range(0, _words.Length).ToList();
            inds.Remove(Array.IndexOf(_words, _rightWord));
            
            var random = new Random();
            
            for (var i = 0; i < length; i++)
            {
                if (result.Length >= pos && pos != -1)
                {
                    result += _rightWord;
                    pos = -1;
                }
                
                if (!lstWord && random.Next(0, 5) == 0 && inds.Count > 0)
                {
                    var ind = inds[random.Next(inds.Count)];
                    currSymb += _words[ind];
                    // inds.Remove(ind);
                    lstWord = true;
                }
                else
                {
                    currSymb = Symbols[random.Next(Symbols.Length)].ToString();
                    lstWord = false;
                }

                if ((result + currSymb).Length > length) continue;
                if ((result + currSymb).Length > pos && pos != -1) continue;
                
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
                if (i % CountCharInLine == 0)
                    leftP.Inlines.Add(new LineBreak());
                
                if (char.IsLetter(str[i]))
                    word += str[i].ToString();
                else
                {   
                    if (word != "" && (i + 1 == str.Length || char.IsLetter(str[i + 1])))
                    {
                        leftP.Inlines.Add(new Span(new Run(word)
                        {
                            FontSize = ConfigManager.Config.FontSize,
                            Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor),
                            FontFamily = _localFontFamily
                        }));
                        
                        word = "";
                    }
                    
                    leftP.Inlines.Add(new Run(str[i].ToString())
                    {
                        FontSize = ConfigManager.Config.FontSize,
                        Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor)
                    });
                }

            }
        }

        private int FindStartColumn()
        {
            for (var i = 0; i < _spans.Count; i++)
            {
                if (_spans[i].Count > 0)
                    return i;
            }

            return -1;
        }

        private int FindNextColumn(int column)
        {
            for (var i = column + 1; i < _spans.Count; i++)
            {
                if (_spans[i].Count > 0)
                    return i;
            }
            
            for (var i = 0; i <= column; i++)
            {
                if (_spans[i].Count > 0)
                    return i;
            }

            return -1;
        }
        
        private int FindPrevColumn(int column)
        {
            for (var i = column - 1; i >= 0; i--)
            {
                if (_spans[i].Count > 0)
                    return i;
            }
            
            for (var i = _spans.Count - 1; i >= column; i--)
            {
                if (_spans[i].Count > 0)
                    return i;
            }

            return -1;
        }
        
        private int FindLastColumn()
        {
            for (var i = _spans.Count - 1; i >= 0; i--)
            {
                if (_spans[i].Count > 0)
                    return i;
            }

            return -1;
        }

        private void SetHighlite(Span span)
        {
            var run = (Run)span.Inlines.FirstInline;
            
            span.Background = new SolidColorBrush(Colors.DarkGreen);
            run.Foreground = new SolidColorBrush(Colors.Azure);
        }
        
        private void Initialize()
        {
            var test = GetTextBlock("@");
            var size = MeasureString(test.Text, test.FontFamily, test.FontStyle, test.FontWeight, test.FontStretch, test.FontSize);

            HeightConsole = (int) Math.Ceiling(size.Height);

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

            CountCharInLine = (int)(LeftRTB.ActualWidth / MeasureString("@", LeftRTB.FontFamily, LeftRTB.FontStyle,
                LeftRTB.FontWeight, LeftRTB.FontStretch, LeftRTB.FontSize).Width);

            _spans.Add(oneSpan);
            _startColumnSpon = FindStartColumn();
            _lastColumnSpon = FindLastColumn();
            _columnSpon = _startColumnSpon;

            SetHighlite(_spans[_columnSpon][_rowSpon]);
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

        private void FillConsole() => CheckTheWord().Split('\n').ForEach(x => AddTextToConsole(x));
        
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
            var text = ((Run) _spans[_columnSpon][_rowSpon].Inlines.FirstInline).Text;
            
            if (text == _rightWord)
            {
                GoToFile();
                
                return ">ACESS";
            }
            else
            {
                _lives--;

                if (_lives >= 0)
                    return ">" + HowManyCorrectSymbols(text) + " из " + _rightWord.Distinct().Count() + " верно!\n>DENIED";
                
                MessageBox.Show("Файл заблокирован");
                GoToBack();

                return ">DENIED";
            }
        }
        
        private TextBlock GetTextBlock(string message) => new TextBlock() {
            FontSize = ConfigManager.Config.FontSize,
            Opacity = ConfigManager.Config.Opacity,
            Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor),
            Margin = new Thickness(5, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Text = message,
            FontFamily = _localFontFamily,
            Focusable = false
        };

        private void AddTextToConsole(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Output.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(HeightConsole)});

                var border = new Border()
                {
                    CornerRadius = new CornerRadius(5),
                    BorderThickness = new Thickness(1),
                    Child = GetTextBlock(message),
                    Focusable = false
                };

                if (IsDebugMod)
                    border.BorderBrush = Brushes.Aqua;

                Grid.SetRow(border, _lineNumber);
                Grid.SetRow(border.Child, _lineNumber);
                
                Output.Children.Add(border);
                _lineNumber++;
            }));
        }
        
        private void ClearBackgoundSpans()
        {
            _spans.ForEach(
                spans => spans.ForEach(
                    span =>
                    {
                        var run = (Run)span.Inlines.FirstInline;
                        
                        span.Background = new SolidColorBrush(Colors.Transparent);
                        run.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
                    }
                )
            );
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
                    _columnSpon = FindNextColumn(_columnSpon);
                    _rowSpon = 0;
                }
                
                if (_rowSpon < 0)
                {
                    _columnSpon = FindPrevColumn(_columnSpon);
                    _rowSpon = _spans[_columnSpon].Count - 1;
                }
                
                if (_columnSpon >= _spans.Count)
                {
                    _columnSpon = _startColumnSpon;
                    _rowSpon = 0;
                }

                if (_columnSpon < 0)
                {
                    _columnSpon = FindLastColumn();
                    _rowSpon = _spans[_columnSpon].Count - 1;
                }
            }
        }

        private void HighliteWord(Direction direction)
        {
            ClearBackgoundSpans();
            var isItArrow = true;

            switch (direction)
            {
                case Direction.Left:
                    _rowSpon -= 1;
                    isItArrow = false;
                    break;
                case Direction.Right:
                    _rowSpon += 1;
                    isItArrow = false;
                    break;
                case Direction.Up:
                    _columnSpon -= 1;
                    break;
                case Direction.Down:
                    _columnSpon += 1;
                    break;
                case Direction.JustNext:
                    _rowSpon += 1;
                    isItArrow = false;
                    break;
            }

            CorrectSpanPos(isItArrow);

            SetHighlite(_spans[_columnSpon][_rowSpon]);
        }
        
        private Size MeasureString(string candidate, FontFamily font, FontStyle style, FontWeight weight, FontStretch stretch, double fontsize)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(font, style, weight, stretch),
                fontsize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}