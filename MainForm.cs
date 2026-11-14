using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace NebulaSearch;

public class MainForm : Form
{
    private readonly TextBox txtAddress = new();
    private readonly Button btnGo = new();
    private readonly Button btnBack = new();
    private readonly Button btnForward = new();
    private readonly Button btnReload = new();
    private readonly WebView2 webView = new();

    public MainForm()
    {
        Text = "🌌 NebulaSearch";
        Width = 1200;
        Height = 800;
        BackColor = Color.FromArgb(15, 15, 25);
        StartPosition = FormStartPosition.CenterScreen;

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.FromArgb(25, 25, 40),
            Padding = new Padding(6)
        };

        btnBack.Text = "◀";
        btnBack.Width = 36;
        btnBack.Dock = DockStyle.Left;
        btnBack.FlatStyle = FlatStyle.Flat;
        btnBack.ForeColor = Color.White;
        btnBack.BackColor = Color.FromArgb(40, 40, 60);
        btnBack.Click += (_, _) => { if (webView.CanGoBack) webView.GoBack(); };

        btnForward.Text = "▶";
        btnForward.Width = 36;
        btnForward.Dock = DockStyle.Left;
        btnForward.FlatStyle = FlatStyle.Flat;
        btnForward.ForeColor = Color.White;
        btnForward.BackColor = Color.FromArgb(40, 40, 60);
        btnForward.Click += (_, _) => { if (webView.CanGoForward) webView.GoForward(); };

        btnReload.Text = "⟳";
        btnReload.Width = 36;
        btnReload.Dock = DockStyle.Left;
        btnReload.FlatStyle = FlatStyle.Flat;
        btnReload.ForeColor = Color.White;
        btnReload.BackColor = Color.FromArgb(40, 40, 60);
        btnReload.Click += (_, _) => webView.Reload();

        btnGo.Text = "GO";
        btnGo.Width = 50;
        btnGo.Dock = DockStyle.Right;
        btnGo.FlatStyle = FlatStyle.Flat;
        btnGo.ForeColor = Color.White;
        btnGo.BackColor = Color.FromArgb(120, 80, 255);
        btnGo.Click += (_, _) => Navigate(txtAddress.Text.Trim());

        txtAddress.Dock = DockStyle.Fill;
        txtAddress.BorderStyle = BorderStyle.None;
        txtAddress.BackColor = Color.FromArgb(30, 30, 45);
        txtAddress.ForeColor = Color.White;
        txtAddress.Font = new Font("Segoe UI", 11);
        txtAddress.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Navigate(txtAddress.Text.Trim());
            }
        };

        topPanel.Controls.Add(txtAddress);
        topPanel.Controls.Add(btnGo);
        topPanel.Controls.Add(btnReload);
        topPanel.Controls.Add(btnForward);
        topPanel.Controls.Add(btnBack);
        Controls.Add(topPanel);

        webView.Dock = DockStyle.Fill;
        Controls.Add(webView);
        webView.BringToFront();

        Load += async (_, _) => await InitWebView();
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
            webView.CoreWebView2.Navigate(
                "https://www.google.com/search?q=" + Uri.EscapeDataString(input));
        }
    }
}
