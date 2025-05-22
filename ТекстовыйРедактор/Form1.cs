using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ТекстовыйРедактор
{
    public partial class Form1 : Form
    {
        private string _currentFilePath = null;
        private bool _isModified = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _currentFilePath = openFileDialog.FileName; // Запоминаем путь открытого файла
                richTextBox1.LoadFile(_currentFilePath, RichTextBoxStreamType.PlainText);
            }
            this.Text = $"Мой редактор - {Path.GetFileName(_currentFilePath)}";
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _currentFilePath = saveFileDialog.FileName; // Запоминаем путь
                richTextBox1.SaveFile(_currentFilePath, RichTextBoxStreamType.PlainText);
            }
            this.Text = $"Мой редактор - {Path.GetFileName(_currentFilePath)}";
            _isModified = false;
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                // Если файл уже был сохранен - просто перезаписываем
                richTextBox1.SaveFile(_currentFilePath, RichTextBoxStreamType.PlainText);
            }
            else
            {
                // Если файл новый - вызываем "Сохранить как"
                сохранитьКакToolStripMenuItem_Click(sender, e);
            }
            _isModified = false;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            _isModified = true;
            UpdateStatusBar();
        }

        private void новыйФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!PromptToSave()) return;

            richTextBox1.Clear();
            _currentFilePath = null;
            _isModified = false;
            this.Text = "Мой редактор - Новый файл";
        }

        private bool PromptToSave()
        {
            if (!_isModified) return true; // Если изменений нет, просто продолжаем

            DialogResult result = MessageBox.Show(
                "Сохранить изменения в файле?",
                "Документ изменен",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Пытаемся сохранить
                сохранитьToolStripMenuItem_Click(null, null);
                return true; // Продолжаем операцию
            }
            else if (result == DialogResult.No)
            {
                return true; // Продолжаем без сохранения
            }

            return false; // Отменяем операцию
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PromptToSave())
            {
                e.Cancel = true; // Отменяем закрытие формы
            }
        }

        private void UpdateStatusBar()
        {
            // Считаем строки и символы
            int lineCount = richTextBox1.Lines.Length;
            int charCount = richTextBox1.Text.Length;

            // Получаем текущую позицию курсора
            int currentLine = richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) + 1;
            int currentColumn = richTextBox1.SelectionStart - richTextBox1.GetFirstCharIndexFromLine(currentLine - 1) + 1;

            // Обновляем статус бар
            ((ToolStripStatusLabel)statusStrip1.Items["lblLines"]).Text = $"Строки: {lineCount}";
            ((ToolStripStatusLabel)statusStrip1.Items["lblChars"]).Text = $"Символы: {charCount}";
            ((ToolStripStatusLabel)statusStrip1.Items["lblPosition"]).Text = $"Строка: {currentLine} | Колонка: {currentColumn}";
        }

        private void UpdateCursorPosition()
        {
            int currentLine = richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) + 1;
            int currentColumn = richTextBox1.SelectionStart - richTextBox1.GetFirstCharIndexFromLine(currentLine - 1) + 1;

            ((ToolStripStatusLabel)statusStrip1.Items["lblPosition"]).Text = $"Строка: {currentLine} | Колонка: {currentColumn}";
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateCursorPosition();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateStatusBar();
        }
    }
}
