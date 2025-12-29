# Google Gemini Chatbot (C# Windows Forms)

Google Gemini API kullanarak basit bir Windows Forms chatbot uygulaması.

## Gereksinimler

- .NET 8.0 SDK veya üzeri
- Windows işletim sistemi
- Google Gemini API key

## Kurulum ve Çalıştırma

1. .NET SDK'yı yükleyin: https://dotnet.microsoft.com/download

2. Projeyi derleyin ve çalıştırın:
```bash
dotnet build
dotnet run
```

## API Key Ayarlama

### Yöntem 1: Environment Variable (Önerilen)
```powershell
# Windows PowerShell
$env:GOOGLE_API_KEY="your_api_key_here"
```

```cmd
# Windows CMD
set GOOGLE_API_KEY=your_api_key_here
```

### Yöntem 2: Kod İçinde
`MainForm.cs` dosyasındaki `DEFAULT_API_KEY` sabitini değiştirin.

## Kullanım

1. Programı çalıştırdığınızda bir pencere açılacak
2. Üst kısımdan model seçin (gemini-1.5-flash, gemini-1.5-pro, veya gemini-pro)
3. Alt kısımdaki kutucuğa mesajınızı yazın
4. Enter'a basın veya "Gönder" butonuna tıklayın
5. Bot cevabı chat alanında görünecek

## Özellikler

- ✅ Basit ve kullanıcı dostu Windows Forms arayüzü
- ✅ Google Gemini API entegrasyonu
- ✅ Model seçimi (gemini-1.5-flash, gemini-1.5-pro, gemini-pro)
- ✅ Renkli mesaj gösterimi (You: mavi, Bot: yeşil)
- ✅ Responsive GUI (async/await ile donma yok)
- ✅ Hata yönetimi
- ✅ Enter tuşu ile mesaj gönderme

## Model Seçenekleri

- **gemini-1.5-flash**: Hızlı ve ücretsiz, günlük kullanım için ideal
- **gemini-1.5-pro**: Yüksek kaliteli, daha gelişmiş görevler için
- **gemini-pro**: Klasik model

## Proje Yapısı

- `Program.cs` - Ana giriş noktası
- `MainForm.cs` - Ana form ve chatbot mantığı
- `Chatbot.csproj` - Proje dosyası

## Notlar

- API key environment variable'dan (`GOOGLE_API_KEY`) okunur, yoksa kod içindeki default değer kullanılır
- `PROJECT_ID` şu anda kullanılmıyor, gelecekte gerekirse kullanılabilir
- Ekstra NuGet paketi gerektirmez (System.Net.Http ve System.Text.Json .NET ile birlikte gelir)
- Kod başlangıç seviyesinde ve anlaşılır şekilde yazılmıştır

## API Key Alma

1. Google AI Studio'ya gidin: https://makersuite.google.com/app/apikey
2. Yeni bir API key oluşturun
3. API key'i environment variable olarak ayarlayın veya kod içine yazın
