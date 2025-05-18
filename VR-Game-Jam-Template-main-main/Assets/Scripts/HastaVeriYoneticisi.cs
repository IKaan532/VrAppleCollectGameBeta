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
    public GameObject rottenApplePrefab;   // ��r�k elma
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
                Debug.LogError("TC Kimlik Numaras� bo�!");
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
                Debug.Log("Firebase (Firestore) ba�ar�yla ba�lat�ld�.");
            }
            else
            {
                Debug.LogError("Firebase ba��ml�l�klar� eksik: " + dependencyStatus);
                isFirebaseInitialized = false;
            }
        });
    }

    async Task FetchOyunAyarlari(string tcKimlikNo)
    {
        var refOyunAyar = firestore.Collection("hastalar").Document(tcKimlikNo).Collection("gunlukKayitlar");

        try
        {
            // T�m g�nl�k kay�tlar�n� al�yoruz
            QuerySnapshot snapshot = await refOyunAyar.GetSnapshotAsync();

            if (snapshot.Documents.Any()) // E�er herhangi bir belge varsa
            {
                // Son kayd� almak i�in 'OrderByDescending' ile ters s�ral�yoruz
                DocumentSnapshot lastRecord = snapshot.Documents.OrderByDescending(doc => doc.Id).FirstOrDefault();

                if (lastRecord != null)
                {
                    // 'oyunAyarlari' alan�n� al�yoruz
                    if (lastRecord.TryGetValue("oyunAyarlari", out object oyunAyarlariObj) && oyunAyarlariObj is Dictionary<string, object> oyunAyarlari)
                    {
                        // 'sure' de�erini al�yoruz
                        if (oyunAyarlari.TryGetValue("sure", out object sureObj) && sureObj is long sureValue)
                        {
                            float remainingTime = (float)sureValue;
                            Debug.Log($"Oyun S�resi: {remainingTime}");
                        }
                        else
                        {
                            // E�er 'sure' alan� yoksa varsay�lan olarak 60 saniye kullan�yoruz
                            Debug.LogWarning("Oyun s�resi 'sure' alan� eksik, varsay�lan 60 saniye kullan�ld�.");
                        }
                    }
                    else
                    {
                        Debug.LogError("'oyunAyarlari' alan� bulunamad�.");
                    }
                }
                else
                {
                    Debug.LogError("Sonu� al�namad�.");
                }
            }
            else
            {
                Debug.LogError("G�nl�k kay�tlar� bulunamad�.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Oyun ayarlar� al�namad�: " + e.Message);
        }
    }

    async Task LoadApplePositions(string tcKimlikNo)
    {
        var refGunlukKayitlar = firestore.Collection("hastalar").Document(tcKimlikNo).Collection("gunlukKayitlar");

        try
        {
            // G�nl�k kay�tlar�n� al�yoruz
            QuerySnapshot tarihSnapshot = await refGunlukKayitlar.GetSnapshotAsync();

            if (tarihSnapshot.Documents.Any()) // E�er herhangi bir belge varsa
            {
                // Son kayd� almak i�in 'OrderByDescending' ile ters s�ral�yoruz
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

                                GameObject prefabToUse = applePrefab; // varsay�lan normal elma

                                if (elmaData.TryGetValue("normalElma", out object normalElmaObj) && normalElmaObj is bool isNormal)
                                {
                                    prefabToUse = isNormal ? applePrefab : rottenApplePrefab;
                                }
                                else
                                {
                                    Debug.LogWarning($"{elmaKey} i�in 'normalElma' bilgisi eksik, normal elma kullan�lacak.");
                                }

                                Vector3 pos = new Vector3(x, y, z);
                                Instantiate(prefabToUse, pos, Quaternion.identity);

                                Debug.Log($"{elmaKey} spawn edildi: ({x}, {y}, {z}) - {(prefabToUse == applePrefab ? "Normal" : "��r�k")}");
                            }
                            else
                            {
                                Debug.LogWarning($"{elmaKey} konumu eksik.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"{elmaKey} bulunamad�.");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Hi� g�nl�k kay�t bulunamad�.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Elma konumlar� al�namad�: " + e.Message);
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
                Debug.LogWarning($"'{key}' de�eri float'a �evrilemedi: {value}");
            }
        }
        return 0f;
    }
}
