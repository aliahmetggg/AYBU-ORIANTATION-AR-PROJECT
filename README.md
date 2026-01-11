# AYBU Campus AR Orientation

<div align="center">

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black?style=for-the-badge&logo=unity)
![ARCore](https://img.shields.io/badge/ARCore-Supported-green?style=for-the-badge&logo=google)
![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?style=for-the-badge&logo=android)
![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)

**Ankara Yildirim Beyazit Universitesi kampusu icin Artirilmis Gerceklik tabanli oryantasyon uygulamasi**

[Ozellikler](#-ozellikler) • [Kurulum](#-kurulum) • [Kullanim](#-kullanim) • [Mimari](#-mimari) • [Ekran Goruntuleri](#-ekran-goruntuleri)

</div>

---

## Hakkinda

Bu proje, AYBU kampusune yeni katilan ogrencilerin oryantasyon surecini kolaylastirmak icin gelistirilmis bir **Artirilmis Gerceklik (AR)** uygulamasidir. Uygulama, GPS konum dogrulama ve AR gorsel tanima teknolojilerini birlestirerek, ogrencilerin kampus icindeki onemli noktalari kesfetmelerini oyunlastirilmis bir deneyimle saglar.

### Neden Bu Proje?

- Geleneksel oryantasyon yontemleri (basili haritalar, rehberli turlar) yetersiz kaliyor
- Yeni ogrenciler kampuste kaybolma sorunu yasiyor
- Kampuse aidiyet hissi gelistirmek zor oluyor

### Cozum

AR teknolojisi ile interaktif, eglenceli ve akilda kalici bir oryantasyon deneyimi!

---

## Ozellikler

### Temel Ozellikler

| Ozellik | Aciklama |
|---------|----------|
| **AR Gorsel Tanima** | Kampusteki isaretcileri kamera ile tarayarak dijital icerik goruntuler |
| **GPS Dogrulama** | Konum tabanli aktivasyon ile gercek zamanli takip |
| **Cift Dogrulama** | GPS + AR birlikte kullanilarak sahte konum onlenir |
| **Avatar Toplama** | Her lokasyonda farkli avatarlar toplanabilir |
| **Puan Sistemi** | Toplanan avatarlar puan kazandirir |
| **Ilerleme Takibi** | Kesfedilen lokasyonlar kaydedilir |

### Teknik Ozellikler

- **Offline Calisma**: AR tanima internet gerektirmez
- **Dusuk Batarya Tuketimi**: Optimize edilmis GPS kullanimi
- **Hizli Tanima**: <500ms gorsel tanima suresi
- **Kucuk Boyut**: ~85MB APK

---

## Desteklenen Lokasyonlar

| Lokasyon | Koordinatlar | Puan |
|----------|--------------|------|
| Mescit | 39.9712, 32.8182 | 15 |
| Kutuphane | 39.9334, 32.8597 | 10 |
| Yemekhane | 39.9330, 32.8590 | 10 |
| Rektorluk | 39.9345, 32.8610 | 15 |
| Spor Salonu | 39.9325, 32.8580 | 10 |

---

## Kurulum

### Gereksinimler

| Gereksinim | Minimum | Onerilen |
|------------|---------|----------|
| Android Surumu | 7.0 (API 24) | 10.0+ |
| RAM | 2 GB | 4 GB |
| ARCore | Destekli cihaz | - |
| Depolama | 100 MB | 200 MB |

### APK Kurulumu

1. [Releases](../../releases) sayfasindan son surumu indirin
2. APK dosyasini telefonunuza aktarin
3. "Bilinmeyen kaynaklardan yukleme" izni verin
4. APK'yi yukleyin ve calistirin

### Kaynak Koddan Derleme

```bash
# Repoyu klonlayin
git clone https://github.com/[KULLANICI_ADI]/CampusAR2.git

# Unity ile acin (2022.3 LTS)
# File > Build Settings > Android > Build And Run
```

### Unity Gereksinimleri

- Unity 2022.3 LTS
- AR Foundation 5.x
- ARCore XR Plugin
- Android Build Support

---

## Kullanim

### Baslangic

1. Uygulamayi acin
2. Kamera ve konum izinlerini verin
3. Kampusteki belirlenen noktalara gidin
4. Referans gorselleri (tabelalar) tarayin
5. Avatarlari toplayin ve puan kazanin!

### Kontroller

| Eylem | Nasil |
|-------|-------|
| Avatar Toplama | Avatara dokunun |
| Harita Gorme | Harita butonuna basin |
| Ilerleme | HUD'da gorunur |

### Test Modu

Gelistirme icin test modu mevcuttur:

1. Unity'de `LocationVerifier` componentini secin
2. `Test Mode Enabled` kutusunu isaretleyin
3. Ekrandaki butona basarak avatar spawn edin

---

## Mimari

### Sistem Mimarisi

```
┌─────────────────────────────────────────────────────────┐
│                    SUNUM KATMANI                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   AR View   │  │   Harita    │  │    HUD      │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
├─────────────────────────────────────────────────────────┤
│                    IS MANTIGI KATMANI                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │  Location   │  │   Avatar    │  │    Skor     │     │
│  │  Verifier   │  │   Manager   │  │   Manager   │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
├─────────────────────────────────────────────────────────┤
│                    VERI KATMANI                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │    GPS      │  │   ARCore    │  │  PlayerPrefs│     │
│  │   Manager   │  │   Images    │  │   Storage   │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
└─────────────────────────────────────────────────────────┘
```

### Klasor Yapisi

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GPSManager.cs          # GPS konum yonetimi
│   │   ├── LocationVerifier.cs    # GPS + AR dogrulama
│   │   ├── LocationData.cs        # Veri modelleri
│   │   ├── CylinderAvatarGenerator.cs
│   │   ├── StarAvatarGenerator.cs
│   │   └── GPSStarSpawner.cs
│   ├── Game/
│   │   ├── GameManager.cs         # Ana oyun dongusu
│   │   ├── ScoreManager.cs        # Puan sistemi
│   │   └── AvatarManager.cs       # Avatar yonetimi
│   └── UI/
│       ├── UIManager.cs
│       ├── HUDController.cs
│       ├── MapUI.cs
│       └── RuntimeUIBuilder.cs
├── Prefabs/
│   ├── CylinderAvatar.prefab
│   └── CollectableAvatar.prefab
├── Data/
│   ├── AYBULocations.asset        # Kampus lokasyonlari
│   └── MescitDemo.asset
├── Scenes/
│   └── SampleScene.unity
└── Paper/
    └── AYBU_AR_Orientation_Paper.pdf
```

### Temel Siniflar

#### LocationVerifier
GPS ve AR dogrulamasini birlestiren merkezi sinif.

```csharp
// Dogrulama mantigi
verified = GPS_OK && AR_OK  // Cift dogrulama (varsayilan)
verified = GPS_OK || AR_OK  // Tek dogrulama (test icin)
```

#### GPSManager
Konum servislerini yoneten singleton sinif.

```csharp
// Mesafe hesaplama (Haversine formulu)
float distance = GPSManager.Instance.DistanceTo(lat, lon);
bool isNear = GPSManager.Instance.IsWithinRadius(lat, lon, radius);
```

#### CylinderAvatarGenerator
Runtime'da 3D avatar olusturan sinif.

---

## Teknolojiler

<div align="center">

| Teknoloji | Kullanim Alani |
|-----------|----------------|
| ![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity) | Oyun Motoru |
| ![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp) | Programlama |
| ![ARCore](https://img.shields.io/badge/ARCore-4285F4?style=flat-square&logo=google) | AR Framework |
| ![Android](https://img.shields.io/badge/Android-3DDC84?style=flat-square&logo=android) | Platform |

</div>

---

## Ekran Goruntuleri
![projess0](https://github.com/user-attachments/assets/e433cca5-c2e1-430d-98d8-180f299ade25)
![projess1](https://github.com/user-attachments/assets/50cfa48b-57ce-484c-80e5-e76ebcc9a921)
![projess2](https://github.com/user-attachments/assets/3c313471-50e0-4831-9f52-4ac5759948df)
![projess3](https://github.com/user-attachments/assets/2577a6de-b489-4efc-bbcc-43b3b4c7fbc6)


<!--
| Ana Ekran | AR Gorunumu | Harita |
|-----------|-------------|--------|
| ![](screenshots/main.png) | ![](screenshots/ar.png) | ![](screenshots/map.png) |
-->

---

## Gelecek Gelistirmeler

- [ ] iOS destegi (ARKit)
- [ ] Coklu oyuncu modu
- [ ] Beacon entegrasyonu (ic mekan)
- [ ] Sesli yonlendirme
- [ ] Farkli dil destegi
- [ ] Liderlik tablosu
- [ ] Basarim sistemi

---

## Katkida Bulunma

Katkida bulunmak isterseniz:

1. Fork'layin
2. Feature branch olusturun (`git checkout -b feature/YeniOzellik`)
3. Commit'leyin (`git commit -m 'Yeni ozellik eklendi'`)
4. Push'layin (`git push origin feature/YeniOzellik`)
5. Pull Request acin

---

## Lisans

Bu proje MIT lisansi altinda lisanslanmistir. Detaylar icin [LICENSE](LICENSE) dosyasina bakin.

---

## Iletisim

**Gelistirici:** Ali Ahmet Taşkesen

**Kurum:** Ankara Yildirim Beyazit Universitesi

**Ders:** Artirilmis Gerceklik

---

<div align="center">

**AYBU Campus AR** ile kampusu kesfet!

Made with Unity & ARCore

</div>
