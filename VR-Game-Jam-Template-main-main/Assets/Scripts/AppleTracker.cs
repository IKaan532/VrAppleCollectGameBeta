using Firebase.Firestore;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic; // Dictionary kullanmak için eklendi
using System.Threading.Tasks; // Task kullanmak için eklendi

public class AppleTracker : MonoBehaviour
{
    private FirebaseFirestore db;

    void Start()
    {
        // Firestore'a baðlantý kuruyoruz
        db = FirebaseFirestore.DefaultInstance;
    }

    // async/await kullanarak kodu daha okunabilir hale getirelim
    public async void RecordApple(string tcNo, bool isBad, bool isCorrect)
    {
        try
        {
            // Ýlk önce hastalar koleksiyonuna ve ardýndan belirli bir tc no'lu belgeye eriþiyoruz
            DocumentReference patientRef = db.Collection("hastalar").Document(tcNo);

            // Ardýndan tc no'lu belgenin altýnda gunlukKayitlar koleksiyonuna eriþiyoruz
            CollectionReference dailyRecordsRef = patientRef.Collection("gunlukKayitlar");

            // Günlük kayýtlardan tüm belgeleri çekiyoruz ve timestamp'e göre en yenisini alýyoruz
            // OrderByDescending ve Limit(1) kullanarak veritabaný seviyesinde filtreleme yapmak daha verimlidir
            QuerySnapshot snapshot = await dailyRecordsRef
                                          .OrderByDescending("timestamp")
                                          .Limit(1) // Sadece en son kaydý al
                                          .GetSnapshotAsync();

            if (snapshot.Documents.Any()) // En az bir belge varsa
            {
                // === DÜZELTÝLEN KISIM BAÞLANGICI ===
                // snapshot.Documents zaten sýralý ve sadece 1 eleman içeriyor (Limit(1) sayesinde)
                // Bu yüzden FirstOrDefault() veya ek sýralama yapmaya gerek yok, doðrudan ilk elemaný alabiliriz.
                DocumentSnapshot latestRecord = snapshot.Documents.First();
                // === DÜZELTÝLEN KISIM SONU ===

                if (latestRecord != null && latestRecord.Exists) // Belgenin var olduðundan emin olalým
                {
                    // En son kaydý aldýk, verileri güncelleme iþlemi yapýyoruz
                    // Verileri güvenli bir þekilde almak için TryGetValue kullanalým
                    latestRecord.TryGetValue<long>("totalBadApples", out long totalBadApples);
                    latestRecord.TryGetValue<long>("totalApples", out long totalApples);
                    latestRecord.TryGetValue<long>("correctBadApples", out long correctBadApples);
                    latestRecord.TryGetValue<long>("correctApples", out long correctApples);

                    // Verileri güncelleme
                    if (isBad)
                    {
                        totalBadApples += 1;
                        if (isCorrect)
                        {
                            correctBadApples += 1;
                        }
                    }
                    else // Good apple
                    {
                        totalApples += 1;
                        if (isCorrect)
                        {
                            correctApples += 1;
                        }
                    }

                    // Güncellenmiþ veriyi Firestore'a kaydetmek için UpdateAsync kullanmak daha iyidir.
                    // SetAsync tüm belgeyi üzerine yazar, UpdateAsync sadece belirtilen alanlarý günceller.
                    // Bu, baþka alanlar varsa onlarýn kaybolmasýný önler.
                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        { "totalBadApples", totalBadApples },
                        { "totalApples", totalApples },
                        { "correctBadApples", correctBadApples },
                        { "correctApples", correctApples },
                        { "timestamp", FieldValue.ServerTimestamp } // Son güncelleme zamanýný da kaydet
                    };
                    await latestRecord.Reference.UpdateAsync(updates);
                    Debug.Log($"Günlük kayýt güncellendi: {latestRecord.Id}");
                }
                else
                {
                    // Limit(1) ile belge geldi ama null veya exist deðilse (beklenmedik durum)
                    Debug.LogWarning("En son kayýt bulundu ancak geçerli deðil. Yeni kayýt oluþturuluyor.");
                    await CreateNewRecord(patientRef, isBad, isCorrect);
                }
            }
            else
            {
                // Eðer hiç kayýt yoksa, ilk kaydý oluþturuyoruz
                Debug.Log("Henüz günlük kayýt yok. Ýlk kayýt oluþturuluyor.");
                await CreateNewRecord(patientRef, isBad, isCorrect);
            }
        }
        catch (Exception e)
        {
            // Hata durumunu loglayalým
            Debug.LogError($"Firestore iþlemi sýrasýnda hata oluþtu: {e.Message}\n{e.StackTrace}");
        }
    }

    // async Task döndürecek þekilde güncelleyelim
    private async Task CreateNewRecord(DocumentReference patientRef, bool isBad, bool isCorrect)
    {
        CollectionReference dailyRecordsRef = patientRef.Collection("gunlukKayitlar");

        // Mantýksal hata düzeltmesi: totalBadApples ve totalApples, isCorrect'ten baðýmsýz olmalý
        long initialTotalBadApples = isBad ? 1 : 0;
        long initialTotalApples = !isBad ? 1 : 0;
        long initialCorrectBadApples = (isBad && isCorrect) ? 1 : 0;
        long initialCorrectApples = (!isBad && isCorrect) ? 1 : 0;

        // Ýlk kaydý oluþturuyoruz
        DocumentReference newRecordRef = await dailyRecordsRef.AddAsync(new Dictionary<string, object>
        {
            { "totalBadApples", initialTotalBadApples },
            { "totalApples", initialTotalApples },
            { "correctBadApples", initialCorrectBadApples },
            { "correctApples", initialCorrectApples },
            { "timestamp", FieldValue.ServerTimestamp }
        });
        Debug.Log($"Yeni günlük kayýt oluþturuldu: {newRecordRef.Id}");
    }
}