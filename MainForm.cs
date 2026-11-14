using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Runtime.InteropServices;

namespace NebulaSearch;

public class MainForm : Form
{
    private readonly WebView2 webView = new();
    
    // Кастомные элементы управления
    private readonly Panel topPanel = new();
    private readonly TextBox txtAddress = new();
    private readonly NeonButton btnGo = new();
    private readonly NeonButton btnBack = new();
    private readonly NeonButton btnForward = new();
    private readonly NeonButton btnReload = new();

    // Для перетаскивания окна за верхнюю панель
    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    // Анимация фона
    private readonly System.Windows.Forms.Timer animTimer = new();
    private float starPhase = 0;

    public MainForm()
    {
        // 1. Настройка окна (без рамки, темный фон)
        Text = "🌌 NebulaSearch";
        FormBorderStyle = FormBorderStyle.None;
        Width = 1200;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true; // Чтобы анимация не мерцала
        
        // Скругление углов самого окна (работает в Windows 11)
        try
        {
            var attribute = (DwmWindowAttribute)19;
            int val = 2;
            DwmSetWindowAttribute(Handle, attribute, ref val, sizeof(int));
        }
        catch { }

        // 2. Верхняя панель (для кнопок и перетаскивания)
        topPanel.Dock = DockStyle.Top;
        topPanel.Height = 50;
        topPanel.BackColor = Color.FromArgb(20, 20, 35); // Темно-фиолетовый
        topPanel.MouseDown += TopPanel_MouseDown;

        // Кнопка Закрыть (справа)
        var btnClose = new NeonButton { Text = "✕", Width = 40, Dock = DockStyle.Right, HoverColor = Color.FromArgb(200, 50, 50) };
        btnClose.Click += (s, e) => Application.Exit();

        // Кнопка Свернуть (справа)
        var btnMinimize = new NeonButton { Text = "—", Width = 40, Dock = DockStyle.Right, HoverColor = Color.FromArgb(100, 100, 150) };
        btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;

        // Кнопка GO (справа от адресной строки)
        btnGo.Text = "ПОИСК";
        btnGo.Width = 80;
        btnGo.Dock = DockStyle.Right;
        btnGo.HoverColor = Color.FromArgb(160, 80, 255);
        btnGo.Click += (_, _) => Navigate(txtAddress.Text.Trim());

        // Кнопка Обновить
        btnReload.Text = "⟳";
        btnReload.Width = 40;
        btnReload.Dock = DockStyle.Left;
        btnReload.Click += (_, _) => webView.Reload();

        // Кнопка Вперед
        btnForward.Text = "▶";
        btnForward.Width = 40;
        btnForward.Dock = DockStyle.Left;
        btnForward.Click += (_, _) => { if (webView.CanGoForward) webView.GoForward(); };

        // Кнопка Назад
        btnBack.Text = "◀";
        btnBack.Width = 40;
        btnBack.Dock = DockStyle.Left;
        btnBack.Click += (_, _) => { if (webView.CanGoBack) webView.GoBack(); };

        // Адресная строка (сделаем её плоской и тёмной)
        txtAddress.Dock = DockStyle.Fill;
        txtAddress.BorderStyle = BorderStyle.None;
        txtAddress.BackColor = Color.FromArgb(15, 15, 25);
        txtAddress.ForeColor = Color.FromArgb(200, 200, 255);
        txtAddress.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        // Добавляем отступы внутри TextBox (хакаем через Padding)
        txtAddress.Margin = new Padding(10, 10, 10, 10); 
        txtAddress.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Navigate(txtAddress.Text.Trim()); }
        };

        // Собираем панель (порядок важен при Dock)
        topPanel.Controls.Add(txtAddress);
        topPanel.Controls.Add(btnGo);
        topPanel.Controls.Add(btnReload);
        topPanel.Controls.Add(btnForward);
        topPanel.Controls.Add(btnBack);
        topPanel.Controls.Add(btnMinimize);
        topPanel.Controls.Add(btnClose);
        
        Controls.Add(topPanel);

        // 3. WebView2
        webView.Dock = DockStyle.Fill;
        webView.DefaultBackgroundColor = Color.FromArgb(10, 10, 20);
        Controls.Add(webView);
        webView.BringToFront();

        // 4. Запуск
        Load += async (_, _) => await InitWebView();

        // 5. Запуск анимации
        animTimer.Interval = 50; // 20 кадров в секунду
        animTimer.Tick += (s, e) => { starPhase += 0.05f; Invalidate(); };
        animTimer.Start();
    }

    // Перетаскивание окна за верхнюю панель
    private void TopPanel_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ReleaseCapture();
            SendMessage(Handle, 0xA1, 0x2, 0);
        }
    }

    // Отрисовка космического фона (звезды и туманность)
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // 1. Градиентный фон
        using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            ClientRectangle, Color.FromArgb(10, 5, 20), Color.FromArgb(25, 15, 45), 45f))
        {
            g.FillRectangle(brush, ClientRectangle);
        }

        // 2. Туманность (размытые пятна)
        using (var nebulaBrush = new SolidBrush(Color.FromArgb(40, 100, 50, 200))) // Полупрозрачный фиолетовый
        {
            g.FillEllipse(nebulaBrush, Width / 2 - 200, Height / 2 - 200, 400, 400);
        }
        using (var nebulaBrush2 = new SolidBrush(Color.FromArgb(30, 50, 150, 250))) // Полупрозрачный синий
        {
            g.FillEllipse(nebulaBrush2, Width / 4 - 150, Height / 3 - 150, 300, 300);
        }

        // 3. Звезды (рандомные точки, которые пульсируют)
        var rand = new Random(42); // Фиксированный сид, чтобы звезды не прыгали
        for (int i = 0; i < 200; i++)
        {
            int x = rand.Next(Width);
            int y = rand.Next(Height);
            int size = rand.Next(1, 4);
            int alpha = (int)(Math.Abs(Math.Sin(starPhase + i)) * 155) + 100; // Пульсация
            using var starBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255));
            g.FillEllipse(starBrush, x, y, size, size);
        }
    }

    private async Task InitWebView()
    {
        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NebulaSearch"));

        await webView.EnsureCoreWebView2Async(env);
        webView.CoreWebView2.Navigate("https://www.google.com");
    }

    private void Navigate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        if (input.Contains(".") && !input.Contains(" "))
        {
            if (!input.StartsWith("http")) input = "https://" + input;
            webView.CoreWebView2.Navigate(input);
        }
        else
        {
            webView.CoreWebView2.Navigate("https://www.google.com/search?q=" + Uri.EscapeDataString(input));
        }
    }
}

// ==========================================
// КЛАСС КАСТОМНОЙ НЕОНОВОЙ КНОПКИ
// ==========================================
public class NeonButton : Button
{
    public Color HoverColor { get; set; } = Color.FromArgb(120, 80, 255);
    private bool isHovered = false;

    public NeonButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(200, 200, 255);
        Font = new Font("Segoe UI", 10, FontStyle.Bold);
        Cursor = Cursors.Hand;
        
        MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
        MouseLeave += (s, e) => { isHovered = false; Invalidate(); };
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // Фон кнопки
        var bgColor = isHovered ? HoverColor : Color.FromArgb(30, 30, 50);
        using (var brush = new SolidBrush(bgColor))
        {
            pevent.Graphics.FillRectangle(brush, ClientRectangle);
        }

        // Неоновая рамка при наведении
        if (isHovered)
        {
            using var pen = new Pen(HoverColor, 2);
            pevent.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        // Текст
        TextRenderer.DrawText(pevent.Graphics, Text, Font, ClientRectangle, ForeColor, 
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// Импорт для скругления окна в Windows 11
public enum DwmWindowAttribute : uint
{
    DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
    DWMWA_WINDOW_CORNER_PREFERENCE = 33
}

public static class DWMHelper
{
    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);
}
