using Firebase.Firestore;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic; // Dictionary kullanmak i�in eklendi
using System.Threading.Tasks; // Task kullanmak i�in eklendi

public class AppleTracker : MonoBehaviour
{
    private FirebaseFirestore db;

    void Start()
    {
        // Firestore'a ba�lant� kuruyoruz
        db = FirebaseFirestore.DefaultInstance;
    }

    // async/await kullanarak kodu daha okunabilir hale getirelim
    public async void RecordApple(string tcNo, bool isBad, bool isCorrect)
    {
        try
        {
            // �lk �nce hastalar koleksiyonuna ve ard�ndan belirli bir tc no'lu belgeye eri�iyoruz
            DocumentReference patientRef = db.Collection("hastalar").Document(tcNo);

            // Ard�ndan tc no'lu belgenin alt�nda gunlukKayitlar koleksiyonuna eri�iyoruz
            CollectionReference dailyRecordsRef = patientRef.Collection("gunlukKayitlar");

            // G�nl�k kay�tlardan t�m belgeleri �ekiyoruz ve timestamp'e g�re en yenisini al�yoruz
            // OrderByDescending ve Limit(1) kullanarak veritaban� seviyesinde filtreleme yapmak daha verimlidir
            QuerySnapshot snapshot = await dailyRecordsRef
                                          .OrderByDescending("timestamp")
                                          .Limit(1) // Sadece en son kayd� al
                                          .GetSnapshotAsync();

            if (snapshot.Documents.Any()) // En az bir belge varsa
            {
                // === D�ZELT�LEN KISIM BA�LANGICI ===
                // snapshot.Documents zaten s�ral� ve sadece 1 eleman i�eriyor (Limit(1) sayesinde)
                // Bu y�zden FirstOrDefault() veya ek s�ralama yapmaya gerek yok, do�rudan ilk eleman� alabiliriz.
                DocumentSnapshot latestRecord = snapshot.Documents.First();
                // === D�ZELT�LEN KISIM SONU ===

                if (latestRecord != null && latestRecord.Exists) // Belgenin var oldu�undan emin olal�m
                {
                    // En son kayd� ald�k, verileri g�ncelleme i�lemi yap�yoruz
                    // Verileri g�venli bir �ekilde almak i�in TryGetValue kullanal�m
                    latestRecord.TryGetValue<long>("totalBadApples", out long totalBadApples);
                    latestRecord.TryGetValue<long>("totalApples", out long totalApples);
                    latestRecord.TryGetValue<long>("correctBadApples", out long correctBadApples);
                    latestRecord.TryGetValue<long>("correctApples", out long correctApples);

                    // Verileri g�ncelleme
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

                    // G�ncellenmi� veriyi Firestore'a kaydetmek i�in UpdateAsync kullanmak daha iyidir.
                    // SetAsync t�m belgeyi �zerine yazar, UpdateAsync sadece belirtilen alanlar� g�nceller.
                    // Bu, ba�ka alanlar varsa onlar�n kaybolmas�n� �nler.
                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        { "totalBadApples", totalBadApples },
                        { "totalApples", totalApples },
                        { "correctBadApples", correctBadApples },
                        { "correctApples", correctApples },
                        { "timestamp", FieldValue.ServerTimestamp } // Son g�ncelleme zaman�n� da kaydet
                    };
                    await latestRecord.Reference.UpdateAsync(updates);
                    Debug.Log($"G�nl�k kay�t g�ncellendi: {latestRecord.Id}");
                }
                else
                {
                    // Limit(1) ile belge geldi ama null veya exist de�ilse (beklenmedik durum)
                    Debug.LogWarning("En son kay�t bulundu ancak ge�erli de�il. Yeni kay�t olu�turuluyor.");
                    await CreateNewRecord(patientRef, isBad, isCorrect);
                }
            }
            else
            {
                // E�er hi� kay�t yoksa, ilk kayd� olu�turuyoruz
                Debug.Log("Hen�z g�nl�k kay�t yok. �lk kay�t olu�turuluyor.");
                await CreateNewRecord(patientRef, isBad, isCorrect);
            }
        }
        catch (Exception e)
        {
            // Hata durumunu loglayal�m
            Debug.LogError($"Firestore i�lemi s�ras�nda hata olu�tu: {e.Message}\n{e.StackTrace}");
        }
    }

    // async Task d�nd�recek �ekilde g�ncelleyelim
    private async Task CreateNewRecord(DocumentReference patientRef, bool isBad, bool isCorrect)
    {
        CollectionReference dailyRecordsRef = patientRef.Collection("gunlukKayitlar");

        // Mant�ksal hata d�zeltmesi: totalBadApples ve totalApples, isCorrect'ten ba��ms�z olmal�
        long initialTotalBadApples = isBad ? 1 : 0;
        long initialTotalApples = !isBad ? 1 : 0;
        long initialCorrectBadApples = (isBad && isCorrect) ? 1 : 0;
        long initialCorrectApples = (!isBad && isCorrect) ? 1 : 0;

        // �lk kayd� olu�turuyoruz
        DocumentReference newRecordRef = await dailyRecordsRef.AddAsync(new Dictionary<string, object>
        {
            { "totalBadApples", initialTotalBadApples },
            { "totalApples", initialTotalApples },
            { "correctBadApples", initialCorrectBadApples },
            { "correctApples", initialCorrectApples },
            { "timestamp", FieldValue.ServerTimestamp }
        });
        Debug.Log($"Yeni g�nl�k kay�t olu�turuldu: {newRecordRef.Id}");
    }
}