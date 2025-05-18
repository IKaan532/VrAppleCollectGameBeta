using Firebase.Firestore;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;
using System.Linq; // LINQ kullanmak i�in ekledik

public class GameTimer : MonoBehaviour
{
    public TMP_Text countdownText;
    private float remainingTime = 0f;  // Ba�lang�� de�eri 0f olarak ayarland�
    private bool timerRunning = false; // Ba�lang�� de�eri false
    private FirebaseFirestore db;
    public HastaVeriYoneticisi hvy;

    async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        // Oyun ayarlar�n� almak i�in FetchGameSettings methodunu �a��r�yoruz
        await FetchGameSettings(hvy.hedefHastaTcKimlikNo);
    }

    async Task FetchGameSettings(string tcKimlikNo)
    {
        var refGunlukKayitlar = db.Collection("hastalar").Document(tcKimlikNo).Collection("gunlukKayitlar");

        try
        {
            // T�m g�nl�k kay�tlar�n� al�yoruz
            QuerySnapshot snapshot = await refGunlukKayitlar.GetSnapshotAsync();

            if (snapshot.Documents.Any()) // E�er herhangi bir belge varsa
            {
                // Son kayd� almak i�in 'OrderByDescending' ile ters s�ral�yoruz
                DocumentSnapshot lastRecord = snapshot.Documents.OrderByDescending(doc => doc.Id).FirstOrDefault();

                if (lastRecord != null)
                {
                    // 'oyunAyarlari' alan�n� al�yoruz ve kontrol ediyoruz
                    if (lastRecord.TryGetValue("oyunAyarlari", out object oyunAyarlariObj))
                    {
                        // E�er oyunAyarlari bir Dictionary<string, object> t�r�nde ise
                        if (oyunAyarlariObj is System.Collections.Generic.Dictionary<string, object> oyunAyarlari)
                        {
                            // 'sure' de�erini kontrol ediyoruz ve do�ru �ekilde al�yoruz
                            if (oyunAyarlari.TryGetValue("sure", out object sureObj))
                            {
                                if (sureObj is long sureLong) // E�er sure long tipindeyse
                                {
                                    remainingTime = (float)sureLong;
                                    //timerRunning = true;
                                    Debug.Log($"Oyun S�resi: {remainingTime}");
                                }
                                else if (sureObj is int sureInt) // E�er sure int tipindeyse
                                {
                                    remainingTime = (float)sureInt;
                                    //timerRunning = true;
                                    Debug.Log($"Oyun S�resi: {remainingTime}");
                                }
                                else
                                {
                                    Debug.LogWarning("Oyun s�resi 'sure' alan� beklenmedik t�rde: " + sureObj.GetType());
                                    remainingTime = 60f; // Varsay�lan 60 saniye
                                    //timerRunning = true;
                                }
                            }
                            else
                            {
                                // E�er 'sure' alan� yoksa varsay�lan olarak 60 saniye kullan�yoruz
                                remainingTime = 60f;
                                //timerRunning = true;
                                Debug.LogWarning("Oyun s�resi 'sure' alan� eksik, varsay�lan 60 saniye kullan�ld�.");
                            }
                        }
                        else
                        {
                            Debug.LogError("'oyunAyarlari' alan� beklenen t�rde de�il. Dictionary<string, object> bekleniyordu.");
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
        catch (System.Exception e)
        {
            Debug.LogError("Oyun ayarlar� al�namad�: " + e.Message);
        }
    }

    void Update()
    {
        if (timerRunning)
        {
            remainingTime -= Time.deltaTime;  // Zaman� azalt�yoruz
            countdownText.text = Mathf.CeilToInt(remainingTime).ToString();  // Kalan zaman� yazd�r�yoruz

            if (remainingTime <= 0)
            {
                timerRunning = false;  // Zaman s�f�r oldu�unda, sayac� durduruyoruz
                ReloadScene();  // Sahneyi yeniden y�kl�yoruz
            }
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Mevcut sahneyi yeniden y�kl�yoruz
    }
}
