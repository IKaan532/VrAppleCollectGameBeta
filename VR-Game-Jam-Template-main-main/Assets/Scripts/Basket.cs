using UnityEngine;

public class Basket : MonoBehaviour
{
    public HastaVeriYoneticisi hvr;
    private AppleTracker appleTracker;

    void Start()
    {
        appleTracker = FindObjectOfType<AppleTracker>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Apple") || other.CompareTag("BadApple"))
        {
            bool isBad = other.CompareTag("BadApple");
            bool isCorrect = !isBad; 

            appleTracker.RecordApple(hvr.hedefHastaTcKimlikNo , isCorrect,isBad);

            Debug.Log($"[Basket] {(isBad ? "Çürük" : "Normal")} elma {(isCorrect ? "✅ doğru" : "❌ yanlış")} sepete girdi.");
            Debug.Log("+1Puan");

            
        }
    }
}
