# Mobil Hedefleme Laboratuvarı (Unity)

Bu proje yalnızca kendi içinde çalışan, bağımsız bir 3D eğitim simülasyonudur. Herhangi bir oyuna bağlanmaz; başka uygulamaların belleğini, ağ trafiğini veya süreçlerini okumaz/değiştirmez.

## İçerik
- 3D eğitim alanı ve hareket eden hedef kuklaları
- 25–200 metre algılama mesafesi
- Açılıp kapatılabilen ESP benzeri ekran etiketleri (yalnızca bu sahnedeki kuklalar)
- 5–180 derece FOV açısı
- FOV ve mesafe içinde kalan en yakın merkez hedefini seçme
- Android APK için Unity Editor build metodu

## Unity ile yerel derleme
Unity Hub üzerinden Unity **2022.3.62f1** ve Android Build Support (SDK/NDK + OpenJDK) kur. Projeyi `UnityProject` klasöründen aç. Ardından **Build > Build Android APK** menüsünü çalıştır.

## GitHub Actions
Otomatik APK derlemesi için Unity Personal lisansını GitHub deposunda `Settings > Secrets and variables > Actions` altına `UNITY_LICENSE` adlı secret olarak eklemek gerekir. Lisans olmadan GitHub Actions Unity Editor'ü etkinleştiremez. Sonra Actions sekmesinden **Build Unity Android APK** workflow'unu çalıştır.
