using System;
using System.Drawing;
using System.Web;
using System.Windows.Forms;

namespace ТекстовыйРедактор
{
    public partial class FindReplaceForm : Form
    {
        private readonly RichTextBox _richTextBox;
        private int _currentHighlightPos = -1;
        private readonly Timer _searchTimer;

        public FindReplaceForm(RichTextBox richTextBox)
        {
            InitializeComponent();
            _richTextBox = richTextBox;

            // Настройка таймера для поиска при вводе
            _searchTimer = new Timer { Interval = 300 };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                HighlightAllMatches();
            };

            txtFind.TextChanged += (s, e) =>
            {
                _currentHighlightPos = -1;
                _searchTimer.Stop();
                _searchTimer.Start();
            };

            // Подсветка при первом открытии формы
            this.Shown += (s, e) => HighlightAllMatches();
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            FindText();
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            ReplaceText();
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            ReplaceAllText();
        }

        private void FindText(bool searchFromStart = false, bool showNotFoundMessage = true)
        {
            if (string.IsNullOrEmpty(txtFind.Text))
            {
                MessageBox.Show("Введите текст для поиска", "Поиск",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RichTextBoxFinds options = GetSearchOptions();
            int startPos = searchFromStart ? 0 : _richTextBox.SelectionStart + _richTextBox.SelectionLength;
            int foundPos = _richTextBox.Find(txtFind.Text, startPos, options);

            if (foundPos < 0 && !searchFromStart && startPos > 0)
            {
                if (DialogResult.Yes == MessageBox.Show("Достигнут конец документа. Начать поиск сначала?",
                                                     "Поиск",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Question))
                {
                    FindText(true, false);
                }
                return;
            }
            else if (foundPos < 0)
            {
                if (showNotFoundMessage)
                {
                    MessageBox.Show("Текст не найден", "Поиск",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                }
                return;
            }

            // Сброс предыдущего выделения
            if (_currentHighlightPos >= 0)
            {
                _richTextBox.Select(_currentHighlightPos, txtFind.Text.Length);
                _richTextBox.SelectionBackColor = Color.FromArgb(255, 255, 180);
            }

            // Выделение нового совпадения
            _richTextBox.Select(foundPos, txtFind.Text.Length);
            _richTextBox.SelectionBackColor = Color.LimeGreen;
            _richTextBox.ScrollToCaret();
            _currentHighlightPos = foundPos;

            _richTextBox.Focus();
        }

        private void ReplaceText()
        {
            if (_richTextBox.SelectedText.Equals(txtFind.Text,
                chkMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
            {
                _richTextBox.SelectedText = txtReplace.Text;
            }
            FindText();
        }

        private void ReplaceAllText()
        {
            _richTextBox.SuspendLayout();
            int replaceCount = 0;
            int startPos = 0;

            while (startPos < _richTextBox.TextLength)
            {
                int foundPos = _richTextBox.Find(txtFind.Text, startPos, GetSearchOptions());
                if (foundPos < 0) break;

                _richTextBox.Select(foundPos, txtFind.Text.Length);
                _richTextBox.SelectedText = txtReplace.Text;
                startPos = foundPos + txtReplace.Text.Length;
                replaceCount++;
            }

            _richTextBox.ResumeLayout();
            MessageBox.Show($"Заменено: {replaceCount} вхождений", "Замена",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
            HighlightAllMatches();
        }

        private RichTextBoxFinds GetSearchOptions()
        {
            RichTextBoxFinds options = RichTextBoxFinds.None;
            if (chkMatchCase.Checked) options |= RichTextBoxFinds.MatchCase;
            if (chkWholeWord.Checked) options |= RichTextBoxFinds.WholeWord;
            return options;
        }

        private void HighlightAllMatches()
        {
            if (string.IsNullOrEmpty(txtFind.Text)) return;

            _richTextBox.SuspendLayout();
            int savedPosition = _richTextBox.SelectionStart;

            // Сброс всей подсветки
            _richTextBox.SelectAll();
            _richTextBox.SelectionBackColor = _richTextBox.BackColor;

            // Подсветка всех совпадений
            int startPos = 0;
            while (startPos < _richTextBox.TextLength)
            {
                int foundPos = _richTextBox.Find(txtFind.Text, startPos, GetSearchOptions());
                if (foundPos < 0) break;

                _richTextBox.Select(foundPos, txtFind.Text.Length);
                _richTextBox.SelectionBackColor =
                    foundPos == _currentHighlightPos ? Color.LimeGreen : Color.FromArgb(255, 255, 180);

                startPos = foundPos + txtFind.Text.Length;
            }

            // Восстановление позиции
            _richTextBox.Select(savedPosition, 0);
            _richTextBox.SelectionBackColor = _richTextBox.BackColor;
            _richTextBox.ResumeLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Сброс подсветки при закрытии формы
            _richTextBox.SelectAll();
            _richTextBox.SelectionBackColor = _richTextBox.BackColor;
            _richTextBox.Select(0, 0);
        }

        private void txtFind_Enter(object sender, EventArgs e)
        {
            if (txtFind.Text == "Найти...")
            {
                txtFind.Text = "";
            }
        }

        private void txtFind_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFind.Text))
            {
                txtFind.Text = "Найти...";
            }
        }

        private void txtReplace_Enter(object sender, EventArgs e)
        {
            if (txtReplace.Text == "Заменить на...")
            {
                txtReplace.Text = "";
            }
        }

        private void txtReplace_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtReplace.Text))
            {
                txtReplace.Text = "Заменить на...";
            }
        }
    }
}