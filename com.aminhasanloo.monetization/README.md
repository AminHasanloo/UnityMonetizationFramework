# فریم‌ورک پرداخت و تبلیغات یونیتی (Amin Hasanloo)

**Open-source** – سازنده: *Amin Hasanloo*  
این پکیج یک لایه‌ی یکپارچه برای پرداخت درون‌برنامه‌ای و تبلیغات در یونیتی است:

- **پرداخت (IAP)**: گوگل‌پلی (Unity IAP)، کافه‌بازار، مایکت، زرین‌پال (REST).
- **تبلیغات (Ads)**: AdMob (Google Mobile Ads)، تپسل (Tapsell / Tapsell Plus)، آیرون‌سورس / Unity LevelPlay.

> همه‌چیز از **Inspector** و از طریق یک **ScriptableObject** تنظیم می‌شود و با **Scripting Define Symbols** در ادیتور، هر شبکه/استور را به سادگی فعال/غیرفعال می‌کنید.

---

## پیش‌نیازها
- Unity 2021.3+ (تست‌شده روی 2021.3/2022.3/2023.x)
- Android Build System: Gradle
- External Dependency Manager (EDM4U) – همراه با Google Mobile Ads نصب می‌شود.

## نصب
### به‌صورت UPM (توصیه‌شده)
1. این ریپو را کلون کنید یا فایل `.tgz`/`.zip` را در پوشه‌ی Packages پروژه‌تان کپی و در `manifest.json` اضافه کنید:
```json
"dependencies": {
  "com.aminhasanloo.monetization": "file:../com.aminhasanloo.monetization"
}
```
2. از منو: `Create > Amin Hasanloo > Monetization Settings` یک تنظیمات بسازید و پر کنید.

### کپی مستقیم پوشه به `Assets`
می‌توانید فولدر `com.aminhasanloo.monetization` را به `Assets/` کپی کنید.

---

## پیکربندی (Monetization Settings)
از منوی `Window > Monetization > Settings` یا از خود آبجکت Settings:

### IAP
- **Store**: یکی یا چند تا (GooglePlay / CafeBazaar / Myket / Zarinpal).
- Google Play: نیازی به کلید عمومی داخل کلاینت نیست؛ Unity IAP استفاده می‌شود. محصولات را در `IAP Catalog` تعریف کنید.
- CafeBazaar: `Public Key` از داشبورد بازار + لیست SKUها.
- Myket: `Public Key` از داشبورد مایکت + لیست SKUها.
- Zarinpal: `merchant_id`، `callback_url` (ترجیحاً به سرور خودتان).

> برای امنیت، تأیید نهایی تراکنش زرین‌پال را **روی سرور** انجام دهید و نتیجه را به کلاینت برگردانید.

### Ads
- **AdMob**: App ID اندروید، Ad Unit IDs (Banner/Interstitial/Rewarded).
- **Tapsell/Plus**: App ID/Key و Zone IDs.
- **ironSource/LevelPlay**: App Key و نام Placementها.

در همان صفحه می‌توانید شبکه‌ها را فعال کنید و Define Symbols به‌صورت خودکار ست می‌شود:
```
AD_ADMOB, AD_TAPSELL, AD_IRONSOURCE, STORE_GOOGLEPLAY, STORE_CAFEBAZAAR, STORE_MYKET, PAY_ZARINPAL
```

---

## استفاده‌ی سریع (کد نمونه)
```csharp
using AminHasanloo.Monetization;

public class Demo : MonoBehaviour
{
    async void Start()
    {
        // راه‌اندازی
        await Monetization.InitializeAsync();

        // تبلیغات
        if (Ads.IsRewardedReady("reward_default"))
            Ads.ShowRewarded("reward_default", onReward: r => Debug.Log($"reward: {r.Amount}"));

        // خرید درون‌برنامه‌ای
        Iap.Purchase("coins_100"); // بر اساس Provider فعال، به استور مربوطه هدایت می‌شود
    }
}
```

### API اصلی
- `Monetization.InitializeAsync()` – راه‌اندازی همه‌ی ماژول‌ها بر اساس تنظیمات.
- `Ads.Load/Show` برای `Banner`, `Interstitial`, `Rewarded` (placement key).
- `Iap.Initialize`, `Iap.Purchase(productId)`, `Iap.Restore()`, رخدادها: `OnPurchaseSucceeded/Failed/Restored`.

---

## وابستگی‌ها (نصب از منابع رسمی)
- **Unity IAP** (`com.unity.purchasing` – حداقل 4.13.0 برای Google Billing v7)
- **Google Mobile Ads Unity** (AdMob) – از گیت‌هاب
- **Tapsell / Tapsell Plus Unity** – از مستندات تپسل
- **ironSource / LevelPlay Unity** – از مستندات رسمی یونیتی/ironSource
- **CafeBazaar Unity IAB** – از گیت‌هاب رسمی بازار
- **Myket Unity IAB** – از KB مایکت
- **Zarinpal REST** – بدون SDK، با REST داخلی (اختیاری با سرور خودتان).

به README پروژه‌های رسمی مراجعه کنید (لینک‌ها داخل ریپوی این پکیج آمده‌اند).

---

## اندروید: Manifest و Gradle
- این پکیج `Plugins/Android/AndroidManifestTemplate.xml` را دارد که اسلات‌های لازم برای:
  - Deep Link زرین‌پال (`myapp://zarinpal`) 
  - مجوزهای بازار/مایکت
  - Activityهای پروکسی هر استور
  فراهم می‌کند. اگر از پلاگین‌های رسمی بازار/مایکت استفاده کنید، ممکن است `EDM4U` آن‌ها را خودکار اضافه کند.

- حتماً از `mainTemplate.gradle` استفاده کنید و **MinSdkVersion** را طبق SDKها تنظیم کنید.

---

## نکات امنیتی
- سرور بک‌اند برای **Verify** زرین‌پال الزامی است (پیشنهادی قوی).
- پارامترهای حساس (merchant_id، کلیدهای RSA) را در سرور ذخیره کنید.
- برای کالاهای مصرفی بعد از Verify موفق، مصرف/تحویل را انجام دهید.

---

## مجوز
MIT – استفاده و توسعه آزاد.

---

## نسبت به SDKها به‌روز بمانید
نسخه‌ی SDKها سریعاً تغییر می‌کند. پیش از انتشار حتماً نسخه‌های آخر را از مراجع رسمی بروزرسانی و در Player Settings > Scripting Define Symbols همسان‌سازی کنید.
