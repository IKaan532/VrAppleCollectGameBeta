using Firebase.Firestore;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;
using System.Linq; // LINQ kullanmak için ekledik

public class GameTimer : MonoBehaviour
{
    public TMP_Text countdownText;
    private float remainingTime = 0f;  // Baþlangýç deðeri 0f olarak ayarlandý
    private bool timerRunning = false; // Baþlangýç deðeri false
    private FirebaseFirestore db;
    public HastaVeriYoneticisi hvy;

    async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        // Oyun ayarlarýný almak için FetchGameSettings methodunu çaðýrýyoruz
        await FetchGameSettings(hvy.hedefHastaTcKimlikNo);
    }

    async Task FetchGameSettings(string tcKimlikNo)
    {
        var refGunlukKayitlar = db.Collection("hastalar").Document(tcKimlikNo).Collection("gunlukKayitlar");

        try
        {
            // Tüm günlük kayýtlarýný alýyoruz
            QuerySnapshot snapshot = await refGunlukKayitlar.GetSnapshotAsync();

            if (snapshot.Documents.Any()) // Eðer herhangi bir belge varsa
            {
                // Son kaydý almak için 'OrderByDescending' ile ters sýralýyoruz
                DocumentSnapshot lastRecord = snapshot.Documents.OrderByDescending(doc => doc.Id).FirstOrDefault();

                if (lastRecord != null)
                {
                    // 'oyunAyarlari' alanýný alýyoruz ve kontrol ediyoruz
                    if (lastRecord.TryGetValue("oyunAyarlari", out object oyunAyarlariObj))
                    {
                        // Eðer oyunAyarlari bir Dictionary<string, object> türünde ise
                        if (oyunAyarlariObj is System.Collections.Generic.Dictionary<string, object> oyunAyarlari)
                        {
                            // 'sure' deðerini kontrol ediyoruz ve doðru þekilde alýyoruz
                            if (oyunAyarlari.TryGetValue("sure", out object sureObj))
                            {
                                if (sureObj is long sureLong) // Eðer sure long tipindeyse
                                {
                                    remainingTime = (float)sureLong;
                                    //timerRunning = true;
                                    Debug.Log($"Oyun Süresi: {remainingTime}");
                                }
                                else if (sureObj is int sureInt) // Eðer sure int tipindeyse
                                {
                                    remainingTime = (float)sureInt;
                                    //timerRunning = true;
                                    Debug.Log($"Oyun Süresi: {remainingTime}");
                                }
                                else
                                {
                                    Debug.LogWarning("Oyun süresi 'sure' alaný beklenmedik türde: " + sureObj.GetType());
                                    remainingTime = 60f; // Varsayýlan 60 saniye
                                    //timerRunning = true;
                                }
                            }
                            else
                            {
                                // Eðer 'sure' alaný yoksa varsayýlan olarak 60 saniye kullanýyoruz
                                remainingTime = 60f;
                                //timerRunning = true;
                                Debug.LogWarning("Oyun süresi 'sure' alaný eksik, varsayýlan 60 saniye kullanýldý.");
                            }
                        }
                        else
                        {
                            Debug.LogError("'oyunAyarlari' alaný beklenen türde deðil. Dictionary<string, object> bekleniyordu.");
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
        catch (System.Exception e)
        {
            Debug.LogError("Oyun ayarlarý alýnamadý: " + e.Message);
        }
    }

    void Update()
    {
        if (timerRunning)
        {
            remainingTime -= Time.deltaTime;  // Zamaný azaltýyoruz
            countdownText.text = Mathf.CeilToInt(remainingTime).ToString();  // Kalan zamaný yazdýrýyoruz

            if (remainingTime <= 0)
            {
                timerRunning = false;  // Zaman sýfýr olduðunda, sayacý durduruyoruz
                ReloadScene();  // Sahneyi yeniden yüklüyoruz
            }
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Mevcut sahneyi yeniden yüklüyoruz
    }
}
