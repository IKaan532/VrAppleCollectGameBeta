using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Linq;

public class HastaVeriYoneticisi : MonoBehaviour
{
    [Header("Prefab ve Ayarlar")]
    public GameObject applePrefab;         // Normal elma
    public GameObject rottenApplePrefab;   // Çürük elma
    public string hedefHastaTcKimlikNo = "11112345121";

    private FirebaseFirestore firestore;
    private bool isFirebaseInitialized = false;

    async void Start()
    {
        await InitializeFirebase();

        if (isFirebaseInitialized)
        {
            if (!string.IsNullOrEmpty(hedefHastaTcKimlikNo))
            {
                await FetchOyunAyarlari(hedefHastaTcKimlikNo);
                await LoadApplePositions(hedefHastaTcKimlikNo);
            }
            else
            {
                Debug.LogError("TC Kimlik Numarasý boþ!");
            }
        }
    }

    async Task InitializeFirebase()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                firestore = FirebaseFirestore.DefaultInstance;
                isFirebaseInitialized = true;
                Debug.Log("Firebase (Firestore) baþarýyla baþlatýldý.");
            }
            else
            {
                Debug.LogError("Firebase baðýmlýlýklarý eksik: " + dependencyStatus);
                isFirebaseInitialized = false;
            }
        });
    }

    async Task FetchOyunAyarlari(string tcKimlikNo)
    {
        var refOyunAyar = firestore.Collection("hastalar").Document(tcKimlikNo).Collection("gunlukKayitlar");

        try
        {
            // Tüm günlük kayýtlarýný alýyoruz
            QuerySnapshot snapshot = await refOyunAyar.GetSnapshotAsync();

            if (snapshot.Documents.Any()) // Eðer herhangi bir belge varsa
            {
                // Son kaydý almak için 'OrderByDescending' ile ters sýralýyoruz
                DocumentSnapshot lastRecord = snapshot.Documents.OrderByDescending(doc => doc.Id).FirstOrDefault();

                if (lastRecord != null)
                {
                    // 'oyunAyarlari' alanýný alýyoruz
                    if (lastRecord.TryGetValue("oyunAyarlari", out object oyunAyarlariObj) && oyunAyarlariObj is Dictionary<string, object> oyunAyarlari)
                    {
                        // 'sure' deðerini alýyoruz
                        if (oyunAyarlari.TryGetValue("sure", out object sureObj) && sureObj is long sureValue)
                        {
                            float remainingTime = (float)sureValue;
                            Debug.Log($"Oyun Süresi: {remainingTime}");
                        }
                        else
                        {
                            // Eðer 'sure' alaný yoksa varsayýlan olarak 60 saniye kullanýyoruz
                            Debug.LogWarning("Oyun süresi 'sure' alaný eksik, varsayýlan 60 saniye kullanýldý.");
                        }
                    }
                    else
                    {
                        Debug.LogError("'oyunAyarlari' alaný bulunamadý.");
                    }
                }
                else
                {
                    Debug.LogError("Sonuç alýnamadý.");
                }
            }
            else
            {
                Debug.LogError("Günlük kayýtlarý bulunamadý.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Oyun ayarlarý alýnamadý: " + e.Message);
        }
    }

    async Task LoadApplePositions(string tcKimlikNo)
    {
        var refGunlukKayitlar = firestore.Collection("hastalar").Document(tcKimlikNo).Collection("gunlukKayitlar");

        try
        {
            // Günlük kayýtlarýný alýyoruz
            QuerySnapshot tarihSnapshot = await refGunlukKayitlar.GetSnapshotAsync();

            if (tarihSnapshot.Documents.Any()) // Eðer herhangi bir belge varsa
            {
                // Son kaydý almak için 'OrderByDescending' ile ters sýralýyoruz
                DocumentSnapshot lastRecord = tarihSnapshot.Documents.OrderByDescending(doc => doc.Id).FirstOrDefault();

                if (lastRecord != null)
                {
                    Dictionary<string, object> veri = lastRecord.ToDictionary();

                    for (int i = 1; i <= 5; i++)
                    {
                        string elmaKey = $"elma{i}";

                        if (veri.TryGetValue(elmaKey, out object elmaObj) && elmaObj is Dictionary<string, object> elmaData)
                        {
                            if (elmaData.TryGetValue("konum", out object konumObj) && konumObj is Dictionary<string, object> konumData)
                            {
                                float x = SafeToFloat(konumData, "x");
                                float y = SafeToFloat(konumData, "y");
                                float z = SafeToFloat(konumData, "z");

                                GameObject prefabToUse = applePrefab; // varsayýlan normal elma

                                if (elmaData.TryGetValue("normalElma", out object normalElmaObj) && normalElmaObj is bool isNormal)
                                {
                                    prefabToUse = isNormal ? applePrefab : rottenApplePrefab;
                                }
                                else
                                {
                                    Debug.LogWarning($"{elmaKey} için 'normalElma' bilgisi eksik, normal elma kullanýlacak.");
                                }

                                Vector3 pos = new Vector3(x, y, z);
                                Instantiate(prefabToUse, pos, Quaternion.identity);

                                Debug.Log($"{elmaKey} spawn edildi: ({x}, {y}, {z}) - {(prefabToUse == applePrefab ? "Normal" : "Çürük")}");
                            }
                            else
                            {
                                Debug.LogWarning($"{elmaKey} konumu eksik.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"{elmaKey} bulunamadý.");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Hiç günlük kayýt bulunamadý.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Elma konumlarý alýnamadý: " + e.Message);
        }
    }

    float SafeToFloat(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out object value))
        {
            try
            {
                return Convert.ToSingle(Convert.ToDouble(value));
            }
            catch
            {
                Debug.LogWarning($"'{key}' deðeri float'a çevrilemedi: {value}");
            }
        }
        return 0f;
    }
}
