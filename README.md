# GJG_Crush - Match-2 Technical Case

Bu proje, istenen Match-2 mekaniklerini yüksek performans ve esnek bir mimariyle sunmak üzere geliştirilmiştir.

## Teknik Özellikler
- **Data-Driven Tasarım:** ScriptableObject yapısı sayesinde kod değiştirmeden yeni seviyeler tasarlanabilir.
- **Object Pooling:** Bloklar için bellek dostu havuzlama sistemi uygulanmıştır.
- **Event-Based Architecture:** Bloklar ve Manager arasındaki bağımlılık (coupling) minimize edilmiştir.
- **Deadlock Check & Shuffle:** Hamle kalmadığında otomatik Fisher-Yates karıştırma algoritması çalışır.

##  Seviye Senaryolarını Test Etme
PDF'te verilen **Example 1** ve **Example 2** senaryolarını test etmek için:
1. Sahnedeki `GridManager` objesini seçin.
2. Inspector panelindeki `Config` kısmına `Assets/ScriptableObjects` klasöründeki `LevelConfig_01` veya `LevelConfig_02` dosyasını sürükleyip bırakın.
3. Oyunu başlatın. Izgara boyutu, renk eşikleri (A, B, C) ve renk sayısı (K) otomatik güncellenecektir.