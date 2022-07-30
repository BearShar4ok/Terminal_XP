using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Terminal_XP.Classes;
using static System.Net.WebRequestMethods;

namespace Terminal_XP.Windows
{
    /// <summary>
    /// Логика взаимодействия для HuckWindow.xaml
    /// </summary>
    
    public partial class HuckWindow : Window
    {
        private const string Symbols = "~!@#$%^&*()_-=+{}|?/\"\';:<>";

        private int HeightConsole = 35;
        private int CountCharInLine = 50;

        private string[] _words;
        private readonly string _theme;
        private FontFamily _localFontFamily;

        private string _rightWord;
        private int _lives;
        private int _startLives;
        private int _startColumnSpon;
        private int _lastColumnSpon;
        private int _rowSpon;
        private int _columnSpon;
        private int _lineNumber;
        private List<List<Span>> _spans = new List<List<Span>>();
        public State ReternedState { get; private set; } = State.None;
        public HuckWindow(string theme, string rightWord)
        {
            InitializeComponent();

            // Get all words for generate
            _words = LingvoNET.Nouns.GetAll().Select(x => x.Word).Where(x => x.Length == rightWord.Length).ToArray();
            // Choose right word
            _rightWord = rightWord;
            // Get count lives
            _lives = (int)ConfigManager.Config.CountLivesForHacking;
            _startLives = (int)ConfigManager.Config.CountLivesForHacking;
            _theme = theme;

            LoadTheme(_theme);

            // KeepAlive = true;

            KeyDown += KeyPress;

            Initialize();

            if (Addition.IsDebugMod)
            {
                AddTextToConsole(_rightWord);

                var exit = false;
                foreach (var column in _spans)
                {
                    foreach (var row in column)
                    {
                        if (((Run)row.Inlines.FirstInline).Text == rightWord)
                        {
                            SetHighlight(row, true);
                            exit = true;
                            break;
                        }
                    }

                    if (exit)
                        break;
                }
            }
            else
            {
                Topmost = true;
                Cursor = Cursors.None;
                LeftRTB.Cursor = Cursors.None;
                leftP.Cursor = Cursors.None;
            }
        }
        private void RemoveLast(object obj, NavigationEventArgs e)
        {
            Addition.NavigationService?.RemoveBackEntry();
        }

        // Method to reload page
        public void Reload()
        {
            LoadTheme(_theme);
        }

        private void LoadTheme(string theme)
        {
            Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(Addition.Themes + theme + "/Background.png", UriKind.Relative)) };
            _localFontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + _theme + "/#" + ConfigManager.Config.FontName);

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
        }

        // Method to generate string with words
        private string GenerateRandomString(int length)
        {
            var random = new Random();

            // Init position right word
            var pos = random.Next(length - _rightWord.Length - 1);

            // Result string
            var result = "";
            var lstWord = false;
            var currSymb = "";

            // Get list indexes for words
            var inds = Enumerable.Range(0, _words.Length).ToList();
            // Remove index right word
            inds.Remove(Array.IndexOf(_words, _rightWord));

            for (var i = 0; i < length; i++)
            {
                // Check pos right word and set that
                if (result.Length >= pos && pos != -1)
                {
                    if (!Symbols.Contains(result[result.Length - 1]))
                        result = result.Remove(result.Length - currSymb.Length - 1);

                    result += _rightWord;
                    pos = -1;
                    lstWord = true;
                }

                // Add word if last not word :) or and symbol
                if (!lstWord && random.Next(0, (int)ConfigManager.Config.RatioSpawnWords) == 0 && inds.Count > 0)
                {
                    var ind = inds[random.Next(inds.Count)];
                    currSymb += _words[ind];
                    inds.Remove(ind);
                    lstWord = true;
                }
                else
                {
                    currSymb = Symbols[random.Next(Symbols.Length)].ToString();
                    lstWord = false;
                }

                // Check length result string 
                if ((result + currSymb).Length > length) continue;
                // Check about position right word
                if ((result + currSymb).Length > pos && pos != -1) continue;

                result += currSymb;
            }

            return result;
        }

        // Method to split generated string to list spans
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
                        Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor),
                        FontFamily = _localFontFamily
                    });
                }

            }
        }

        // Find first index column where contains words
        private int FindStartColumn()
        {
            for (var i = 0; i < _spans.Count; i++)
            {
                if (_spans[i].Count > 0)
                    return i;
            }

            return -1;
        }

        // Find next index column where contains words
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

        // Find prev index column where contains words
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

        // Find last index column where contains words
        private int FindLastColumn()
        {
            for (var i = _spans.Count - 1; i >= 0; i--)
            {
                if (_spans[i].Count > 0)
                    return i;
            }

            return -1;
        }

        // Highlight current span
        private static void SetHighlight(Span span, bool isDebugMod = false)
        {
            var run = (Run)span.Inlines.FirstInline;

            if (isDebugMod)
                span.Background = new SolidColorBrush(Colors.Red);
            else
                span.Background = new SolidColorBrush(Colors.DarkGreen);
            run.Foreground = new SolidColorBrush(Colors.Azure);
        }

        // Method for initializing
        private void Initialize()
        {
            var test = GetTextBlock("@");
            // Get size char of @
            var size = MeasureString(test.Text, test.FontFamily, test.FontStyle, test.FontWeight, test.FontStretch, test.FontSize);

            HeightConsole = (int)Math.Ceiling(size.Height);

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

            CountCharInLine = (int)(LeftRTB.ActualWidth / size.Width);

            _spans.Add(oneSpan);
            _startColumnSpon = FindStartColumn();
            _lastColumnSpon = FindLastColumn();
            _columnSpon = _startColumnSpon;

            // Highlight first word
            SetHighlight(_spans[_columnSpon][_rowSpon]);
        }

        private void KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    ReternedState = State.Cancel;
                    Close();
                    break;
                case Key.D:
                case Key.Right:
                    HighlightWord(Direction.Right);
                    break;
                case Key.A:
                case Key.Left:
                    HighlightWord(Direction.Left);
                    break;
                case Key.S:
                case Key.Down:
                    HighlightWord(Direction.Down);
                    break;
                case Key.W:
                case Key.Up:
                    HighlightWord(Direction.Up);
                    break;
                case Key.Tab:
                    HighlightWord(Direction.JustNext);
                    break;
                case Key.Enter:
                    FillConsole();
                    break;
            }
        }

        // And text of correct/uncorrent to console
        private void FillConsole() => CheckTheWord().Split('\n').ForEach(AddTextToConsole);

        // Get the number of identical characters in strings
        private int HowManyCorrectSymbols(string word)
        {
            var lstw = word.ToList();
            var lstrw = _rightWord.ToList();
            var result = 0;

            foreach (var charw in lstw)
            {
                if (lstrw.Contains(charw))
                {
                    result++;
                    lstrw.Remove(charw);
                }
            }

            return result;
        }

        // Check word to correct
        private string CheckTheWord()
        {
            var text = ((Run)_spans[_columnSpon][_rowSpon].Inlines.FirstInline).Text;

            if (text == _rightWord)
            {
                ReternedState = State.Access;
                Close();
                return ">ACESS";
            }

            _lives--;

            if (_lives > 0 && ConfigManager.Config.DifficultyInfo)
                return ">" + HowManyCorrectSymbols(text) + " из " + _rightWord.Length + " верно!\n>Осталось " + _lives + " из " + _startLives + " попыток!\n>DENIED";

            var alert = new AlertWindow("Уведомление", "Влом провален.", "Закрыть", _theme);

            if (alert.ShowDialog() == false)
            {

                ReternedState = State.Fail;
                Close();
                return ">DENIED";
            }
            return default;
        }

        // Get Textblock
        private TextBlock GetTextBlock(string message) => new TextBlock()
        {
            FontSize = ConfigManager.Config.FontSize,
            Opacity = ConfigManager.Config.Opacity,
            Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor),
            Margin = new Thickness(5, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Text = message,
            FontFamily = _localFontFamily,
            Focusable = false
        };

        // Add string to console
        private void AddTextToConsole(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Output.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(HeightConsole) });

                var border = new Border()
                {
                    CornerRadius = new CornerRadius(5),
                    BorderThickness = new Thickness(1),
                    Child = GetTextBlock(message),
                    Focusable = false
                };

                if (Addition.IsDebugMod)
                    border.BorderBrush = Brushes.Aqua;

                Grid.SetRow(border, _lineNumber);
                Grid.SetRow(border.Child, _lineNumber);

                Output.Children.Add(border);
                _lineNumber++;
            }));
        }

        // Clear background for all spans 
        private void ClearBackgroundSpans()
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

        // Correcting position next span
        private void CorrectSpanPos(bool isItArrow, bool up)
        {
            if (isItArrow)
            {
                if (_columnSpon >= _spans.Count)
                    _columnSpon = _spans.Count - 1;

                if (_columnSpon < 0)
                    _columnSpon = 0;

                if (_spans[_columnSpon].Count == 0)
                    _columnSpon = up ? FindPrevColumn(_columnSpon) : FindNextColumn(_columnSpon);

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
                    _columnSpon = _lastColumnSpon;
                    _rowSpon = _spans[_columnSpon].Count - 1;
                }
            }
        }

        // Highlight next word
        private void HighlightWord(Direction direction)
        {
            ClearBackgroundSpans();
            var isItArrow = true;
            var up = true;

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
                    up = false;
                    break;
                case Direction.JustNext:
                    _rowSpon += 1;
                    isItArrow = false;
                    break;
            }

            CorrectSpanPos(isItArrow, up);

            SetHighlight(_spans[_columnSpon][_rowSpon]);
        }

        // Get size of string
        private static Size MeasureString(string candidate, FontFamily font, FontStyle style, FontWeight weight, FontStretch stretch, double fontsize)
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
