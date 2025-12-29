using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatbot
{
    public partial class MainForm : Form
    {
        // Google Gemini API key (environment variable'dan alınır, yoksa bu değer kullanılır)
        private const string DEFAULT_API_KEY = "AIzaSyDJ28orLZJi3l4TXQ-u8mGZHvZ1q9NrSeE";
        
        // Google Cloud Project ID (gerekirse kullanılabilir)
        private const string PROJECT_ID = "YOUR_PROJECT_ID";
        
        // Model adı (gemini-2.5-flash güncel ve hızlı model)
        private const string MODEL_NAME = "gemini-2.5-flash";

        private RichTextBox chatArea = null!;
        private TextBox inputBox = null!;
        private Button sendButton = null!;
        private ComboBox modelComboBox = null!;
        private HttpClient httpClient = null!;
        private string apiKey = "";
        private string selectedModel = MODEL_NAME;

        public MainForm()
        {
            InitializeComponent();
            InitializeGemini();
        }

        private void InitializeComponent()
        {
            this.Text = "Google Gemini Chatbot";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(500, 400);

            // Model seçimi
            Label modelLabel = new Label
            {
                Text = "Model:",
                Location = new Point(10, 10),
                Size = new Size(50, 20)
            };

            modelComboBox = new ComboBox
            {
                Location = new Point(65, 8),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            modelComboBox.Items.Add("gemini-2.5-flash (Güncel ve Hızlı)");
            modelComboBox.SelectedIndex = 0;
            modelComboBox.Enabled = false; // Sadece bir model olduğu için devre dışı
            modelComboBox.SelectedIndexChanged += ModelComboBox_SelectedIndexChanged;

            // Chat alanı
            chatArea = new RichTextBox
            {
                ReadOnly = true,
                Location = new Point(10, 40),
                Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Arial", 10),
                BackColor = Color.White
            };
            chatArea.Text = "Google Gemini Chatbot'a hoş geldiniz! Mesajınızı yazın ve Enter'a basın veya Gönder butonuna tıklayın.\n\n";

            // Input panel
            Panel inputPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10)
            };

            // Input kutusu
            inputBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 11),
                Margin = new Padding(0, 0, 10, 0)
            };
            inputBox.KeyDown += InputBox_KeyDown;

            // Gönder butonu
            sendButton = new Button
            {
                Text = "Gönder",
                Dock = DockStyle.Right,
                Width = 100,
                Font = new Font("Arial", 11),
                UseVisualStyleBackColor = true
            };
            sendButton.Click += SendButton_Click;

            inputPanel.Controls.Add(sendButton);
            inputPanel.Controls.Add(inputBox);

            // Ana layout
            this.Controls.Add(modelLabel);
            this.Controls.Add(modelComboBox);
            this.Controls.Add(chatArea);
            this.Controls.Add(inputPanel);

            // Focus input'a
            inputBox.Focus();
        }

        private void ModelComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Sadece gemini-2.5-flash kullanılıyor
            selectedModel = "gemini-2.5-flash";
        }

        private void InitializeGemini()
        {
            // API key'i environment variable'dan al, yoksa default değeri kullan
            apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? DEFAULT_API_KEY;

            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show(
                    "GOOGLE_API_KEY environment variable bulunamadı ve default key de ayarlanmamış!",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            httpClient = new HttpClient();
        }

        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        private void SendButton_Click(object? sender, EventArgs e)
        {
            SendMessage();
        }

        private async void SendMessage()
        {
            string userInput = inputBox.Text.Trim();

            if (string.IsNullOrEmpty(userInput))
            {
                return;
            }

            // Kullanıcı mesajını ekrana ekle
            chatArea.SelectionStart = chatArea.TextLength;
            chatArea.SelectionLength = 0;
            chatArea.SelectionColor = Color.Blue;
            chatArea.SelectionFont = new Font("Arial", 10, FontStyle.Bold);
            chatArea.AppendText($"You: {userInput}\n\n");

            // Input alanını temizle
            inputBox.Clear();

            // Gönder butonunu devre dışı bırak
            sendButton.Enabled = false;

            try
            {
                // Gemini API isteği için JSON oluştur
                var requestData = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = userInput }
                            }
                        }
                    }
                };

                string jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Google Gemini API'ye istek gönder
                // Önce v1beta endpoint'ini deneyelim (Google AI Studio için)
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{selectedModel}:generateContent?key={apiKey}";
                
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                // Eğer 404 alırsak, v1 endpoint'ini dene
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    apiUrl = $"https://generativelanguage.googleapis.com/v1/models/{selectedModel}:generateContent?key={apiKey}";
                    response = await httpClient.PostAsync(apiUrl, content);
                }

                // Hata durumunda response body'yi oku
                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"HTTP {response.StatusCode}: {errorBody}");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;

                // Gemini API yanıt formatı: candidates[0].content.parts[0].text
                string botResponse = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                // Bot cevabını ekrana ekle
                chatArea.SelectionStart = chatArea.TextLength;
                chatArea.SelectionLength = 0;
                chatArea.SelectionColor = Color.Green;
                chatArea.SelectionFont = new Font("Arial", 10);
                chatArea.AppendText($"Bot: {botResponse}\n\n");
            }
            catch (HttpRequestException ex)
            {
                string errorMsg = ex.Message;
                string errorDetails = "";

                // Hata detaylarını al
                try
                {
                    if (ex.Data.Contains("Response"))
                    {
                        errorDetails = ex.Data["Response"]?.ToString() ?? "";
                    }
                }
                catch { }

                // Quota hatası için özel mesaj
                if (errorMsg.ToLower().Contains("quota") || errorMsg.ToLower().Contains("429"))
                {
                    errorMsg = "API kotası aşıldı. Lütfen Google Cloud hesabınızın planını ve faturalama detaylarını kontrol edin.";
                }
                else if (errorMsg.ToLower().Contains("401") || errorMsg.ToLower().Contains("403"))
                {
                    errorMsg = "API key geçersiz veya yetkisiz. Lütfen GOOGLE_API_KEY'inizi kontrol edin.";
                }
                else if (errorMsg.ToLower().Contains("404"))
                {
                    errorMsg = $"Model '{selectedModel}' bulunamadı. Lütfen farklı bir model seçin. Kullanılan endpoint: v1/models/{selectedModel}:generateContent";
                }

                chatArea.SelectionStart = chatArea.TextLength;
                chatArea.SelectionLength = 0;
                chatArea.SelectionColor = Color.Red;
                chatArea.SelectionFont = new Font("Arial", 10);
                chatArea.AppendText($"Hata: {errorMsg}\n");
                if (!string.IsNullOrEmpty(errorDetails))
                {
                    chatArea.AppendText($"Detay: {errorDetails}\n");
                }
                chatArea.AppendText("\n");
            }
            catch (Exception ex)
            {
                chatArea.SelectionStart = chatArea.TextLength;
                chatArea.SelectionLength = 0;
                chatArea.SelectionColor = Color.Red;
                chatArea.SelectionFont = new Font("Arial", 10);
                chatArea.AppendText($"Hata: {ex.Message}\n\n");
            }

            // Scroll to bottom
            chatArea.SelectionStart = chatArea.TextLength;
            chatArea.ScrollToCaret();

            // Gönder butonunu tekrar aktif et
            sendButton.Enabled = true;
            inputBox.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            httpClient?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
