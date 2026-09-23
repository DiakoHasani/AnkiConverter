# AnkiConverter

**Web API برای استخراج و تبدیل فایل‌های Anki (.apkg) به دیتای ساخت‌یافتهٔ فلش‌کارت** — ساخته‌شده با ASP.NET Core 6.

---

## 📖 دربارهٔ پروژه

فایل‌های خروجی [Anki](https://apps.ankiweb.net/) (پسوند `.apkg`) در واقع یک آرشیو ZIP هستند که داخلش یک دیتابیس **SQLite** به همراه فایل‌های رسانه (تصویر/صدا) قرار دارد. **AnkiConverter** یک وب‌سرویس ساده است که این فایل را می‌گیرد، آرشیو را باز می‌کند، جدول `notes` را از دیتابیس داخلی می‌خواند و کارت‌ها (روی/پشت) را به همراه لیست فایل‌های رسانهٔ مرتبط، به‌صورت JSON ساخت‌یافته برمی‌گرداند.

به‌طور خلاصه: یک `.apkg` می‌دهید → لیست کارت‌ها (Front/Back) + فایل‌های رسانهٔ آن‌ها می‌گیرید.

---

## ✨ امکانات

- 📦 باز کردن و پردازش فایل‌های `.apkg` (آرشیو ZIP حاوی دیتابیس Anki)
- 🗄️ خواندن جدول `notes` از دیتابیس SQLite داخل فایل با `Microsoft.Data.Sqlite`
- 🃏 استخراج فیلدهای `flds` (روی کارت) و `sfld` (پشت کارت) و تبدیل به مدل کارت (`ItemCardModel`)
- 🖼️ استخراج و نگاشت فایل‌های رسانه (تصویر/صدا) مرتبط با کارت‌ها
- 📑 مستندسازی API با Swagger / OpenAPI

---

## 🧱 پشته‌ی فناوری (Tech Stack)

| بخش | فناوری |
|---|---|
| فریم‌ورک | ASP.NET Core 6 (Web API) |
| زبان | C# / .NET 6 |
| دیتابیس / پردازش فایل | `Microsoft.EntityFrameworkCore.Sqlite`، `Microsoft.Data.Sqlite` |
| فشرده‌سازی | `System.IO.Compression` |
| مستندسازی API | `Swashbuckle.AspNetCore` (Swagger) |

---

## 🏗️ ساختار پروژه

```
AnkiConverter/
└── AnkiConverter/
    ├── Controllers/
    │   └── AnkiController.cs      # اکشن API: GetDetails/{fileName}
    ├── Services/
    │   └── AnkiService.cs         # منطق اصلی استخراج/پردازش فایل apkg
    ├── Models/
    │   ├── DetailAnkiModel.cs     # خروجی نهایی: Files + Texts + Media
    │   ├── ItemCardModel.cs       # یک کارت: From (روی) / Back (پشت)
    │   ├── FileItemModel.cs       # یک فایل رسانه: Name / Url
    │   └── ResultModel.cs         # پوششِ استاندارد پاسخ API (Result/Message/Code/Data)
    ├── wwwroot/
    │   ├── Files/                 # محل قرارگیری فایل‌های apkg ورودی
    │   └── Items/
    │       ├── FileCards/
    │       ├── Ankies/
    │       └── Medias/            # خروجی فایل‌های رسانهٔ استخراج‌شده
    ├── Program.cs
    └── AnkiConverter.csproj
```

---

## ⚙️ پیش‌نیازها

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

## 🚀 راه‌اندازی و اجرا

```bash
# کلون پروژه
git clone https://github.com/DiakoHasani/AnkiConverter.git
cd AnkiConverter/AnkiConverter

# ریستور پکیج‌ها
dotnet restore

# اجرای پروژه
dotnet run
```

بعد از اجرا، مستندات Swagger در آدرسی مثل `https://localhost:{port}/swagger` در دسترس است.

### نحوهٔ استفاده

1. فایل `.apkg` مورد نظر را داخل مسیر `wwwroot/Files/` قرار دهید.
2. درخواست زیر را ارسال کنید:

```
GET /api/anki/GetDetails/{fileName}
```

که `{fileName}` نام فایل apkg (بدون یا با پسوند، بسته به پیاده‌سازی) است.

3. پاسخ، ساختاری شبیه زیر برمی‌گرداند:

```json
{
  "result": true,
  "message": "",
  "code": 200,
  "data": {
    "texts": [
      { "from": "Question text", "back": "Answer text" }
    ],
    "files": [
      { "name": "media1.jpg", "url": "/Items/Medias/media1.jpg" }
    ],
    "media": "..."
  }
}
```

- **404 Not Found** اگر فایل apkg مورد نظر پیدا نشود.
- **400 Bad Request** در صورت بروز خطای دیگر در پردازش فایل.

---

## 🔍 جزئیات فنی نحوهٔ کار

1. فایل `.apkg` به‌عنوان یک آرشیو ZIP باز می‌شود.
2. دیتابیس SQLite داخل آرشیو (معمولاً `collection.anki2`) به یک فایل موقت استخراج می‌شود.
3. روی این دیتابیس کوئری `SELECT * FROM notes` اجرا می‌شود.
4. از هر رکورد، فیلد `flds` (روی کارت) و `sfld` (پشت کارت) خوانده و به `ItemCardModel` تبدیل می‌شود.
5. فایل‌های رسانهٔ داخل آرشیو (بر اساس entry نگاشت `media`) استخراج و آدرس‌دهی می‌شوند.
6. نتیجهٔ نهایی (کارت‌ها + فایل‌های رسانه) در قالب `DetailAnkiModel` و پوشش `ResultModel` بازگردانده می‌شود.

---

## 🤝 مشارکت

Pull Request ها و Issue های شما برای بهبود پروژه خوش‌آمد هستند.

## 📄 لایسنس

این پروژه در حال حاضر بدون لایسنس مشخص منتشر شده است. در صورت نیاز به استفاده یا توزیع، با نویسندهٔ پروژه هماهنگ کنید.
